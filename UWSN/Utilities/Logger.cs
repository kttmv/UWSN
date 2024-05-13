using UWSN.Model;
using UWSN.Model.Sim;

namespace UWSN.Utilities;

public class Logger
{
    public const int PADDING_SIZE = 4;

    public static readonly StreamWriter File;
    public static readonly string FilePath;
    public static bool SaveOutput { get; set; } = false;

    public static int LeftPadding { get; set; } = 0;

    static Logger()
    {
        string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string strWorkPath = Path.GetDirectoryName(strExeFilePath)!;
        string path = Path.Combine(strWorkPath, "output.txt");

        File = new StreamWriter(path);
        FilePath = path;
    }

    public static void WriteLine(string value, bool withTime = false)
    {
        string str = withTime ? $"[{Simulation.Instance.Time:dd.MM.yyyy HH:mm:ss.fff}] " : "";
        str += value;

        if (LeftPadding > 0)
        {
            str = new string(' ', LeftPadding * PADDING_SIZE) + str;
        }

        if (SaveOutput)
            File.WriteLine(str);

        Console.WriteLine(str);
    }

    public static void WriteSensorLine(Sensor sensor, string value)
    {
        WriteLine($"Сенсор #{sensor.Id}: {value}");
    }
}