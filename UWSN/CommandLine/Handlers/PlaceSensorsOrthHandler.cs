using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWSN.CommandLine.Options;
using UWSN.Model;

namespace UWSN.CommandLine.Handlers;

public class PlaceSensorsOrthHandler
{
    public static void Handle(PlaceSensorsOrthOptions o)
    {
        var loader = new Loader(o.FilePath);
        var environment = loader.LoadEnv();

        environment.Sensors.Clear();
        for (int i = 0; i < o.SensorsCount; i++)
        {
            environment.Sensors.Add(new Sensor(i));
        }

        environment.Sensors = new SensorPlacementOrthogonalGrid(environment.Sensors, o.OrthogonalStep).PlaceSensors();

        Console.WriteLine($"Расстановка сенсоров ({o.SensorsCount}) на ортогональной сетке прошла успешно.");

        environment.SaveEnv(o.FilePath);
    }
}