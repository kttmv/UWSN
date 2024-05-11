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
    private Signal?[] Channels
    {
        get
        {
            // костыль. при создании объекта количество каналов не известно,
            // так как десериализация заполняет его свойства ПОСЛЕ его создания.
            // так что приходится создавать массив каналов при первом обращении к нему,
            // когда все уже точно создано.
            if (_channels == null)
            {
                _channels = new Signal?[NumberOfChannels];
            }

            return _channels;
        }
    }

    private Signal?[]? _channels { get; set; }

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
        Logger.WriteLine(
            $"Менеджер сигналов: Сенсор #{signal.Emitter.Id} занял канал {channelId}",
            false,
            false
        );

        // обработка коллизии
        if (Channels[channelId] != null)
        {
            Logger.WriteLine($"Обнаружена коллизия на канале {channelId}", false, false);
            Channels[channelId]!.DetectCollision();
            signal.DetectCollision();
            return;
        }

        Channels[channelId] = signal;
    }

    public void FreeChannel(int channelId)
    {
        Logger.WriteLine($"Менеджер сигналов: Канал {channelId} освобожден", false, false);
        Channels[channelId] = null;
    }
}
