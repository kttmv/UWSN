using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWSN.Model.Protocols;
using UWSN.Model.Sim;

namespace UWSN.Model.Protocols;

public class NetworkProtocol : ProtocolBase
{
    public struct Neighbour
    {
        public int Id { get; set; }
        public Vector3 Position { get; set; }

        public Neighbour(int id, Vector3 position)
        {
            Id = id;
            Position = position;
        }
    }

    public List<Neighbour> Neighbours;

    //public int ClusterId;

    public bool IsReference;

    public List<int> DeadSensors;

    public NetworkProtocol()
    {
        Neighbours = new();

        //ClusterId = -1;
        IsReference = false;
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
            };

            Sensor.DataLink.SendFrame(newFrame);

            Sensor.Physical.CurrentState = PhysicalProtocol.State.Idle;
            Sensor.DataLink.StopAllAction();
            Sensor.Physical.ShouldReceiveMessages = false;

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
            };

            Sensor.DataLink.SendFrame(newFrame);

            Sensor.Physical.CurrentState = PhysicalProtocol.State.Idle;
            Sensor.DataLink.StopAllAction();
            Sensor.Physical.ShouldReceiveMessages = false;

            return;
        }

        if (frame.Type == Frame.FrameType.Warning)
        {
            var newDeads = frame.DeadSensors;

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
                };

                Sensor.DataLink.SendFrame(newFrame);

                Sensor.Physical.CurrentState = PhysicalProtocol.State.Idle;
                Sensor.DataLink.StopAllAction();
                Sensor.Physical.ShouldReceiveMessages = false;

                return;
            }   

        }

        if (frame.Type == Frame.FrameType.Hello)
        {
            if (Neighbours.Count == 0)
            {
                Neighbours.Add(new(Sensor.Id, Sensor.Position));
            }

            var newNeighbours = frame.NeighboursData ?? throw new Exception("Неправильный тип");

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
                };

                Sensor.DataLink.SendFrame(newFrame);
            }
        }
    }

    public void SendFrame(Frame frame)
    {
    }
}