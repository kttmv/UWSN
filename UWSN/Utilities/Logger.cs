using UWSN.Model;
using UWSN.Model.Sim;

namespace UWSN.Utilities;

public class Logger
{
    private static List<string> Lines { get; set; } = new();

    public static void WriteLine(string value, bool withTime = false)
    {
        string str = withTime ? $"[{Simulation.Instance.Time:dd.MM.yyyy HH:mm:ss.fff}] " : "";

        str += value;

        Console.WriteLine(str);
        Lines.Add(str);
    }

    public static void WriteSensorLine(Sensor sensor, string value)
    {
        WriteLine($"Сенсор #{sensor.Id}: {value}");
    }

    public static void Save()
    {
        string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath)!;
        string path = Path.Combine(strWorkPath, "output.txt");

        var content = string.Join("\n", Lines.ToArray());

        File.WriteAllText(path, content);

        Console.WriteLine($"Вывод программы сохранен в {path}");
    }
}
