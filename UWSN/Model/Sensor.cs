using System.Numerics;

namespace UWSN.Model
{
    public class Sensor
    {
        public int Id { get; set; }
        public Vector3 Position { get; set; }

        public Sensor(int id)
        {
            Id = id;
            Position = new Vector3();
        }
    }
}