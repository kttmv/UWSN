using System.Numerics;

namespace UWSN.Model
{
    public class SensorPlacementFromFile : ISensorPlacementModel
    {
        private readonly List<Sensor> _sensors;
        private readonly string _filePath;

        public List<Sensor> PlaceSensors()
        {
            var lines = File.ReadAllLines(_filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                _sensors.Add(new Sensor(i));
            }

            try
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    var split = lines[i].Split(" ");
                    _sensors[i].Position = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
                }
            }
            catch
            {
                throw new Exception("Не удалось распарсить файл с координатами");
            }

            return _sensors;
        }

        public SensorPlacementFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                _filePath = filePath;
            }
            else
            {
                throw new Exception("Файл не существует");
            }

            _sensors = new List<Sensor>();
        }
    }
}