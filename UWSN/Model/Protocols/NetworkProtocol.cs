using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWSN.Model.Protocols;
using UWSN.Model.Sim;
using UWSN.Utilities;
using static UWSN.Model.Sim.SimulationDelta;

namespace UWSN.Model.Protocols;

public class NetworkProtocol : ProtocolBase
{
    public struct Neighbour
    {
        public int Id { get; set; }
        public Vector3 Position { get; set; }
        public int? ClusterId { get; set; }
        public bool? IsReference { get; set; }

        public Neighbour(int id, Vector3 position, int? clusterId, bool? isReference)
        {
            Id = id;
            Position = position;
            ClusterId = clusterId;
            IsReference = isReference;
        }
    }

    public List<Neighbour> Neighbours;

    //public int ClusterId;


    public List<int> DeadSensors;

    public NetworkProtocol()
    {
        Neighbours = new();

        //ClusterId = -1;
        DeadSensors = new();
    }

    public void ReceiveFrame(Frame frame)
    {
        if (Sensor.Battery < 5.0)
        {
            if (!DeadSensors.Contains(Sensor.Id))
            {
                DeadSensors.Add(Sensor.Id);
            }

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

            Sensor.DataLink.SendFrame(newFrame);

            StopAction();

            return;
        }

        if (frame.BatteryLeft < 5.0)
        {
            if (!DeadSensors.Contains(frame.SenderId))
            {
                DeadSensors.Add(frame.SenderId);
            }

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

            Sensor.DataLink.SendFrame(newFrame);

            StopAction();

            return;
        }

        if (frame.Type == Frame.FrameType.Data && frame.ReceiverId == Sensor.Id)
        {
            if ((bool)Sensor.IsReference)
            {
                Sensor.ReceivedData.Add(frame.Data);

                return;
            }

            int hopId = Sensor.CalculateNextHop();
            if (hopId < 0)
                throw new Exception("Дефолтный id = -1(ничего не вычислилось)");

            var newFrame = new Frame
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
                Data = frame.Data,
            };

            Sensor.DataLink.SendFrame(newFrame);
        }

        if (frame.Type == Frame.FrameType.Warning)
        {
            var newDeads =
                frame.DeadSensors ?? throw new NullReferenceException("Неправильный тип данных");

            bool shouldSendToAll = false;
            foreach (var dead in newDeads)
            {
                if (!DeadSensors.Contains(dead))
                {
                    DeadSensors.Add(dead);
                    shouldSendToAll = true;
                }
            }

            if (shouldSendToAll)
            {
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

                Sensor.DataLink.SendFrame(newFrame);

                StopAction();

                return;
            }
        }

        if (frame.Type == Frame.FrameType.Hello)
        {
            if (Neighbours.Count == 0)
            {
                Neighbours.Add(new(Sensor.Id, Sensor.Position, Sensor.ClusterId, Sensor.IsReference));
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

            if (shouldSendToAll)
            {
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

                Sensor.DataLink.SendFrame(newFrame);
            }

            if (Neighbours.Count == Simulation.Instance.Environment.Sensors.Count)
            {
                Clusterize();
            }
        }
    }

    private void StopAction()
    {
        Sensor.Physical.CurrentState = PhysicalProtocol.State.Idle;
        Sensor.DataLink.StopAllAction();
        Sensor.Physical.ShouldReceiveMessages = false;

        Clusterize();
    }

    public void SendFrame(Frame frame) { }

    private void Clusterize()
    {
        if (Sensor.NextClusterization == null)
        {
            Simulation.Instance.Clusterize();
        }

        if (Sensor.NextClusterization == null)
        {
            throw new NullReferenceException("Что-то пошло не так в процессе кластеризации");
        }

        Sensor.ClusterId = Sensor.NextClusterization.ClusterId;
        Sensor.IsReference = Sensor.NextClusterization.IsReference;
        Sensor.NextClusterization = null;

        if (Sensor.IsReference.Value)
        {
            Logger.WriteSensorLine(
                Sensor,
                $"(Network) определил себя новым референсным узлом кластера {Sensor.ClusterId}"
            );
        }
        else
        {
            Logger.WriteSensorLine(
                Sensor,
                $"(Network) определил себя к кластеру {Sensor.ClusterId}."
            );
        }

        var time = Simulation.Instance.Time;
        var delta = new ClusterizationDelta(
            Sensor.Id,
            Sensor.ClusterId.Value,
            Sensor.IsReference.Value
        );

        Simulation.Instance.Result!.AllDeltas[time].ClusterizationDeltas.Add(delta);
    }
}
