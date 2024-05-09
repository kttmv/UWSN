﻿using System.Numerics;
using Newtonsoft.Json;
using UWSN.Model.Protocols;
using UWSN.Model.Protocols.DataLink;
using UWSN.Model.Sim;
using UWSN.Utilities;
using static UWSN.Model.Protocols.NetworkProtocol;
using static UWSN.Model.Sim.SimulationDelta;

namespace UWSN.Model;

public class NextClusterization
{
    public int ClusterId { get; set; }
    public bool IsReference { get; set; }
}

public class Sensor
{
    #region Properties

    [JsonIgnore]
    public PhysicalProtocol Physical { get; set; } = new();

    [JsonIgnore]
    public DataLinkProtocol DataLink { get; set; }

    [JsonIgnore]
    public NetworkProtocol Network { get; set; } = new();

    [JsonIgnore]
    private double _battery;

    [JsonIgnore]
    public double Battery
    {
        get { return _battery; }
        set
        {
            var delta = SimulationResult.GetOrCreateSimulationDelta(Simulation.Instance.Time);
            delta.SensorDeltas.Add(
                new SensorDelta
                {
                    Id = Id,
                    ClusterId = null,
                    IsReference = null,
                    Battery = -(_battery - value)
                }
            );

            _battery = value;

            if (!IsDead && _battery < Simulation.Instance.SensorSettings.BatteryDeadCharge)
            {
                Logger.WriteSensorLine(this, "Осталось мало зарядки");
                IsDead = true;

                Network.SendDeathWarning();
                StopAllAction();
            }
        }
    }

    [JsonIgnore]
    public bool IsDead { get; set; }

    /// <summary>
    /// Данные, полученные в ходе обмена данными. Только для референсов. Для проверки
    /// </summary>
    [JsonIgnore]
    public List<string> ReceivedData { get; set; } = new();

    [JsonIgnore]
    public int? ClusterId { get; set; } = null;

    [JsonIgnore]
    public bool? IsReference { get; set; } = null;

    [JsonIgnore]
    public NextClusterization? NextClusterization { get; set; } = null;

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

    #endregion Properties

    public Sensor()
    {
        _battery = Simulation.Instance.SensorSettings.InitialSensorBattery;

        DataLink = Simulation.Instance.SensorSettings.DataLinkProtocol.Clone();
    }

    public void StopAllAction()
    {
        RemoveAllEvents();
        DataLink.StopAllAction();
        Network.StopAllAction();
        Physical.CurrentState = PhysicalProtocol.State.Idle;
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
        ReceivedData.Clear();

        if (!Simulation.Instance.ShouldSkipHello)
        {
            var frame = new Frame
            {
                SenderId = Id,
                SenderPosition = Position,
                ReceiverId = -1,
                Type = Frame.FrameType.Hello,
                TimeSend = Simulation.Instance.Time,
                AckIsNeeded = false,
                NeighboursData = Network.Neighbours,
                BatteryLeft = Battery,
                DeadSensors = null,
                Data = null,
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
            Clusterize();

            List<Neighbour> neighbours = new();

            foreach (var sensor in Simulation.Instance.Environment.Sensors)
            {
                var n = new Neighbour
                {
                    Position = sensor.Position,
                    Id = sensor.Id,
                    ClusterId = sensor.ClusterId ?? sensor.NextClusterization!.ClusterId,
                    IsReference = sensor.IsReference ?? sensor.NextClusterization!.IsReference
                };
                neighbours.Add(n);
            }

            Network.Neighbours = neighbours;
        }

        for (int i = 1; i <= Simulation.MAX_CYCLES; i++)
        {
            int k = i;

            AddEvent(
                new Event(
                    Simulation.Instance.SensorSettings.StartSamplingTime.Add(
                        Simulation.Instance.SensorSettings.SensorSampleInterval * i
                    ),
                    $"Сбор данных с датчиков сенсором #{Id}",
                    () =>
                    {
                        Simulation.Instance.CurrentCycle = k;
                        CollectData();
                    }
                )
            );
        }
    }

    public void CollectData()
    {
        // тут надо выяснить сколько по времени сенсор собирает данные
        Battery -= Simulation.Instance.SensorSettings.Modem.PowerSP * 0.02;

        if (!IsDead)
            Network.SendCollectedData();
    }

    public void Clusterize()
    {
        if (IsDead || NextClusterization == null)
        {
            Simulation.Instance.Clusterize();
        }

        if (NextClusterization == null)
        {
            throw new NullReferenceException("Что-то пошло не так в процессе кластеризации");
        }

        ClusterId = NextClusterization.ClusterId;
        IsReference = NextClusterization.IsReference;
        NextClusterization = null;

        if (IsReference.Value)
        {
            Logger.WriteSensorLine(
                this,
                $"Определил себя новым референсным узлом кластера {ClusterId}"
            );
        }
        else
        {
            Logger.WriteSensorLine(this, $"Определил себя к кластеру {ClusterId}.");
        }

        var delta = SimulationResult.GetOrCreateSimulationDelta(Simulation.Instance.Time);
        delta.SensorDeltas.Add(
            new SensorDelta
            {
                Id = Id,
                ClusterId = ClusterId.Value,
                IsReference = IsReference.Value,
                Battery = null
            }
        );
    }
}