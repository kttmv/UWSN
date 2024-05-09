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

    public static DateTime RoundUpToNearest(this DateTime dateTime, TimeSpan timeSpan)
    {
        var ticksInDateTime = dateTime.Ticks;
        var ticksInTimeSpan = timeSpan.Ticks;
        var remainderTicks = ticksInDateTime % ticksInTimeSpan;
        var shouldRoundUp = remainderTicks > 0 ? 1 : 0;
        return new DateTime((ticksInDateTime / ticksInTimeSpan + shouldRoundUp) * ticksInTimeSpan);
    }

    public static IList<T> Shuffle<T>(this IList<T> list)
    {
        var rng = new Random();
        var copy = new List<T>(list);

        int n = copy.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = copy[k];
            copy[k] = copy[n];
            copy[n] = value;
        }

        return copy;
    }
}
