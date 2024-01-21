using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model
{
    public class SensorPlacementOrthogonalGrid : ISensorPlacementModel
    {
        public required List<Sensor> Sensors { get; set; }
        public required float Step { get; set; }

        public List<Sensor> PlaceSensors()
        {
            int placedCount = 0;
            int cubicEdge = (int)(Math.Ceiling(Math.Pow(Sensors.Count, 1 / 3)));

            for (int i = 0; i < cubicEdge; i++)
            {
                for (int j = 0; j < cubicEdge; j++)
                {
                    for (int k = 0; k < cubicEdge; k++)
                    {
                        if (placedCount >= Sensors.Count)
                        {
                            break;
                        }

                        Sensors[placedCount].Position = new System.Numerics.Vector3(i * Step, j * Step, k * Step);
                        placedCount++;
                    }
                }
            }

            return Sensors;
        }

        public SensorPlacementOrthogonalGrid(List<Sensor> sensors, float step)
        {
            Sensors = sensors;
            Step = step;
        }
    }
}
