using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWSN.CommandLine.Options;
using UWSN.Model;

namespace UWSN.CommandLine.Handlers;

public class InitializationHandler
{
    public static void Handle(InitializationOptions o)
    {
        var areaLimits = o.AreaLimits.ToList();
        var v1 = new Vector3(areaLimits[0], areaLimits[1], areaLimits[2]);
        var v2 = new Vector3(areaLimits[3], areaLimits[4], areaLimits[5]);

        //environment.SaveEnv(o.FilePath);

        _ = new Simulation();

        Simulation.Instance.Environment.AreaLimits = new Tuple<Vector3, Vector3>(v1, v2);

        Console.WriteLine($"Границы окружения: {v1}, {v2}");
        Console.WriteLine("Инициализация симуляции проведена успешно.");

        float length = Math.Abs(v1.X - v2.X);
        float width = Math.Abs(v1.Y - v2.Y);
        float height = Math.Abs(v1.Z - v2.Z);
        float volume = length * width * height;

        Console.WriteLine($"Объем окружения: {volume} м³");

        SerializationHelper.SaveSimulation(o.FilePath);
    }
}