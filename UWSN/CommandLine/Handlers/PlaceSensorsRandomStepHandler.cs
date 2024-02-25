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
        SerializationHelper.LoadSimulation(o.FilePath);
        var sensors = new List<Sensor>();

        for (int i = 0; i < o.SensorsCount; i++)
        {
            sensors.Add(new Sensor(i));
        }

        var placement =
            new SensorPlacementRandomStep(
                sensors,
                o.StepRange,
                o.DistributionType,
                o.UniParameterA,
                o.UniParameterB);

        Simulation.Instance.Environment.Sensors = placement.PlaceSensors();

        string distributionType = o.DistributionType switch
        {
            (DistributionType.Normal) => "нормальному",
            (DistributionType.Uniform) => "непрерывному равномерному",
            _ => throw new NotImplementedException(),
        };

        Console.WriteLine($"Расстановка сенсоров ({o.SensorsCount}) по {distributionType} распределению прошла успешно.");

        SerializationHelper.SaveSimulation(o.FilePath);
    }
}