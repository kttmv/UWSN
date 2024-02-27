using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dew.Math;
using UWSN.CommandLine.Options;
using UWSN.Model;

namespace UWSN.CommandLine.Handlers;

public class PlaceSensorsPoissonHandler
{
    public static void Handle(PlaceSensorsPoissonOptions o)
    {
        SerializationHelper.LoadSimulation(o.FilePath);
        var sensors = new List<Sensor>();

        for (int i = 0; i < o.SensorsCount; i++)
        {
            sensors.Add(new Sensor(i));
        }

        var areaLimits = Simulation.Instance.Environment.AreaLimits;

        Simulation.Instance.Environment.Sensors = PlaceSensors(sensors, o.LambdaParameter, areaLimits);

        Console.WriteLine($"Расстановка сенсоров ({o.SensorsCount}) по закону Пуассона прошла успешно.");

        SerializationHelper.SaveSimulation(o.FilePath);
    }

    private static List<Sensor> PlaceSensors(List<Sensor> sensors, double lambdaParameter, Vector3Range areaLimits)
    {
        var rng = new TRngStream();

        var rand = new Random();

        var bytes = new byte[4];
        rand.NextBytes(bytes);
        uint seed = BitConverter.ToUInt32(bytes);

        rng.NewStream(0, seed);

        var dst = new TVecInt
        {
            Length = sensors.Count * 3
        };

        rng.RandomPoisson(dst, lambdaParameter);

        int dstIndex = 0;

        for (int i = 0; i < sensors.Count; i++)
        {
            float x = (float)PoissonDouble(dst.IValues[dstIndex + 0], areaLimits.Min.X, areaLimits.Max.X);
            float y = (float)PoissonDouble(dst.IValues[dstIndex + 1], areaLimits.Min.Y, areaLimits.Max.Y);
            float z = (float)PoissonDouble(dst.IValues[dstIndex + 2], areaLimits.Min.Z, areaLimits.Max.Z);

            sensors[i].Position = new Vector3(x, y, z);

            dstIndex += 3;
        }

        return sensors;
    }

    private static double PoissonDouble(double uniformValue, double min, double max)
    {
        return min + (uniformValue * (max - min));
    }
}