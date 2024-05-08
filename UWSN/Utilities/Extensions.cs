using Newtonsoft.Json;

public static class SystemExtension
{
    public static T Clone<T>(this T source)
    {
        var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };

        var serialized = JsonConvert.SerializeObject(source, settings);
        var deserialized = JsonConvert.DeserializeObject<T>(serialized, settings);
        return deserialized ?? throw new NullReferenceException();
    }
}
