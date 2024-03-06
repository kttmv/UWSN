using System.Numerics;
using Newtonsoft.Json;
using UWSN.Model.Network;
using UWSN.Model.Protocols.NetworkLayer;

namespace UWSN.Model
{
    public class Sensor
    {
        public PhysicalProtocol PhysicalLayer { get; set; }
        public INetworkLayer NetworkLayer { get; set; }

        public int Id { get; set; }
        public Vector3 Position { get; set; }

        [JsonIgnore]
        public List<Frame> FrameBuffer { get; set; }

        public Sensor(int id)
        {
            Id = id;
            Position = new Vector3();
            PhysicalLayer = new PhysicalProtocol(Id);
            NetworkLayer = new PureAlohaProtocol(Id);
            FrameBuffer = new List<Frame>();
        }
    }
}