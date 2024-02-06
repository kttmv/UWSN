using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UWSN.Model
{
    public class SensorPlacementFromFile : ISensorPlacementModel
    {
        private List<Sensor> Sensors;
        private string FilePath;

        public List<Sensor> PlaceSensors()
        {
            var lines = File.ReadAllLines(FilePath);

            for (int i = 0; i < lines.Length; i++)
            {
                Sensors.Add(new Sensor(i));
            }

            try
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    var split = lines[i].Split(" ");
                    Sensors[i].Position = new System.Numerics.Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
                }
            }
            catch
            {
                throw new Exception("Не удалось распарсить файл с координатами");
            }

            return Sensors;
        }

        public SensorPlacementFromFile(string filePath) 
        {
            if (File.Exists(filePath))
            {
                FilePath = filePath;
            }
            else
            {
                throw new Exception("Файл не существует");
            }

            Sensors = new List<Sensor>();
        }
    }
}
