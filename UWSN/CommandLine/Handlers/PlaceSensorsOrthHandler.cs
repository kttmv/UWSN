using UWSN.CommandLine.Options;
using UWSN.Model;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.CommandLine.Handlers;

public class PlaceSensorsOrthHandler
{
    public static void Handle(PlaceSensorsOrthOptions o)
    {
        SerializationHelper.LoadSimulation(o.FilePath);
        var sensors = new List<Sensor>();

        for (int i = 0; i < o.SensorsCount; i++)
        {
            sensors.Add(new Sensor() { Id = i });
        }

        Simulation.Instance.Environment.Sensors = PlaceSensors(sensors, o.OrthogonalStep);

        Console.WriteLine(
            $"Расстановка сенсоров ({o.SensorsCount}) на ортогональной сетке прошла успешно."
        );

        SerializationHelper.SaveSimulation(o.FilePath);
    }

    private static List<Sensor> PlaceSensors(List<Sensor> sensors, float step)
    {
        int placedCount = 0;
        int cubicEdge = (int)Math.Ceiling(Math.Pow(sensors.Count, 1.0 / 3.0));

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

                    sensors[placedCount].Position = new System.Numerics.Vector3(
                        i * step,
                        j * step,
                        k * step
                    );
                    placedCount++;
                }
            }
        }

        return sensors;
    }
}
