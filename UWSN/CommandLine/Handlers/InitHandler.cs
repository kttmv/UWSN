using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWSN.CommandLine.Options;
using UWSN.Model;

namespace UWSN.CommandLine.Handlers;

public class InitHandler
{
    public static void Handle(InitOptions o)
    {
        var areaLimits = o.AreaLimits.ToList();
        var v1 = new Vector3(areaLimits[0], areaLimits[1], areaLimits[2]);
        var v2 = new Vector3(areaLimits[3], areaLimits[4], areaLimits[5]);

        var environment = new Model.Environment(v1, v2, new List<Sensor>());

        Console.WriteLine("Инициализация окружения проведена успешно.");
        Console.WriteLine($"Границы окружения: {v1}, {v2}");

        float length = Math.Abs(v1.X - v2.X);
        float width = Math.Abs(v1.Y - v2.Y);
        float height = Math.Abs(v1.Z - v2.Z);
        float volume = length * width * height;

        Console.WriteLine($"Объем окружения: {volume} м³");

        environment.SaveEnv(o.FilePath);
    }
}