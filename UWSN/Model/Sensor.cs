using System.Numerics;

namespace UWSN.Model
{
    public class Sensor
    {
        public PhysicalLayer PhysicalLayer { get; set; }

        public int Id { get; set; }
        public Vector3 Position { get; set; }

        public List<Packet> Buffer { get; set; }

        public Sensor(int id)
        {
            Id = id;
            Position = new Vector3();
            PhysicalLayer = new PhysicalLayer(this);
            Buffer = new List<Packet>();
        }
    }
}