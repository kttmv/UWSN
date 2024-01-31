using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace UWSN.Model
{
    public class Environment
    {
        public required Tuple<Vector3, Vector3> AreaLimits { get; set; }
        
        public required List<Sensor> Sensors { get; set; }

        public required ISensorPlacementModel PlacementType { get; set; }

        public void SaveEnv(string envFilePath)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };

            string json = JsonConvert.SerializeObject(this, settings);
            File.WriteAllText(envFilePath, json);
        }
    }
}
