using System.Numerics;
using Newtonsoft.Json;
using UWSN.Model.Protocols;
using UWSN.Model.Protocols.DataLink;
using UWSN.Model.Sim;
using static UWSN.Model.Protocols.NetworkProtocol;

namespace UWSN.Model;

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
    public double Battery { get; set; } = 100;

    [JsonIgnore]
    public bool IsDead
    {
        get { return Battery < Simulation.Instance.SensorSettings.BatteryDeadCharge; }
    }

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

    #endregion Properties

    public Sensor(int id)
    {
        Battery = Simulation.Instance.SensorSettings.InitialSensorBattery;

        DataLink = Simulation.Instance.SensorSettings.DataLinkProtocol.Clone();

        Id = id;
    }

    public Sensor()
    {
        Battery = Simulation.Instance.SensorSettings.InitialSensorBattery;

        DataLink = Simulation.Instance.SensorSettings.DataLinkProtocol.Clone();
    }

    public void WakeUp(bool shouldSkipHello)
    {
        ReceivedData.Clear();

        if (!shouldSkipHello)
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

            Simulation.Instance.EventManager.AddEvent(
                new Event(
                    default,
                    $"Отправка HELLO от #{frame.SenderId}",
                    () => DataLink.SendFrame(frame)
                )
            );
        }
        else
        {
            List<Neighbour> neighbours = new();

            foreach (var sensor in Simulation.Instance.Environment.Sensors)
            {
                var n = new Neighbour
                {
                    Position = sensor.Position,
                    Id = sensor.Id,
                };
                neighbours.Add(n);
            }

            Network.Neighbours = neighbours;
            Network.Clusterize();
        }
        
        for (int i = 1; i <= Simulation.MAX_CYCLES; i++)
        {
            int k = i;

            Simulation.Instance.EventManager.AddEvent(
                new Event(
                    Simulation.Instance.StartSamplingTime.Add(Simulation.Instance.SensorSampleInterval * i),
                    $"Отправка DATA от #{Id}",
                    () =>
                    {
                        Simulation.Instance.CurrentCycle = k;
                        SendData();
                    }
                )
            );
        }
    }

    public void SendData()
    {
        Battery -= Simulation.Instance.Modem.PowerSP * 0.02;

        if (IsReference == null)
            return;

        if ((bool)IsReference)
        {
            ReceivedData.Add($"D_{Id}");

            return;
        }

        int hopId = CalculateNextHop();

        var frame = new Frame
        {
            SenderId = Id,
            SenderPosition = Position,
            ReceiverId = hopId,
            Type = Frame.FrameType.Data,
            TimeSend = Simulation.Instance.Time,
            AckIsNeeded = true,
            NeighboursData = null,
            BatteryLeft = Battery,
            DeadSensors = null,
            Data = $"D_{Id}",
        };

        if (Id == 9)
        {
            var specialFrame = new Frame
            {
                SenderId = Id,
                SenderPosition = Position,
                ReceiverId = hopId,
                Type = Frame.FrameType.Data,
                TimeSend = Simulation.Instance.Time,
                AckIsNeeded = true,
                NeighboursData = null,
                BatteryLeft = Battery,
                DeadSensors = null,
                Data = $"D_{Id}q",
            };

            DataLink.SendFrame(specialFrame);
            return;
        }

        DataLink.SendFrame(frame);
    }

    public int CalculateNextHop()
    {
        var clusterMates = Simulation.Instance.Environment.Sensors.Where(s => s.ClusterId == ClusterId).ToList();
        if (clusterMates.Any(m => m.IsReference == null))
            throw new Exception("Свойство IsReference не должно быть null");

        int referenceId = clusterMates.First(m => m.IsReference.HasValue && m.IsReference.Value).Id;

        var referencePosition = Network.Neighbours.First(n => n.Id == referenceId).Position;

        double distanceToReference = Vector3.Distance(Position, referencePosition);

        var neighboursByDistance = Network.Neighbours.OrderBy(n =>
            Vector3.Distance(Position, n.Position)
        );

        int hopId = -1;
        foreach (var neighbour in neighboursByDistance)
        {
            if (
                Vector3.Distance(neighbour.Position, referencePosition) < distanceToReference
                && clusterMates.Count(m => m.Id == neighbour.Id) > 0
            )
            {
                hopId = neighbour.Id;
            }
        }

        return hopId;
    }
}

public class NextClusterization
{
    public int ClusterId { get; set; }
    public bool IsReference { get; set; }
}
