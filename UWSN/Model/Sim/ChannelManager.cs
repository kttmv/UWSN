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
    private Signal?[] Channels { get; set; }

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

    public ChannelManager()
    {
        Channels = new Signal?[NumberOfChannels];
    }

    public bool IsChannelBusy(int channelId)
    {
        return Channels[channelId] != null;
    }

    public void OccupyChannel(int channelId, Signal signal)
    {
        Logger.WriteLine($"Менеджер сигналов: Сенсор #{signal.Emitter.Id} занял канал {channelId}");

        // обработка коллизии
        if (Channels[channelId] != null)
        {
            Logger.WriteLine($"Обнаружена коллизия на канале {channelId}");
            Channels[channelId]!.DetectCollision();
            signal.DetectCollision();
            return;
        }

        Channels[channelId] = signal;
    }

    public void FreeChannel(int channelId)
    {
        Logger.WriteLine($"Менеджер сигналов: Канал {channelId} освобожден");
        Channels[channelId] = null;
    }
}
