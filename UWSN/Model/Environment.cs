using System.Numerics;
using Newtonsoft.Json;

namespace UWSN.Model
{
    public class Environment
    {
        public Tuple<Vector3, Vector3> AreaLimits { get; set; }

        public List<Sensor> Sensors { get; set; }

        public Environment(Vector3 v1, Vector3 v2, List<Sensor> sensors)
        {
            AreaLimits = new Tuple<Vector3, Vector3>(v1, v2);
            Sensors = sensors;
        }

        public void SaveEnv(string envFilePath)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };

            string json = JsonConvert.SerializeObject(this, settings);
            File.WriteAllText(envFilePath, json);

            Console.WriteLine($"Файл {envFilePath} успешно сохранен.");
        }
    }
}