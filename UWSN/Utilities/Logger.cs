using UWSN.Model;
using UWSN.Model.Sim;
using System.IO;

namespace UWSN.Utilities;

public class Logger
{
    public static readonly StreamWriter File;
    public static bool ShouldWriteToConsole { get; set; } = false;

    static Logger()
    {
        string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string strWorkPath = Path.GetDirectoryName(strExeFilePath)!;
        string path = Path.Combine(strWorkPath, "output.txt");

        File = new StreamWriter(path);
    }

    public static void WriteLine(string value, bool withTime = false)
    {
        string str = withTime ? $"[{Simulation.Instance.Time:dd.MM.yyyy HH:mm:ss.fff}] " : "";
        str += value;

        File.WriteLine(str);

        if (ShouldWriteToConsole)
            Console.WriteLine(str);
    }

    public static void WriteSensorLine(Sensor sensor, string value)
    {
        WriteLine($"Сенсор #{sensor.Id}: {value}");
    }
}