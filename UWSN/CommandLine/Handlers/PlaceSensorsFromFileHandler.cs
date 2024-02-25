using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWSN.CommandLine.Options;
using UWSN.Model;

namespace UWSN.CommandLine.Handlers;

public class PlaceSensorsFromFileHandler
{
    public static void Handle(PlaceSensorsFromFileOptions o)
    {
        SerializationHelper.LoadSimulation(o.FilePath);
        var environment = Simulation.Instance.Environment;

        environment.Sensors = new SensorPlacementFromFile(o.SensorsFilePath).PlaceSensors();
        Console.WriteLine($"Расстановка сенсоров ({environment.Sensors.Count}) из пользовательского файла прошла успешно.");

        SerializationHelper.SaveSimulation(o.FilePath);
    }
}