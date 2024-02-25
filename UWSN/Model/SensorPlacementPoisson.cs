﻿using System.Numerics;
using Dew.Math;

namespace UWSN.Model
{
    /// <summary>
    /// Модель расстановки узлов по закону Пуассона
    /// </summary>
    public class SensorPlacementPoisson : ISensorPlacementModel
    {
        private readonly List<Sensor> _sensors;
        private readonly double _lambdaParameter;
        private readonly Tuple<Vector3, Vector3> _areaLimits;

        public List<Sensor> PlaceSensors()
        {
            var rng = new TRngStream();

            var rand = new Random();

            var bytes = new byte[4];
            rand.NextBytes(bytes);
            uint seed = BitConverter.ToUInt32(bytes);

            rng.NewStream(0, seed);

            //var dst = new TMtxVecInt();
            var dst = new TVecInt
            {
                Length = _sensors.Count * 3
            };

            rng.RandomPoisson(dst, _lambdaParameter);

            int dstIndex = 0;

            for (int i = 0; i < _sensors.Count; i++)
            {
                float x = (float)PoissonDouble(dst.IValues[dstIndex + 0], _areaLimits.Item1.X, _areaLimits.Item2.X);
                float y = (float)PoissonDouble(dst.IValues[dstIndex + 1], _areaLimits.Item1.Y, _areaLimits.Item2.Y);
                float z = (float)PoissonDouble(dst.IValues[dstIndex + 2], _areaLimits.Item1.Z, _areaLimits.Item2.Z);

                _sensors[i].Position = new Vector3(x, y, z);

                dstIndex += 3;
            }

            return _sensors;
        }

        public SensorPlacementPoisson(List<Sensor> sensors, double lambdaParameter, Tuple<Vector3, Vector3> areaLimits)
        {
            _sensors = sensors;
            _lambdaParameter = lambdaParameter;
            _areaLimits = areaLimits;
        }

        private static double PoissonDouble(double uniformValue, double min, double max)
        {
            return min + (uniformValue * (max - min));
        }
    }
}