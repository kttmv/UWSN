using System.Numerics;
using Dew.Math;
using UWSN.CommandLine;

namespace UWSN.Model
{
    public class SensorPlacementRandomStep : ISensorPlacementModel
    {
        private readonly List<Sensor> _sensors;
        private readonly float _stepRange;
        private readonly DistributionType _distrType;
        private readonly double _uniParameterA;
        private readonly double _uniParameterB;

        public List<Sensor> PlaceSensors()
        {
            if (_distrType == DistributionType.Normal)
            {
                var rnd = new Random();

                int placedCount = 0;
                int cubicEdge = (int)(Math.Ceiling(Math.Pow(_sensors.Count, 1.0 / 3.0)));

                for (int i = 0; i < cubicEdge; i++)
                {
                    for (int j = 0; j < cubicEdge; j++)
                    {
                        for (int k = 0; k < cubicEdge; k++)
                        {
                            if (placedCount >= _sensors.Count)
                            {
                                break;
                            }

                            var x = (float)((i * _stepRange) + NextDouble(rnd, -_stepRange / 2, _stepRange / 2));
                            var y = (float)((j * _stepRange) + NextDouble(rnd, -_stepRange / 2, _stepRange / 2));
                            var z = (float)((k * _stepRange) + NextDouble(rnd, -_stepRange / 2, _stepRange / 2));

                            _sensors[placedCount].Position = new Vector3(x, y, z);

                            placedCount++;
                        }
                    }
                }
            }
            if (_distrType == DistributionType.Uniform)
            {
                var rng = new TRngStream();

                var rand = new Random();

                var bytes = new byte[4];
                rand.NextBytes(bytes);
                uint seed = BitConverter.ToUInt32(bytes);

                rng.NewStream(0, seed);

                var dst = new TVec
                {
                    Length = _sensors.Count * 3
                };

                rng.RandomUniform(dst, _uniParameterA, _uniParameterB);

                int placedCount = 0;
                int dstIndx = 0;
                int cubicEdge = (int)(Math.Ceiling(Math.Pow(_sensors.Count, 1.0 / 3.0)));

                for (int i = 0; i < cubicEdge; i++)
                {
                    for (int j = 0; j < cubicEdge; j++)
                    {
                        for (int k = 0; k < cubicEdge; k++)
                        {
                            if (placedCount >= _sensors.Count)
                            {
                                break;
                            }

                            var x = (float)((i * _stepRange) + UniformDouble(dst.Values[dstIndx + 0], -_stepRange / 2, _stepRange / 2));
                            var y = (float)((j * _stepRange) + UniformDouble(dst.Values[dstIndx + 1], -_stepRange / 2, _stepRange / 2));
                            var z = (float)((k * _stepRange) + UniformDouble(dst.Values[dstIndx + 2], -_stepRange / 2, _stepRange / 2));

                            _sensors[placedCount].Position = new Vector3(x, y, z);

                            placedCount++;
                            dstIndx += 3;
                        }
                    }
                }
            }

            return _sensors;
        }

        public SensorPlacementRandomStep(
            List<Sensor> sensors,
            float stepRange,
            DistributionType distrType,
            double uniParameterA = 0,
            double uniParameterB = 1)
        {
            _sensors = sensors;
            _stepRange = stepRange;
            _distrType = distrType;
            _uniParameterA = uniParameterA;
            _uniParameterB = uniParameterB;
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