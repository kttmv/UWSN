using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWSN.CommandLine.Options;
using UWSN.Model;

namespace UWSN.CommandLine.Handlers;

public class PlaceSensorsPoissonHandler
{
    public static void Handle(PlaceSensorsPoissonOptions o)
    {
        var loader = new Loader(o.FilePath);
        var environment = loader.LoadEnv();
        environment.Sensors.Clear();

        for (int i = 0; i < o.SensorsCount; i++)
        {
            environment.Sensors.Add(new Sensor(i));
        }

        Vector3[] areaLimits = new Vector3[2]
        {
            new Vector3(environment.AreaLimits.Item1.X, environment.AreaLimits.Item1.Y, environment.AreaLimits.Item1.Z),
            new Vector3(environment.AreaLimits.Item2.X, environment.AreaLimits.Item2.Y, environment.AreaLimits.Item2.Z)
        };

        environment.Sensors = new SensorPlacementPoisson(environment.Sensors, o.LambdaParameter, areaLimits).PlaceSensors();

        Console.WriteLine($"Расстановка сенсоров ({o.SensorsCount}) по закону Пуассона прошла успешно.");

        environment.SaveEnv(o.FilePath);
    }
}