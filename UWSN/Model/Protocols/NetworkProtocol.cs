using System.Numerics;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.Model.Protocols;

public class NetworkProtocol : ProtocolBase
{
    public class Neighbour
    {
        public required int Id { get; set; }
        public required Vector3 Position { get; set; }
        public required int? ClusterId { get; set; }
        public required bool? IsReference { get; set; }
    }

    public List<Neighbour> Neighbours { get; set; } = new();

    public List<int> DeadSensors { get; set; } = new();

    public void ReceiveFrame(Frame frame)
    {
        if (frame.Type == Frame.FrameType.Data && frame.ReceiverId == Sensor.Id)
        {
            if (Sensor.IsReference.HasValue && Sensor.IsReference.Value)
            {
                Sensor.ReceivedData.Add(frame.Data!);

                return;
            }

            var newFrame = new Frame
            {
                SenderId = Sensor.Id,
                SenderPosition = Sensor.Position,
                ReceiverId = -1,
                Type = Frame.FrameType.Data,
                TimeSend = Simulation.Instance.Time,
                AckIsNeeded = true,
                NeighboursData = null,
                BatteryLeft = Sensor.Battery,
                DeadSensors = null,
                Data = frame.Data,
            };

            SendFrameWithRouting(newFrame);

            return;
        }

        if (frame.Type == Frame.FrameType.Warning)
        {
            var newDeads =
                frame.DeadSensors ?? throw new NullReferenceException("Неправильный тип данных");

            bool shouldSendWarning = false;
            foreach (var dead in newDeads)
            {
                if (!DeadSensors.Contains(dead))
                {
                    DeadSensors.Add(dead);
                    shouldSendWarning = true;
                }
            }

            if (!shouldSendWarning)
                return;

            var newFrame = new Frame
            {
                SenderId = Sensor.Id,
                SenderPosition = Sensor.Position,
                ReceiverId = -1,
                Type = Frame.FrameType.Warning,
                TimeSend = Simulation.Instance.Time,
                AckIsNeeded = false,
                NeighboursData = Sensor.Network.Neighbours,
                BatteryLeft = Sensor.Battery,
                DeadSensors = Sensor.Network.DeadSensors,
                Data = null,
            };

            Sensor.StopAllAction();
            SendFrameToAll(newFrame);

            Clusterize();

            return;
        }

        if (frame.Type == Frame.FrameType.Hello)
        {
            if (Neighbours.Count == 0)
            {
                Neighbours.Add(
                    new Neighbour
                    {
                        Id = Sensor.Id,
                        Position = Sensor.Position,
                        ClusterId = Sensor.ClusterId,
                        IsReference = Sensor.IsReference
                    }
                );
            }

            var newNeighbours =
                frame.NeighboursData ?? throw new Exception("Неправильный тип данных");

            bool shouldSendToAll = false;
            foreach (var neighbour in newNeighbours)
            {
                if (!Neighbours.Contains(neighbour))
                {
                    Neighbours.Add(neighbour);
                    shouldSendToAll = true;
                }
            }

            if (!shouldSendToAll)
                return;

            var newFrame = new Frame
            {
                SenderId = Sensor.Id,
                SenderPosition = Sensor.Position,
                ReceiverId = -1,
                Type = Frame.FrameType.Hello,
                TimeSend = Simulation.Instance.Time,
                AckIsNeeded = false,
                NeighboursData = Sensor.Network.Neighbours,
                BatteryLeft = Sensor.Battery,
                DeadSensors = null,
                Data = null,
            };

            SendFrameToAll(newFrame);

            if (Neighbours.Count == Simulation.Instance.Environment.Sensors.Count)
            {
                Clusterize();
            }

            return;
        }
    }

    public void StopAllAction() { }

    public void SendFrameWithRouting(Frame frame)
    {
        int hopId = CalculateNextHop();

        if (hopId != -1)
            frame.ReceiverId = hopId;

        Sensor.DataLink.SendFrame(frame);
    }

    public void SendFrameToAll(Frame frame)
    {
        Sensor.DataLink.SendFrame(frame);
    }

    private int CalculateNextHop()
    {
        if (!Sensor.ClusterId.HasValue)
            throw new Exception("Не определена кластеризация для данного сенсора");

        // если сенсор мертв и алгоритм кластеризации присвоил ему специальный кластер для мертвых сенсоров
        if (Sensor.ClusterId == -1)
            return -1;

        var clusterMates = Neighbours.Where(s => s.ClusterId == Sensor.ClusterId).ToList();

        if (clusterMates.Any(m => m.IsReference == null))
            throw new Exception("Свойство IsReference не должно быть null");

        int referenceId = clusterMates.First(m => m.IsReference.HasValue && m.IsReference.Value).Id;

        var referencePosition = Neighbours.First(n => n.Id == referenceId).Position;

        double distanceToReference = Vector3.Distance(Sensor.Position, referencePosition);

        var neighboursByDistance = Neighbours.OrderBy(n =>
            Vector3.Distance(Sensor.Position, n.Position)
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

    private void Clusterize()
    {
        Sensor.Clusterize();

        foreach (var neighbour in Neighbours)
        {
            var sensor = Simulation.Instance.Environment.Sensors.First(s => s.Id == neighbour.Id);

            neighbour.ClusterId = sensor.ClusterId;
            neighbour.IsReference = sensor.IsReference;
        }
    }

    public void SendCollectedData()
    {
        if (Sensor.IsReference == null || Sensor.ClusterId == null)
        {
            throw new Exception(
                "Невозможно отправить данные с датчиков, так как не определена кластеризация"
            );
        }

        if ((bool)Sensor.IsReference)
        {
            Sensor.ReceivedData.Add($"D_{Sensor.Id}");

            return;
        }

        int hopId = CalculateNextHop();
        if (hopId < 0)
            throw new Exception("Не удалось определить, кому отправлять фрейм");

        var frame = new Frame
        {
            SenderId = Sensor.Id,
            SenderPosition = Sensor.Position,
            ReceiverId = hopId,
            Type = Frame.FrameType.Data,
            TimeSend = Simulation.Instance.Time,
            AckIsNeeded = true,
            NeighboursData = null,
            BatteryLeft = Sensor.Battery,
            DeadSensors = null,
            Data = $"D_{Sensor.Id}",
        };

        SendFrameWithRouting(frame);
    }

    public void SendDeathWarning()
    {
        Logger.WriteSensorLine(Sensor, "(Network) начинаю отправку предсмертного фрейма");

        if (!DeadSensors.Contains(Sensor.Id))
        {
            DeadSensors.Add(Sensor.Id);
        }

        var frame = new Frame
        {
            SenderId = Sensor.Id,
            SenderPosition = Sensor.Position,
            ReceiverId = -1,
            Type = Frame.FrameType.Warning,
            TimeSend = Simulation.Instance.Time,
            AckIsNeeded = false,
            NeighboursData = Neighbours,
            BatteryLeft = Sensor.Battery,
            DeadSensors = DeadSensors,
            Data = null,
        };

        SendFrameToAll(frame);
        Clusterize();
    }
}
