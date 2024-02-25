using System.Numerics;
using Newtonsoft.Json;

namespace UWSN.Model
{
    public class Environment
    {
        public Tuple<Vector3, Vector3> AreaLimits { get; set; }

        public List<Sensor> Sensors { get; set; }

        public Environment()
        {
            Sensors = new List<Sensor>();
            AreaLimits = new Tuple<Vector3, Vector3>(new Vector3(), new Vector3());
        }
    }
}