using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWSN.Model.Sim;

namespace UWSN.Utilities;

public class Logger
{
    public static void WriteSimulationLine(string message)
    {
        Console.WriteLine($"[{Simulation.Instance.Time.ToString("dd.MM.yyyy HH:mm:ss.fff")}]: {message}");
    }
}