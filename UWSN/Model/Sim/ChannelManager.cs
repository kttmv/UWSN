using Newtonsoft.Json;
using UWSN.Utilities;

namespace UWSN.Model.Sim;

public class ChannelManager
{
    /// <summary>
    /// Количество доступных каналов
    /// </summary>
    public int NumberOfChannels { get; set; } = 4;

    /// <summary>
    /// Отсортированные по каналам эммиты
    /// </summary>
    [JsonIgnore]
    private Signal?[] Channels
    {
        get
        {
            // при создании объекта количество каналов не известно,
            // так как десериализация заполняет его свойства ПОСЛЕ его создания.
            // так что приходится создавать массив каналов при первом обращении к нему,
            // когда все уже точно заполнено.
            _channels ??= new Signal?[NumberOfChannels];

            return _channels;
        }
    }

    [JsonIgnore]
    private Signal?[]? _channels;

    [JsonIgnore]
    public List<int> FreeChannels
    {
        get
        {
            var freeChannels = new List<int>();

            foreach (var c in Channels)
            {
                if (c == null)
                {
                    freeChannels.Add(Channels.ToList().IndexOf(c));
                }
            }

            return freeChannels;
        }
    }

    [JsonIgnore]
    public List<int> BusyChannels
    {
        get
        {
            return Channels
                .Select((Signal, Id) => (Signal, Id))
                .Where(x => x.Signal != null)
                .Select(x => x.Id)
                .ToList();
        }
    }

    public bool IsChannelBusy(int channelId)
    {
        return Channels[channelId] != null;
    }

    public void OccupyChannel(int channelId, Signal signal)
    {
        if (Simulation.Instance.SimulationSettings.Verbose)
        {
            Logger.WriteLine(
                $"Менеджер сигналов: Сенсор #{signal.Emitter.Id} занял канал {channelId}",
                false
            );
        }

        // обработка коллизии
        if (Channels[channelId] != null)
        {
            if (Simulation.Instance.SimulationSettings.Verbose)
                Logger.WriteLine($"Обнаружена коллизия на канале {channelId}");

            Channels[channelId]!.DetectCollision();
            signal.DetectCollision();
            return;
        }

        Channels[channelId] = signal;
    }

    public void FreeChannel(int channelId)
    {
        if (Simulation.Instance.SimulationSettings.Verbose)
            Logger.WriteLine($"Менеджер сигналов: Канал {channelId} освобожден");

        Channels[channelId] = null;
    }
}
