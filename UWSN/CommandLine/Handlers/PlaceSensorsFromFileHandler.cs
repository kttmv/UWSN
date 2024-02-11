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
        var loader = new Loader(o.FilePath);
        var environment = loader.LoadEnv();
        environment.Sensors.Clear();

        environment.Sensors = new SensorPlacementFromFile(o.SensorsFilePath).PlaceSensors();
        Console.WriteLine($"Расстановка сенсоров ({environment.Sensors.Count}) из пользовательского файла прошла успешно.");

        environment.SaveEnv(o.FilePath);
    }
}