using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWSN.CommandLine.Options;
using UWSN.Model;

namespace UWSN.CommandLine.Handlers;

public class PlaceSensorsRandomStepHandler
{
    public static void Handle(PlaceSensorsRandomStepOptions o)
    {
        var loader = new Loader(o.FilePath);
        var environment = loader.LoadEnv();
        environment.Sensors.Clear();

        for (int i = 0; i < o.SensorsCount; i++)
        {
            environment.Sensors.Add(new Sensor(i));
        }

        var placement = new SensorPlacementRandomStep(
            environment.Sensors,
            o.StepRange,
            o.DistributionType,
            o.UniParameterA,
            o.UniParameterB);

        environment.Sensors = placement.PlaceSensors();

        string distributionType = o.DistributionType switch
        {
            (DistributionType.Normal) => "нормальному",
            (DistributionType.Uniform) => "непрерывному равномерному",
            _ => throw new NotImplementedException(),
        };

        Console.WriteLine($"Расстановка сенсоров ({o.SensorsCount}) по {distributionType} распределению прошла успешно.");

        environment.SaveEnv(o.FilePath);
    }
}