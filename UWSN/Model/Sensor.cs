﻿using System.Numerics;
using Newtonsoft.Json;
using UWSN.Model.Protocols;
using UWSN.Model.Protocols.DataLink;
using UWSN.Model.Protocols.Network;
using UWSN.Model.Sim;
using UWSN.Utilities;
using static UWSN.Model.Sim.SimulationDelta;

namespace UWSN.Model;

public class Sensor
{
    #region Nested

    public enum State
    {
        Idle,
        Listening,
        Receiving,
        Emitting
    }

    public class Neighbour
    {
        public required int Id { get; set; }
        public required Vector3 Position { get; set; }
        public required int? ClusterId { get; set; }
        public required bool? IsReference { get; set; }
    }

    public class Clusterization
    {
        public int ClusterId { get; set; }
        public bool IsReference { get; set; }
    }

    #endregion Nested

    #region Properties

    [JsonIgnore]
    private State _currentState;

    [JsonIgnore]
    public State CurrentState
    {
        get { return _currentState; }
        set
        {
            _currentState = value;
            Simulation.Instance.Result!.AddSensorDelta(
                new SensorDelta { Id = Id, State = value },
                false
            );
        }
    }

    [JsonIgnore]
    public PhysicalProtocol Physical { get; set; } = new();

    [JsonIgnore]
    public DataLinkProtocol DataLink { get; set; }

    [JsonIgnore]
    public NetworkProtocol Network { get; set; }

    [JsonIgnore]
    private double _battery;

    [JsonIgnore]
    public double Battery
    {
        get { return _battery; }
        set
        {
            Simulation.Instance.Result!.AddSensorDelta(
                new SensorDelta { Id = Id, Battery = _battery },
                false
            );

            _battery = value;

            if (!IsDead && _battery < Simulation.Instance.SensorSettings.BatteryDeadCharge)
            {
                if (Simulation.Instance.SimulationSettings.Verbose)
                    Logger.WriteSensorLine(this, "Осталось мало зарядки");

                IsDead = true;

                // храним количество мертвых сенсоров в симуляции, так как это значительно
                // ускоряет симуляцию. раньше после каждого ивента считалось количество мертвых
                // сенсоров, но это очень медленно.
                Simulation.Instance.DeadSensorsCount += 1;

                Network.SendDeathWarning();
            }
        }
    }

    [JsonIgnore]
    public bool IsDead { get; set; }

    /// <summary>
    /// Данные, полученные в ходе обмена данными. Только для референсов.
    /// </summary>
    [JsonIgnore]
    public List<CollectedData> ReceivedData { get; set; } = new();

    [JsonIgnore]
    public int? ClusterId { get; set; } = null;

    [JsonIgnore]
    public bool? IsReference { get; set; } = null;

    [JsonIgnore]
    public Clusterization? NextClusterization { get; set; } = null;

    private int _id;

    public int Id
    {
        get { return _id; }
        set
        {
            _id = value;
            // костыль. при десериализации всегда вызывается конструктор
            // без параметров (Sensor()), после чего заполняются все его
            // свойства. получается, что на момент создания объекта мы не
            // знаем какой у него id, поэтому приходится делать так
            Physical.SensorId = Id;
            DataLink.SensorId = Id;
            Network.SensorId = Id;
        }
    }

    public Vector3 Position { get; set; } = new();

    [JsonIgnore]
    public List<Frame> FrameBuffer { get; set; } = new List<Frame>();

    [JsonIgnore]
    public List<Event> Events { get; set; } = new();

    [JsonIgnore]
    public Dictionary<int, Neighbour> Neighbours { get; set; } = new();

    [JsonIgnore]
    public List<int> DeadSensors { get; set; } = new();

    #endregion Properties

    public Sensor()
    {
        _battery = Simulation.Instance.SensorSettings.InitialSensorBattery;

        DataLink = Simulation.Instance.SensorSettings.DataLinkProtocol.Clone();
        Network = Simulation.Instance.SensorSettings.NetworkProtocol.Clone();
    }

    public void StopAllAction()
    {
        RemoveAllEvents();
        Physical.StopAllAction();
        DataLink.StopAllAction();
        Network.StopAllAction();
    }

    public void AddEvent(Event e)
    {
        // создаем ивент, который будет оболочкой, которая будет удалять себя сама при исполнении
        // чтобы удалить ивент нужна ссылка на него. поэтому, чтобы получить ссылку на объект ивента,
        // мы создаем его с дефолтным пустым действием, а затем меняем это действие на нормальное.
        var wrapperEvent = new Event(
            e.Time,
            e.Description,
            () =>
            {
                throw new Exception("Действие события не задано");
            }
        );

        void action()
        {
            Events.Remove(wrapperEvent);
            e.Invoke();
        }

        wrapperEvent.SetAction(action);

        Events.Add(wrapperEvent);

        Simulation.Instance.EventManager.AddEvent(wrapperEvent);
    }

    public void RemoveEvent(Event e)
    {
        Events.Remove(e);
        Simulation.Instance.EventManager.RemoveEvent(e);
    }

    public void RemoveAllEvents()
    {
        while (Events.Count > 0)
        {
            RemoveEvent(Events.First());
        }
    }

    public void WakeUp()
    {
        CurrentState = State.Listening;
        ReceivedData.Clear();

        if (!Simulation.Instance.SimulationSettings.ShouldSkipHello)
        {
            var frame = new Frame
            {
                SenderId = Id,
                SenderPosition = Position,
                ReceiverId = -1,
                Type = Frame.FrameType.Hello,
                TimeSend = Simulation.Instance.Time,
                AckIsNeeded = false,
                NeighboursData = Neighbours,
                BatteryLeft = Battery,
                DeadSensors = null,
                CollectedData = null,
            };

            AddEvent(
                new Event(
                    default,
                    $"Отправка HELLO от #{frame.SenderId}",
                    () => DataLink.SendFrame(frame)
                )
            );
        }
        else
        {
            Neighbours = Clusterize();
        }
    }

    public void CollectData()
    {
        CurrentState = State.Listening;

        ReceivedData.Clear();

        // todo: тут надо выяснить сколько по времени сенсор собирает данные
        Battery -= Simulation.Instance.SensorSettings.Modem.PowerSP * 0.02;

        if (!IsDead)
        {
            var data = new CollectedData
            {
                SensorId = Id,
                CycleId = Simulation.Instance.CurrentCycle
            };
            Network.SendCollectedData(data);
        }
    }

    public Dictionary<int, Neighbour> Clusterize()
    {
        if (IsDead || NextClusterization == null)
        {
            Simulation.Instance.Clusterize();
        }

        if (NextClusterization == null)
        {
            throw new NullReferenceException("Что-то пошло не так в процессе кластеризации");
        }

        var neighbours = new Dictionary<int, Neighbour>();
        foreach (var sensor in Simulation.Instance.Environment.Sensors)
        {
            var neighbour = new Neighbour
            {
                Id = sensor.Id,
                Position = sensor.Position,
                ClusterId =
                    sensor.NextClusterization != null
                        ? sensor.NextClusterization!.ClusterId
                        : sensor.ClusterId,
                IsReference =
                    sensor.NextClusterization != null
                        ? sensor.NextClusterization!.IsReference
                        : sensor.IsReference
            };
            neighbours.Add(neighbour.Id, neighbour);
        }

        ClusterId = NextClusterization.ClusterId;
        IsReference = NextClusterization.IsReference;
        NextClusterization = null;

        Simulation.Instance.Result!.AddSensorDelta(
            new SensorDelta
            {
                Id = Id,
                ClusterId = ClusterId.Value,
                IsReference = IsReference.Value,
            },
            true
        );

        if (IsReference.Value)
        {
            if (Simulation.Instance.SimulationSettings.Verbose)
            {
                Logger.WriteSensorLine(
                    this,
                    $"Определил себя новым референсным узлом кластера {ClusterId}"
                );
            }
        }
        else
        {
            if (Simulation.Instance.SimulationSettings.Verbose)
            {
                Logger.WriteSensorLine(this, $"Определил себя к кластеру {ClusterId}.");
            }
        }

        return neighbours;
    }
}