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
        SerializationHelper.LoadSimulation(o.FilePath);
        var sensors = new List<Sensor>();

        for (int i = 0; i < o.SensorsCount; i++)
        {
            sensors.Add(new Sensor(i));
        }

        var areaLimits = Simulation.Instance.Environment.AreaLimits;

        Simulation.Instance.Environment.Sensors =
            new SensorPlacementPoisson(sensors, o.LambdaParameter, areaLimits).PlaceSensors();

        Console.WriteLine($"Расстановка сенсоров ({o.SensorsCount}) по закону Пуассона прошла успешно.");

        SerializationHelper.SaveSimulation(o.FilePath);
    }
}