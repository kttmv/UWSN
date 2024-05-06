using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.Model.Clusterization
{
    public class RetardedClusterization : IClusterization
    {
        public int NumberOfClusters { get; set; }

        public List<Sensor> Clusterize()
        {
            if (NumberOfClusters <= 0)
            {
                throw new ArgumentException("Количество кластеров должно быть больше нуля");
            }

            if (NumberOfClusters % 2 == 1)
            {
                throw new ArgumentException("Количество кластеров есть чотное число");
            }

            var areaLimits = Simulation.Instance.AreaLimits;
            var sensors = Simulation.Instance.Environment.Sensors;

            double areaLength = areaLimits.Max.X - areaLimits.Min.X;
            double areaWidth = areaLimits.Max.Z - areaLimits.Min.Z;

            // эта единица на конце - по сути сколько угодно малое число еписилон,
            // чтобы сенсоры, расположенные на границах, не образовывали новый кластер
            double clusterLength = areaLength / (NumberOfClusters / 2) + 1;
            double clusterWidth = areaWidth / (NumberOfClusters / 2) + 1;

            var clusterIdEncoder = new List<string>();

            foreach (var sensor in sensors)
            {
                // уравнение по n
                // areaLimits.Min.X + clusterLength * n = sensor.Position.x
                // n = (sensor.position.x - areaLimints.Min.X) div ClusterLength

                int idClusterX = (int)
                    Math.Floor((sensor.Position.X - areaLimits.Min.X) / clusterLength);
                int idClusterZ = (int)
                    Math.Floor((sensor.Position.Z - areaLimits.Min.Z) / clusterWidth);

                string clusterIdEncoding = idClusterX + "_" + idClusterZ;

                if (!clusterIdEncoder.Contains(clusterIdEncoding))
                {
                    clusterIdEncoder.Add(clusterIdEncoding);
                }

                int clusterId = clusterIdEncoder.IndexOf(clusterIdEncoding);

                sensor.NextClusterization = new() { ClusterId = clusterId };
            }

            var groups = sensors.GroupBy(s => s.NextClusterization!.ClusterId);
            foreach (var gr in groups)
            {
                gr.OrderBy(s => s.Position.Y).Last().NextClusterization!.IsReference = true;
            }

            return sensors;
        }
    }
}
