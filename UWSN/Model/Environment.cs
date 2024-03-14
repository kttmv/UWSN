using System.Numerics;
using Newtonsoft.Json;
using UWSN.Utilities;

namespace UWSN.Model
{
    public class Environment
    {
        public List<Sensor> Sensors { get; set; }

        public Environment()
        {
            Sensors = new List<Sensor>();
        }
    }
}
