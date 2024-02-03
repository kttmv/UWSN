using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dew.Math;

namespace UWSN.Model
{
    public class SensorPlacementRandomStep : ISensorPlacementModel
    {
        private List<Sensor> Sensors { get; set; }
        private float StepRange { get; set; }
        private string DistrType { get; set; }
        private double UniParameterA { get; set; }
        private double UniParameterB { get; set; }

        public List<Sensor> PlaceSensors()
        {
            if (DistrType == "Normal")
            {
                Random rnd = new Random();

                int placedCount = 0;
                int cubicEdge = (int)(Math.Ceiling(Math.Pow(Sensors.Count, 1.0 / 3.0)));

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

                            var x = (float)((i * StepRange) + NextDouble(rnd, -StepRange / 2, StepRange / 2));
                            var y = (float)((j * StepRange) + NextDouble(rnd, -StepRange / 2, StepRange / 2));
                            var z = (float)((k * StepRange) + NextDouble(rnd, -StepRange / 2, StepRange / 2));

                            Sensors[placedCount].Position = new Vector3(x, y, z);
                            
                            placedCount++;
                        }
                    }
                }
            }
            if (DistrType == "Uniform")
            {
                TRngStream rng = new TRngStream();
                rng.NewStream(0, 0);
                
                var dst = new TVec();

                dst.Length = Sensors.Count * 3;
                rng.RandomUniform(dst, UniParameterA, UniParameterB);

                int placedCount = 0;
                int dstIndx = 0;
                int cubicEdge = (int)(Math.Ceiling(Math.Pow(Sensors.Count, 1.0 / 3.0)));

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

                            var x = (float)((i * StepRange) + UniformDouble(dst.Values[dstIndx + 0], -StepRange / 2, StepRange / 2));
                            var y = (float)((j * StepRange) + UniformDouble(dst.Values[dstIndx + 1], -StepRange / 2, StepRange / 2));
                            var z = (float)((k * StepRange) + UniformDouble(dst.Values[dstIndx + 2], -StepRange / 2, StepRange / 2));

                            Sensors[placedCount].Position = new Vector3(x, y, z);
                            placedCount++;
                            dstIndx += 3;
                        }
                    }
                }
            }

            return Sensors;
        }

        public SensorPlacementRandomStep(
            List<Sensor> sensors, 
            float stepRange, 
            string distrType, 
            double uniParameterA = 0, 
            double uniParameterB = 1) 
        {
            Sensors = sensors;
            StepRange = stepRange;
            if (distrType != "Normal" && distrType != "Uniform")
            {
                throw new Exception("Неверное распределение");
            }
            else
            {
                DistrType = distrType;
                UniParameterA = uniParameterA;
                UniParameterB = uniParameterB;
            }
        }

        private static double NextDouble(Random rnd, double min, double max)
        {
            return min + (rnd.NextDouble() * (max - min));
        }

        private static double UniformDouble(double uniformValue, double min, double max)
        {
            return min + (uniformValue * (max - min));
        }
    }
}
