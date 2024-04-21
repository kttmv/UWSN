using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWSN.Model.Protocols;

namespace UWSN.Model.Protocols;

public class NetworkProtocol : ProtocolBase
{
    public List<(int Id, Vector3 Position)> Neighbours;

    public NetworkProtocol()
    {
        Neighbours = new();
    }

    public void ReceiveFrame(Frame frame)
    {
        if (frame.Type == Frame.FrameType.Hello)
        {
            Neighbours.Add(new(frame.SenderId, frame.SenderPosition));
        }
    }
}