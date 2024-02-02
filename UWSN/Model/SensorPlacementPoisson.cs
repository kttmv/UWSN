using Dew.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model
{
    /// <summary>
    /// Модель расстановки узлов по закону Пуассона
    /// </summary>
    public class SensorPlacementPoisson : ISensorPlacementModel
    {
        private List<Sensor> Sensors { get; set; }
        private double LambdaParameter { get; set; }
        private Vector3[] AreaLimits { get; set; }

        public List<Sensor> PlaceSensors()
        {
            TRngStream rng = new TRngStream();

            var rand = new Random();

            var bytes = new byte[4];
            rand.NextBytes(bytes);
            uint seed = BitConverter.ToUInt32(bytes);

            rng.NewStream(0, seed);

            //var dst = new TMtxVecInt();
            var dst = new TVecInt();
            dst.Length = Sensors.Count * 3;

            rng.RandomPoisson(dst, LambdaParameter);

            int dstIndex = 0;

            for (int i = 0; i < Sensors.Count; i++)
            {
                float x = (float)PoissonDouble(dst.IValues[dstIndex + 0], AreaLimits[0].X, AreaLimits[1].X);
                float y = (float)PoissonDouble(dst.IValues[dstIndex + 1], AreaLimits[0].Y, AreaLimits[1].Y);
                float z = (float)PoissonDouble(dst.IValues[dstIndex + 2], AreaLimits[0].Z, AreaLimits[1].Z);

                Sensors[i].Position = new Vector3(x, y, z);

                dstIndex += 3;
            }

            return Sensors;
        }

        public SensorPlacementPoisson(List<Sensor> sensors, double lambdaParameter, Vector3[] areaLimits)
        {
            Sensors = sensors;
            LambdaParameter = lambdaParameter;
            AreaLimits = areaLimits;
        }

        private static double PoissonDouble(double uniformValue, double min, double max)
        {
            return min + (uniformValue * (max - min));
        }
    }
}
