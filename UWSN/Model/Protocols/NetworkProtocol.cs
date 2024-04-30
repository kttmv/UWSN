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

    public NetworkProtocol()
    {
        Neighbours = new();
        //ClusterId = -1;
        IsReference = false;
    }

    public void ReceiveFrame(Frame frame)
    {
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
                    BatteryLeft = Sensor.Battery
                };

                Sensor.DataLink.SendFrame(newFrame);
            }
        }
    }

    public void SendFrame(Frame frame)
    {
    }
}