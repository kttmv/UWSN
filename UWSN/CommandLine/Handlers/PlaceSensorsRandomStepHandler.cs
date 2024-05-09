using System.Numerics;
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

        Simulation.Instance.Result = null;

        var sensors = new List<Sensor>();

        int sensorsCount = o.Count_X * o.Count_Y * o.Count_Z;

        for (int i = 0; i < sensorsCount; i++)
        {
            sensors.Add(new Sensor() { Id = i });
        }

        Simulation.Instance.Environment.Sensors = PlaceSensors(
            sensors,
            o.Count_X,
            o.Count_Y,
            o.Count_Z,
            o.DistributionType,
            o.UniParameterA,
            o.UniParameterB
        );

        SerializationHelper.SaveSimulation(o.FilePath);
    }

    private static List<Sensor> PlaceSensors(
        List<Sensor> sensors,
        int countX,
        int countY,
        int countZ,
        DistributionType distrType,
        double uniParameterA = 0,
        double uniParameterB = 1
    )
    {
        switch (distrType)
        {
            case DistributionType.Normal:
                PlaceNormal(sensors, countX, countY, countZ);
                break;

            case DistributionType.Uniform:
                //PlaceUniform(sensors, countX, countY, countZ, uniParameterA, uniParameterB);
                break;

            default:
                throw new NotImplementedException();
        }

        return sensors;
    }

    private static List<Sensor> PlaceNormal(
        List<Sensor> sensors,
        int countX,
        int countY,
        int countZ
    )
    {
        var al = Simulation.Instance.AreaLimits;

        double stepRangeX = (al.Max.X - al.Min.X) / countX;
        double stepRangeY = (al.Max.Y - al.Min.Y) / countY;
        double stepRangeZ = (al.Max.Z - al.Min.Z) / countZ;

        var rnd = new Random();

        int placedCount = 0;
        //int cubicEdge = (int)(Math.Ceiling(Math.Pow(sensors.Count, 1.0 / 3.0)));

        for (int i = 0; i < countX; i++)
        {
            for (int j = 0; j < countY; j++)
            {
                for (int k = 0; k < countZ; k++)
                {
                    //if (placedCount >= sensors.Count)
                    //{
                    //    break;
                    //}

                    var x = (float)(
                        al.Min.X
                        + stepRangeX / 2
                        + (i * stepRangeX)
                        + NextDouble(rnd, -stepRangeX / 2, stepRangeX / 2)
                    );
                    var y = (float)(
                        al.Min.Y
                        + stepRangeY / 2
                        + (j * stepRangeY)
                        + NextDouble(rnd, -stepRangeY / 2, stepRangeY / 2)
                    );
                    var z = (float)(
                        al.Min.Z
                        + stepRangeZ / 2
                        + (k * stepRangeZ)
                        + NextDouble(rnd, -stepRangeZ / 2, stepRangeZ / 2)
                    );

                    sensors[placedCount].Position = new Vector3(x, y, z);

                    placedCount++;
                }
            }
        }

        Console.WriteLine(
            $"Расстановка сенсоров ({countX * countY * countZ}) по нормальному распределению прошла успешно."
        );

        return sensors;
    }

    private static List<Sensor> PlaceUniform(
        List<Sensor> sensors,
        float stepRange,
        double uniParameterA = 0,
        double uniParameterB = 1
    )
    {
        var rng = new TRngStream();

        var rand = new Random();

        var bytes = new byte[4];
        rand.NextBytes(bytes);
        uint seed = BitConverter.ToUInt32(bytes);

        rng.NewStream(0, seed);

        var dst = new TVec { Length = sensors.Count * 3 };

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

                    var x = (float)(
                        (i * stepRange)
                        + UniformDouble(dst.Values[dstIndx + 0], -stepRange / 2, stepRange / 2)
                    );
                    var y = (float)(
                        (j * stepRange)
                        + UniformDouble(dst.Values[dstIndx + 1], -stepRange / 2, stepRange / 2)
                    );
                    var z = (float)(
                        (k * stepRange)
                        + UniformDouble(dst.Values[dstIndx + 2], -stepRange / 2, stepRange / 2)
                    );

                    sensors[placedCount].Position = new Vector3(x, y, z);

                    placedCount++;
                    dstIndx += 3;
                }
            }
        }

        Console.WriteLine(
            $"Расстановка сенсоров ({sensors.Count}) по непрерывному распределению прошла успешно."
        );

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
