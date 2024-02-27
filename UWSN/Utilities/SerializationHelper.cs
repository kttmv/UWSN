using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UWSN.Model.Sim;

namespace UWSN.Utilities;

public class SerializationHelper
{
    public static JsonSerializerSettings SerializerSettings
    {
        get
        {
            return new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
        }
    }

    public static void SaveSimulation(string path)
    {
        string json = JsonConvert.SerializeObject(Simulation.Instance, SerializerSettings);
        File.WriteAllText(path, json);

        Console.WriteLine($"Файл {path} успешно сохранен.");
    }

    public static void LoadSimulation(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Не удалось найти указанный файл.");
        }

        using StreamReader reader = new(path);

        var sim = JsonConvert.DeserializeObject<Simulation>(reader.ReadToEnd(), SerializerSettings)
            ?? throw new NullReferenceException("Не удалось прочитать файл");
    }
}