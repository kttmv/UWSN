using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model;

public class Logger
{
    public static void WriteSimulationLine(string message)
    {
        Console.WriteLine($"[{Simulation.Instance.Time.ToString("dd.MM.yyyy HH:mm:ss.fff")}]: {message}");
    }
}