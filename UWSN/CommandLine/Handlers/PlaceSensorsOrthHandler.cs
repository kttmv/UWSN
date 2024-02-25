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
        SerializationHelper.LoadSimulation(o.FilePath);
        var sensors = new List<Sensor>();

        for (int i = 0; i < o.SensorsCount; i++)
        {
            sensors.Add(new Sensor(i));
        }

        Simulation.Instance.Environment.Sensors =
            new SensorPlacementOrthogonalGrid(sensors, o.OrthogonalStep).PlaceSensors();

        Console.WriteLine($"Расстановка сенсоров ({o.SensorsCount}) на ортогональной сетке прошла успешно.");

        SerializationHelper.SaveSimulation(o.FilePath);
    }
}