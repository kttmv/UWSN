using System.Numerics;
using UWSN.Model.Modems;
using UWSN.Model.Protocols;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.Model;

public class Signal
{
    public const double SPEED_OF_SOUND_IN_METERS_PER_SECOND = 1500;

    public Sensor Emitter { get; }
    private Frame Frame { get; }
    private int ChannelId { get; }

    public Event EndSending { get; set; }
    public List<(
        Sensor Receiver,
        Event StartReceiving,
        Event EndReceiving
    )> ReceivingEvents { get; }

    public Signal(Sensor emitter, Frame frame, int channelId)
    {
        Emitter = emitter;
        Frame = frame;
        ChannelId = channelId;
        ReceivingEvents = new();

        double transmissionTime =
            Frame.FRAME_SIZE_IN_BITS / (Simulation.Instance.Modem.Bitrate * 1024.0);

        EndSending = new Event(
            Simulation.Instance.Time.AddSeconds(transmissionTime),
            $"Окончание отправки кадра сенсором #{Emitter.Id}",
            () => Emitter.Physical.EndSending(Frame)
        );

        var timeEndReceivingMax = default(DateTime);

        int receiversCount = 0;
        foreach (var sensor in Simulation.Instance.Environment.Sensors)
        {
            if (sensor == Emitter)
                continue;

            double probability = CalculateDeliveryProbability(sensor);
            double random = new Random().NextDouble();

            if (random > probability)
            {
                continue;
            }

            double distance = Vector3.Distance(sensor.Position, Emitter.Position);
            var timeStartReceiving = Simulation.Instance.Time.AddSeconds(
                distance / SPEED_OF_SOUND_IN_METERS_PER_SECOND
            );
            var timeEndReceiving = timeStartReceiving.AddSeconds(transmissionTime);

            if (timeEndReceiving > timeEndReceivingMax)
            {
                timeEndReceivingMax = timeEndReceiving;
            }

            var startReceiving = new Event(
                timeStartReceiving,
                $"Начало получения сенсором #{sensor.Id} кадра от #{Emitter.Id}",
                () =>
                {
                    if (sensor.Physical.CurrentState == PhysicalProtocol.State.Listening)
                        sensor.Physical.StartReceiving(Frame);
                    else
                        Logger.WriteLine(
                            $"Менеджер сигналов: Сенсор #{sensor.Id} находится не в состоянии прослушивания."
                        );
                }
            );

            int id = Simulation.Instance.Result!.AllSignals.Count;
            Simulation.Instance.Result.AllSignals.Add(new(id, Emitter.Id, sensor.Id));

            var delta = GetOrCreateSignalDelta(timeStartReceiving);
            delta.SignalDeltas.Add(new(id, SimulationDelta.SignalDeltaType.Add));

            var endReceiving = new Event(
                timeEndReceiving,
                $"Окончание получения сенсором #{sensor.Id} кадра от #{Emitter.Id}",
                () =>
                {
                    var delta = GetOrCreateSignalDelta(timeEndReceiving);
                    delta.SignalDeltas.Add(new(id, SimulationDelta.SignalDeltaType.Remove));

                    sensor.Physical.EndReceiving(Frame);
                }
            );

            ReceivingEvents.Add(new(sensor, startReceiving, endReceiving));
            Simulation.Instance.EventManager.AddEvent(startReceiving);
            Simulation.Instance.EventManager.AddEvent(endReceiving);

            receiversCount++;
        }

        if (receiversCount == 0)
        {
            Logger.WriteLine(
                $"Менеджер сигналов: Сигнал от #{Emitter.Id} не дошел ни до одного сенсора. Канал {ChannelId} свободен."
            );
            return;
        }

        Simulation.Instance.Result!.AllFrames.Add(frame);

        Simulation.Instance.EventManager.AddEvent(EndSending);

        var timeFreeChannel = timeEndReceivingMax;

        // TODO: уточнить когда освобождать
        // возможно когда сигнал выходит за границы окружения
        Simulation.Instance.EventManager.AddEvent(
            new Event(
                timeFreeChannel,
                $"Удаление сигнала из среды",
                () => Simulation.Instance.ChannelManager.FreeChannel(ChannelId)
            )
        );

        Simulation.Instance.ChannelManager.OccupyChannel(ChannelId, this);
        Logger.WriteLine(
            $"Менеджер сигналов: Сигнал от #{Emitter.Id} занял канал {ChannelId}.\n"
                + $"\tКоличество получателей сигнала: {receiversCount}.\n"
                + $"\tКанал будет особожден в {timeFreeChannel:dd.MM.yyyy HH:mm:ss.fff}."
        );
    }

    public void DetectCollision()
    {
        Simulation.Instance.EventManager.RemoveEvent(EndSending);
        Emitter.Physical.DetectCollision();

        foreach (var (Receiver, StartReceiving, EndReceiving) in ReceivingEvents)
        {
            Simulation.Instance.EventManager.AddEvent(
                new Event(
                    StartReceiving.Time.AddMilliseconds(1),
                    $"Обнаружение коллизии сенсором {Receiver.Id}",
                    Receiver.Physical.DetectCollision
                )
            );

            Simulation.Instance.EventManager.RemoveEvent(EndReceiving);
        }

        Simulation.Instance.Result!.TotalCollisions += 1;
    }

    private double CalculateDeliveryProbability(Sensor sensor)
    {
        //return 1.0;

        // взяты значения параметров модели среды для тестового моделирования
        return DeliveryProbabilityCalculator.Calculate(
            60.0,
            12.8,
            sensor.Position,
            Emitter.Position,
            25.0
        );

        //return DeliveryProbabilityCalculator.CaulculateSensorDistance(new AquaModem1000(),
        //     new Vector3Range(new Vector3(-10000, -10000, -10000), new Vector3(10000, 10000, 10000)));

        //return DeliveryProbabilityCalculator.CalculatePassiveSonarEq(20.0, 0, 18000);
        //return DeliveryProbabilityCalculator.Calculate(60.0, 12.8, new Vector3(0, 0, 0), new Vector3(0, 0, 2225), 25.0, true);
    }

    private SimulationDelta GetOrCreateSignalDelta(DateTime time)
    {
        if (!Simulation.Instance.Result!.Deltas.TryGetValue(time, out SimulationDelta? value))
        {
            value = new();
            Simulation.Instance.Result.Deltas.Add(time, value);
        }

        return value;
    }
}
