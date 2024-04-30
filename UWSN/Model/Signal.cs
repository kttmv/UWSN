using System.Numerics;
using System.Xml.Linq;
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

    public Event? EndSending { get; set; }
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

        var timeEndReceivingMax = default(DateTime);

        int receiversCount = 0;
        foreach (var sensor in Simulation.Instance.Environment.Sensors)
        {
            if (sensor == Emitter)
                continue;

            if (!CheckProbablity(sensor))
                continue;

            receiversCount++;

            double distance = Vector3.Distance(sensor.Position, Emitter.Position);
            var timeStartReceiving = Simulation.Instance.Time.AddSeconds(
                distance / SPEED_OF_SOUND_IN_METERS_PER_SECOND
            );

            var timeEndReceiving = timeStartReceiving.AddSeconds(transmissionTime);

            if (timeEndReceiving > timeEndReceivingMax)
            {
                timeEndReceivingMax = timeEndReceiving;
            }

            // добавление сигнала в результат симуляции
            int id = Simulation.Instance.Result!.AllSignals.Count;
            Simulation.Instance.Result.AllSignals.Add(new(id, Emitter.Id, sensor.Id));

            var delta = GetOrCreateSignalDelta(timeStartReceiving);
            delta.SignalDeltas.Add(new(id, SimulationDelta.SignalDeltaType.Add));

            // создание событий начала и окончания приема сообщения
            var startReceiving = CreateStartReceivingEvent(sensor, timeStartReceiving);
            var endReceiving = CreateEndReceivingEvent(sensor, timeEndReceiving, id);

            ReceivingEvents.Add(new(sensor, startReceiving, endReceiving));
        }

        if (receiversCount == 0)
        {
            Logger.WriteLine(
                $"Менеджер сигналов: Сигнал от #{Emitter.Id} не дошел "
                    + $"ни до одного сенсора. Канал {ChannelId} свободен."
            );

            return;
        }

        // добавляем фрейм в результат симуляции
        Simulation.Instance.Result!.AllFrames.Add(frame);

        CreateEndSendingEvent(transmissionTime);

        DateTime timeFreeChannel = CreateFreeChannelEvent(timeEndReceivingMax);

        Simulation.Instance.ChannelManager.OccupyChannel(ChannelId, this);

        Logger.WriteLine(
            $"Менеджер сигналов: Сигнал от #{Emitter.Id} занял канал {ChannelId}.\n"
                + $"\tКоличество получателей сигнала: {receiversCount}.\n"
                + $"\tКанал будет особожден в {timeFreeChannel:dd.MM.yyyy HH:mm:ss.fff}."
        );
    }

    private DateTime CreateFreeChannelEvent(DateTime timeEndReceivingMax)
    {
        var timeFreeChannel = timeEndReceivingMax;

        Simulation.Instance.EventManager.AddEvent(
            new Event(
                timeFreeChannel,
                $"Удаление сигнала из среды",
                () => Simulation.Instance.ChannelManager.FreeChannel(ChannelId)
            )
        );
        return timeFreeChannel;
    }

    private void CreateEndSendingEvent(double transmissionTime)
    {
        EndSending = new Event(
            Simulation.Instance.Time.AddSeconds(transmissionTime),
            $"Окончание отправки кадра сенсором #{Emitter.Id}",
            () => Emitter.Physical.EndSending(Frame)
        );

        Simulation.Instance.EventManager.AddEvent(EndSending);
    }

    private bool CheckProbablity(Sensor sensor)
    {
        double probability = CalculateDeliveryProbability(sensor);
        double random = new Random().NextDouble();

        return random <= probability;
    }

    private Event CreateStartReceivingEvent(Sensor sensor, DateTime timeStartReceiving)
    {
        var startReceiving = new Event(
            timeStartReceiving,
            $"Начало получения сенсором #{sensor.Id} кадра от #{Emitter.Id}",
            () => StartReceivingAction(sensor)
        );

        Simulation.Instance.EventManager.AddEvent(startReceiving);
        return startReceiving;
    }

    private void StartReceivingAction(Sensor sensor)
    {
        if (sensor.Physical.CurrentState == PhysicalProtocol.State.Listening)
        {
            sensor.Physical.StartReceiving(Frame);
        }
        else
        {
            Logger.WriteLine(
                $"Менеджер сигналов: Сенсор #{sensor.Id} находится не в состоянии прослушивания."
            );
        }
    }

    private Event CreateEndReceivingEvent(Sensor sensor, DateTime timeEndReceiving, int id)
    {
        var endReceiving = new Event(
            timeEndReceiving,
            $"Окончание получения сенсором #{sensor.Id} кадра от #{Emitter.Id}",
            () => EndReceivingAction(sensor, timeEndReceiving, id)
        );

        Simulation.Instance.EventManager.AddEvent(endReceiving);
        return endReceiving;
    }

    private void EndReceivingAction(Sensor sensor, DateTime timeEndReceiving, int id)
    {
        var delta = GetOrCreateSignalDelta(timeEndReceiving);
        delta.SignalDeltas.Add(new(id, SimulationDelta.SignalDeltaType.Remove));

        sensor.Physical.EndReceiving(Frame);
    }

    public void DetectCollision()
    {
        if (EndSending == null)
        {
            throw new NullReferenceException("Что-то пошло не так");
        }

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
        //Name = nameof(SMTUTestModem);
        //CenterFrequency = 26.0;
        //Bandwidth = 16.0;
        //Bitrate = 13900.0;
        //Range = 3.5;
        //PowerTX = 35.0;
        //PowerRX = 0.72;
        //PowerSP = double.NaN;
        //PowerATWU = double.NaN;
        // взяты значения параметров модели среды для тестового моделирования
        return DeliveryProbabilityCalculator.Calculate(
            26.0,
            13.9,
            sensor.Position,
            Emitter.Position,
            35.0
        );

        //return DeliveryProbabilityCalculator.CaulculateSensorDistance(new AquaModem1000(),
        //     new Vector3Range(new Vector3(-10000, -10000, -10000), new Vector3(10000, 10000, 10000)));

        //return DeliveryProbabilityCalculator.CalculatePassiveSonarEq(20.0, 0, 18000);
        //return DeliveryProbabilityCalculator.Calculate(60.0, 12.8, new Vector3(0, 0, 0), new Vector3(0, 0, 2225), 25.0, true);
    }

    private SimulationDelta GetOrCreateSignalDelta(DateTime time)
    {
        if (!Simulation.Instance.Result!.AllDeltas.TryGetValue(time, out SimulationDelta? value))
        {
            value = new();
            Simulation.Instance.Result.AllDeltas.Add(time, value);
        }

        return value;
    }
}
