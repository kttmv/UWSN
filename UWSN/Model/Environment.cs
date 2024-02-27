using System.Numerics;
using Newtonsoft.Json;

namespace UWSN.Model
{
    public struct Vector3Range
    {
        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }

        public Vector3Range(Vector3 min, Vector3 max)
        {
            Min = min; Max = max;
        }
    }

    public class Environment
    {
        public Vector3Range AreaLimits { get; set; }

        public List<Sensor> Sensors { get; set; }

        public Environment()
        {
            Sensors = new List<Sensor>();
            AreaLimits = new Vector3Range(new Vector3(), new Vector3());
        }
    }
}