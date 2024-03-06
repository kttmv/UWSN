using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWSN.Utilities;

namespace UWSN.Model.Sim;

public class ChannelManager
{
    // TODO: Не использовать дефолтное значение и выставлять его при инициализации (или вместе с протоколами)
    /// <summary>
    /// Количество доступных каналов
    /// </summary>
    public int NumberOfChannels { get; set; } = 1;

    /// <summary>
    /// Отсортированные по каналам эммиты
    /// </summary>
    private Signal?[] Channels { get; set; }

    public List<int> FreeChannels
    {
        get
        {
            return
                Channels
                .Select((Signal, Id) => (Signal, Id))
                .Where(x => x.Signal == null)
                .Select(x => x.Id)
                .ToList();
        }
    }

    public List<int> BusyChannels
    {
        get
        {
            return
                Channels
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
        Logger.WriteLine($"Менеджер сигналов: Канал {channelId} был занят сенсором №{signal.Emitter.Id}");

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