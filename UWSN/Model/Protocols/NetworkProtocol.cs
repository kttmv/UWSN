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
    public List<(int Id, Vector3 Position)> Neighbours;
    
    public int ClusterId;

    public bool IsReference;

    public NetworkProtocol()
    {
        Neighbours = new();
        ClusterId = -1;
        IsReference = false;
    }

    public void ReceiveFrame(Frame frame)
    {
        if (frame.Type == Frame.FrameType.Hello)
        {
            if (Neighbours.Count == 0) 
            {
                Neighbours.Add((Sensor.Id, Sensor.Position));
            }

            var newNeighbours = frame.Data as List<(int Id, Vector3 Position)>;

            if (newNeighbours == null)
            {
                throw new Exception("Неправильный тип");
            }

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
                    ReceiverId = 0,
                    Type = Frame.FrameType.Hello,
                    TimeSend = Simulation.Instance.Time,
                    AckIsNeeded = false,
                    Data = Sensor.Network.Neighbours
                };

                Sensor.DataLink.SendFrame(newFrame);
            }
        }
    }

    public void SendFrame(Frame frame)
    {
        
    }
}