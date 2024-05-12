using System.Numerics;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.Model.Protocols.Network;

public class BasicNetworProtocol : NetworkProtocol
{
    public int ResendWarningCount { get; set; } = 1;

    public override void ReceiveFrame(Frame frame)
    {
        if (frame.Type == Frame.FrameType.Data && frame.ReceiverId == Sensor.Id)
        {
            if (Sensor.IsReference.HasValue && Sensor.IsReference.Value)
            {
                Sensor.ReceivedData.Add(frame.CollectedData!);

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
                CollectedData = frame.CollectedData,
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
                if (!Sensor.DeadSensors.Contains(dead))
                {
                    Sensor.DeadSensors.Add(dead);
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
                NeighboursData = Sensor.Neighbours,
                BatteryLeft = Sensor.Battery,
                DeadSensors = Sensor.DeadSensors,
                CollectedData = null,
            };

            Sensor.StopAllAction();
            SendFrameToAll(newFrame);

            CreateResendWarningEvents(newFrame);

            Clusterize();

            return;
        }

        if (frame.Type == Frame.FrameType.Hello)
        {
            if (Sensor.Neighbours.Count == 0)
            {
                Sensor.Neighbours.Add(
                    Sensor.Id,
                    new Sensor.Neighbour
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
                if (!Sensor.Neighbours.ContainsKey(neighbour.Key))
                {
                    Sensor.Neighbours.Add(
                        neighbour.Key,
                        new Sensor.Neighbour
                        {
                            Id = neighbour.Value.Id,
                            Position = neighbour.Value.Position,
                            ClusterId = neighbour.Value.ClusterId,
                            IsReference = neighbour.Value.IsReference
                        }
                    );
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
                NeighboursData = Sensor.Neighbours,
                BatteryLeft = Sensor.Battery,
                DeadSensors = null,
                CollectedData = null,
            };

            SendFrameToAll(newFrame);

            if (Sensor.Neighbours.Count == Simulation.Instance.Environment.Sensors.Count)
            {
                Clusterize();
            }

            return;
        }
    }

    public override void SendFrame(Frame frame)
    {
        Sensor.DataLink.SendFrame(frame);
    }

    public override void StopAllAction() { }

    public void SendFrameWithRouting(Frame frame)
    {
        int hopId = CalculateNextHop();

        if (hopId != -1)
            frame.ReceiverId = hopId;

        Sensor.DataLink.SendFrame(frame);
    }

    public void SendFrameToAll(Frame frame)
    {
        SendFrame(frame);
    }

    private int CalculateNextHop()
    {
        if (!Sensor.ClusterId.HasValue)
            throw new Exception("Не определена кластеризация для данного сенсора");

        // если сенсор мертв и алгоритм кластеризации присвоил ему специальный кластер для мертвых сенсоров
        if (Sensor.ClusterId == -1)
            return -1;

        var clusterMates = Sensor
            .Neighbours.Where(s => s.Value.ClusterId == Sensor.ClusterId)
            .ToList();

        if (clusterMates.Any(m => m.Value.IsReference == null))
            throw new Exception("Свойство IsReference не должно быть null");

        int referenceId = clusterMates
            .First(m => m.Value.IsReference.HasValue && m.Value.IsReference.Value)
            .Value.Id;

        var referencePosition = Sensor
            .Neighbours.First(n => n.Value.Id == referenceId)
            .Value.Position;

        double distanceToReference = Vector3.Distance(Sensor.Position, referencePosition);

        var neighboursByDistance = Sensor.Neighbours.OrderBy(n =>
            Vector3.Distance(Sensor.Position, n.Value.Position)
        );

        int hopId = -1;
        foreach (var neighbour in neighboursByDistance)
        {
            if (
                Vector3.Distance(neighbour.Value.Position, referencePosition) < distanceToReference
                && clusterMates.Any(m => m.Value.Id == neighbour.Value.Id)
            )
            {
                hopId = neighbour.Value.Id;
            }
        }

        return hopId;
    }

    private void Clusterize()
    {
        Sensor.Neighbours = Sensor.Clusterize();
    }

    public override void SendCollectedData(CollectedData data)
    {
        if (Sensor.IsReference == null || Sensor.ClusterId == null)
        {
            throw new Exception(
                "Невозможно отправить данные с датчиков, так как не определена кластеризация"
            );
        }

        if ((bool)Sensor.IsReference)
        {
            Sensor.ReceivedData.Add(
                new CollectedData
                {
                    SensorId = Sensor.Id,
                    CycleId = Simulation.Instance.CurrentCycle
                }
            );

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
            CollectedData = data
        };

        SendFrameWithRouting(frame);
    }

    public override void SendDeathWarning()
    {
        if (Simulation.Instance.SimulationSettings.Verbose)
            Logger.WriteSensorLine(Sensor, "(Network) начинаю отправку предсмертного фрейма");

        if (!Sensor.DeadSensors.Contains(Sensor.Id))
        {
            Sensor.DeadSensors.Add(Sensor.Id);
        }

        var frame = new Frame
        {
            SenderId = Sensor.Id,
            SenderPosition = Sensor.Position,
            ReceiverId = -1,
            Type = Frame.FrameType.Warning,
            TimeSend = Simulation.Instance.Time,
            AckIsNeeded = true,
            NeighboursData = Sensor.Neighbours,
            BatteryLeft = Sensor.Battery,
            DeadSensors = Sensor.DeadSensors,
            CollectedData = null,
        };

        SendFrameToAll(frame);
        Clusterize();
    }

    private void CreateResendWarningEvents(Frame newFrame)
    {
        for (int i = 0; i < ResendWarningCount; i++)
        {
            var time = Simulation.Instance.Time.AddSeconds(10 * i);
            Sensor.AddEvent(
                new Event(
                    time,
                    "Повторная отправка фрейма с предупреждением о смерти",
                    () => SendFrameToAll(newFrame)
                )
            );
        }
    }
}
