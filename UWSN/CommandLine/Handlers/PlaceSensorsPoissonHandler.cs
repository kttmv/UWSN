using System.Numerics;
using Dew.Math;
using UWSN.CommandLine.Options;
using UWSN.Model;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.CommandLine.Handlers;

public class PlaceSensorsPoissonHandler
{
    public static void Handle(PlaceSensorsPoissonOptions o)
    {
        SerializationHelper.LoadSimulation(o.FilePath);
        var sensors = new List<Sensor>();

        for (int i = 0; i < o.SensorsCount; i++)
        {
            sensors.Add(new Sensor() { Id = i });
        }

        var areaLimits = Simulation.Instance.AreaLimits;

        Simulation.Instance.Environment.Sensors = PlaceSensors(sensors, areaLimits);

        Console.WriteLine(
            $"Расстановка сенсоров ({o.SensorsCount}) по закону Пуассона прошла успешно."
        );

        SerializationHelper.SaveSimulation(o.FilePath);
    }

    private static List<Sensor> PlaceSensors(List<Sensor> sensors, Vector3Range areaLimits)
    {
        int d = 3;
        int n = 50;
        double seed = 0.5;
        double g = Phi(d);
        double[] alpha = new double[d];
        double[,] R = new double[n, d];

        for (int j = 0; j < d; j++)
        {
            alpha[j] = Math.Pow(1 / g, j + 1);
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < d; j++)
            {
                R[i, j] = (seed + alpha[j] * (i + 1));
            }
        }

        R = MinMaxStandardize(R);

        var xVals = new List<double>();
        for (int i = 0; i < R.GetLength(0); i++)
        {
            xVals.Add(R[i, 0]);
        }
        var yVals = new List<double>();
        for (int i = 0; i < R.GetLength(0); i++)
        {
            yVals.Add(R[i, 1]);
        }
        var zVals = new List<double>();
        for (int i = 0; i < R.GetLength(0); i++)
        {
            zVals.Add(R[i, 2]);
        }

        Random rnd = new Random();

        for (int i = 0; i < sensors.Count; i++)
        {
            int xInd = (int)rnd.NextInt64(0, n - i - 1);
            int yInd = (int)rnd.NextInt64(0, n - i - 1);
            int zInd = (int)rnd.NextInt64(0, n - i - 1);

            float x = (float)PoissonDouble(xVals[xInd], areaLimits.Min.X, areaLimits.Max.X);
            float y = (float)PoissonDouble(yVals[yInd], areaLimits.Min.Y, areaLimits.Max.Y);
            float z = (float)PoissonDouble(zVals[zInd], areaLimits.Min.Z, areaLimits.Max.Z);

            xVals.RemoveAt(xInd);
            yVals.RemoveAt(yInd);
            zVals.RemoveAt(zInd);

            sensors[i].Position = new Vector3(x, y, z);
        }

        return sensors;
    }

    private static List<Sensor> PlaceSensors(
        List<Sensor> sensors,
        double lambdaParameter,
        Vector3Range areaLimits
    )
    {
        var rng = new TRngStream();

        var rand = new Random();

        var bytes = new byte[4];
        rand.NextBytes(bytes);
        uint seed = BitConverter.ToUInt32(bytes);

        rng.NewStream(0, seed);

        var dst = new TVecInt { Length = sensors.Count * 3 };

        rng.RandomPoisson(dst, lambdaParameter);

        int dstIndex = 0;

        for (int i = 0; i < sensors.Count; i++)
        {
            float x = (float)PoissonDouble(
                dst.IValues[dstIndex + 0],
                areaLimits.Min.X,
                areaLimits.Max.X
            );
            float y = (float)PoissonDouble(
                dst.IValues[dstIndex + 1],
                areaLimits.Min.Y,
                areaLimits.Max.Y
            );
            float z = (float)PoissonDouble(
                dst.IValues[dstIndex + 2],
                areaLimits.Min.Z,
                areaLimits.Max.Z
            );

            sensors[i].Position = new Vector3(x, y, z);

            dstIndex += 3;
        }

        return sensors;
    }

    private static double PoissonDouble(double uniformValue, double min, double max)
    {
        return min + (uniformValue * (max - min));
    }

    private static double Phi(int d)
    {
        double x = 2.0000;
        for (int i = 0; i < 10; i++)
        {
            x = Math.Pow(1 + x, 1 / (d + 1.0));
        }
        return x;
    }

    /// <summary>
    /// Алгоритм минимаксной стандартизации от 0 до 1 двумерного массива координат сенсоров
    /// </summary>
    /// <param name="R">Массив координат</param>
    /// <returns></returns>
    private static double[,] MinMaxStandardize(double[,] R)
    {
        double min = double.MaxValue;
        double max = double.MinValue;

        // Находим минимальное и максимальное значения в массиве R
        for (int i = 0; i < R.GetLength(0); i++)
        {
            for (int j = 0; j < R.GetLength(1); j++)
            {
                min = Math.Min(min, R[i, j]);
                max = Math.Max(max, R[i, j]);
            }
        }

        // Производим стандартизацию чисел от 0 до 1
        for (int i = 0; i < R.GetLength(0); i++)
        {
            for (int j = 0; j < R.GetLength(1); j++)
            {
                R[i, j] = (R[i, j] - min) / (max - min);
            }
        }

        return R;
    }
}
