using System.Numerics;
using UWSN.CommandLine.Options;
using UWSN.Model;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.CommandLine.Handlers;

public class PlaceSensorsFromFileHandler
{
    public static void Handle(PlaceSensorsFromFileOptions o)
    {
        SerializationHelper.LoadSimulation(o.FilePath);
        var environment = Simulation.Instance.Environment;

        environment.Sensors = PlaceSensors(o.SensorsFilePath);
        Console.WriteLine(
            $"Расстановка сенсоров ({environment.Sensors.Count}) "
                + $"из пользовательского файла прошла успешно."
        );

        SerializationHelper.SaveSimulation(o.FilePath);
    }

    private static List<Sensor> PlaceSensors(string filePath)
    {
        var lines = File.ReadAllLines(filePath);

        var sensors = new List<Sensor>();

        for (int i = 0; i < lines.Length; i++)
        {
            sensors.Add(new Sensor() { Id = i });
        }

        try
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var split = lines[i].Split(" ");
                sensors[i].Position = new Vector3(
                    float.Parse(split[0]),
                    float.Parse(split[1]),
                    float.Parse(split[2])
                );
            }
        }
        catch
        {
            throw new Exception("Не удалось распарсить файл с координатами");
        }

        return sensors;
    }
}
