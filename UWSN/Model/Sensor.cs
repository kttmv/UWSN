using System.Numerics;

namespace UWSN.Model
{
    public class Sensor
    {
        public PhysicalLayer PhysicalLayer { get; set; }
        public INetworkLayer NetworkLayer { get; set; }

        public int Id { get; set; }
        public Vector3 Position { get; set; }

        public List<Frame> Buffer { get; set; }

        public Sensor(int id)
        {
            Id = id;
            Position = new Vector3();
            PhysicalLayer = new PhysicalLayer(this);
            NetworkLayer = new PureAlohaProtocol(this);
            Buffer = new List<Frame>();
        }
    }
}