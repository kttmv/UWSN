using Newtonsoft.Json;
using UWSN.Model.Sim;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace UWSN.Utilities;

public class SerializationHelper
{
    private static JsonSerializer Serializer
    {
        get
        {
            var serializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
            serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            return serializer;
        }
    }

    public static void SaveSimulation(string path)
    {
        using (var sw = new StreamWriter(path))
        using (var writer = new JsonTextWriter(sw))
        {
            Serializer.Serialize(writer, Simulation.Instance);
        }

        Console.WriteLine($"Файл {path} успешно сохранен.");
    }

    public static void LoadSimulation(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Не удалось найти указанный файл.");
        }

        using (var sr = new StreamReader(path))
        using (var reader = new JsonTextReader(sr))
        {
            Serializer.Deserialize<Simulation>(reader);
        }
    }
}
