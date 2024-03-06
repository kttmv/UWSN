using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWSN.Model;
using UWSN.Model.Sim;

namespace UWSN.Utilities;

public class Logger
{
    public static void WriteLine(string value, bool withTime = false)
    {
        string str = withTime ? $"[{Simulation.Instance.Time.ToString("dd.MM.yyyy HH:mm:ss.fff")}] " : "";

        str += value;

        Console.WriteLine(str);
    }

    public static void WriteSensorLine(Sensor sensor, string value)
    {
        WriteLine($"Сенсор №{sensor.Id}: {value}");
    }
}