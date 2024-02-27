using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dew.Math;
using UWSN.CommandLine.Options;
using UWSN.Model;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.CommandLine.Handlers;

public class PlaceSensorsRandomStepHandler
{
    public static void Handle(PlaceSensorsRandomStepOptions o)
    {
        SerializationHelper.LoadSimulation(o.FilePath);
        var sensors = new List<Sensor>();

        for (int i = 0; i < o.SensorsCount; i++)
        {
            sensors.Add(new Sensor(i));
        }

        Simulation.Instance.Environment.Sensors =
            PlaceSensors(sensors,
                o.StepRange,
                o.DistributionType,
                o.UniParameterA,
                o.UniParameterB);

        string distributionTypeString = o.DistributionType switch
        {
            (DistributionType.Normal) => "нормальному",
            (DistributionType.Uniform) => "непрерывному равномерному",
            _ => throw new NotImplementedException(),
        };

        Console.WriteLine($"Расстановка сенсоров ({o.SensorsCount}) по {distributionTypeString} распределению прошла успешно.");

        SerializationHelper.SaveSimulation(o.FilePath);
    }

    private static List<Sensor> PlaceSensors(
        List<Sensor> sensors,
        float stepRange,
        DistributionType distrType,
        double uniParameterA = 0,
        double uniParameterB = 1)
    {
        switch (distrType)
        {
            case DistributionType.Normal:
                PlaceNormal(sensors, stepRange);
                break;

            case DistributionType.Uniform:
                PlaceUniform(sensors, stepRange, uniParameterA, uniParameterB);
                break;

            default:
                throw new NotImplementedException();
        }

        return sensors;
    }

    private static List<Sensor> PlaceNormal(List<Sensor> sensors, float stepRange)
    {
        var rnd = new Random();

        int placedCount = 0;
        int cubicEdge = (int)(Math.Ceiling(Math.Pow(sensors.Count, 1.0 / 3.0)));

        for (int i = 0; i < cubicEdge; i++)
        {
            for (int j = 0; j < cubicEdge; j++)
            {
                for (int k = 0; k < cubicEdge; k++)
                {
                    if (placedCount >= sensors.Count)
                    {
                        break;
                    }

                    var x = (float)((i * stepRange) + NextDouble(rnd, -stepRange / 2, stepRange / 2));
                    var y = (float)((j * stepRange) + NextDouble(rnd, -stepRange / 2, stepRange / 2));
                    var z = (float)((k * stepRange) + NextDouble(rnd, -stepRange / 2, stepRange / 2));

                    sensors[placedCount].Position = new Vector3(x, y, z);

                    placedCount++;
                }
            }
        }

        return sensors;
    }

    private static List<Sensor> PlaceUniform(
        List<Sensor> sensors,
        float stepRange,
        double uniParameterA = 0,
        double uniParameterB = 1)
    {
        var rng = new TRngStream();

        var rand = new Random();

        var bytes = new byte[4];
        rand.NextBytes(bytes);
        uint seed = BitConverter.ToUInt32(bytes);

        rng.NewStream(0, seed);

        var dst = new TVec
        {
            Length = sensors.Count * 3
        };

        rng.RandomUniform(dst, uniParameterA, uniParameterB);

        int placedCount = 0;
        int dstIndx = 0;
        int cubicEdge = (int)(Math.Ceiling(Math.Pow(sensors.Count, 1.0 / 3.0)));

        for (int i = 0; i < cubicEdge; i++)
        {
            for (int j = 0; j < cubicEdge; j++)
            {
                for (int k = 0; k < cubicEdge; k++)
                {
                    if (placedCount >= sensors.Count)
                    {
                        break;
                    }

                    var x = (float)((i * stepRange) + UniformDouble(dst.Values[dstIndx + 0], -stepRange / 2, stepRange / 2));
                    var y = (float)((j * stepRange) + UniformDouble(dst.Values[dstIndx + 1], -stepRange / 2, stepRange / 2));
                    var z = (float)((k * stepRange) + UniformDouble(dst.Values[dstIndx + 2], -stepRange / 2, stepRange / 2));

                    sensors[placedCount].Position = new Vector3(x, y, z);

                    placedCount++;
                    dstIndx += 3;
                }
            }
        }

        return sensors;
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