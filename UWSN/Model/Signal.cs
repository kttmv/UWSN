using System;
using System.Numerics;
using System.Threading.Channels;
using UWSN.Model.Network;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.Model;

public class Signal
{
    public Sensor Emitter { get; }
    private Frame Frame { get; }
    private int ChannelId { get; }

    public Event EndSending { get; set; }
    public List<(Sensor Receiver, Event StartReceiving, Event EndReceiving)> ReceivingEvents { get; }

    public Signal(Sensor emitter, Frame frame, int channelId)
    {
        Emitter = emitter;
        Frame = frame;
        ChannelId = channelId;
        ReceivingEvents = new();

        double transmissionTime = 1; // TODO: добавить вычисление времени передачи

        EndSending = new Event(
            Simulation.Instance.Time.AddSeconds(transmissionTime),
            $"Окончание отправки кадра сенсором #{Emitter.Id}",
            () => Emitter.PhysicalLayer.EndSending(Frame));

        var timeEndReceivingMax = default(DateTime);

        foreach (var sensor in Simulation.Instance.Environment.Sensors)
        {
            if (sensor == Emitter || new Random().NextDouble() > CalculateDeliveryProbability(sensor))
                continue;

            double distance = Vector3.Distance(sensor.Position, Emitter.Position);
            var timeStartReceiving = Simulation.Instance.Time.AddSeconds(distance / 1500); // TODO: уточнить скорость звука
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
                    if (sensor.PhysicalLayer.CurrentState == PhysicalProtocol.State.Listening)
                        sensor.PhysicalLayer.StartReceiving(Frame);
                    else
                        Logger.WriteLine($"Менеджер сигналов: Сенсор #{sensor.Id} находится не в состоянии прослушивания.");
                });

            var endReceiving = new Event(
                timeEndReceiving,
                $"Окончание получения сенсором #{sensor.Id} кадра от #{Emitter.Id}",
                () => sensor.PhysicalLayer.EndReceiving(Frame));

            ReceivingEvents.Add(new(sensor, startReceiving, endReceiving));
            Simulation.Instance.EventManager.AddEvent(startReceiving);
            Simulation.Instance.EventManager.AddEvent(endReceiving);
        }

        Simulation.Instance.EventManager.AddEvent(EndSending);

        // TODO: уточнить когда освобождать
        // возможно когда сигнал выходит за границы окружения?
        Simulation.Instance.EventManager.AddEvent(new Event(
            timeEndReceivingMax.AddSeconds(1),
            $"Удаление сигнала из среды",
            () => Simulation.Instance.ChannelManager.FreeChannel(ChannelId)));
    }

    public void Emit()
    {
        Simulation.Instance.ChannelManager.OccupyChannel(ChannelId, this);
    }

    public void DetectCollision()
    {
        Simulation.Instance.EventManager.RemoveEvent(EndSending);
        Emitter.PhysicalLayer.DetectCollision();

        foreach (var (Receiver, StartReceiving, EndReceiving) in ReceivingEvents)
        {
            Simulation.Instance.EventManager.AddEvent(new Event(
                StartReceiving.Time.AddMilliseconds(1),
                $"Обнаружение коллизии сенсором {Receiver.Id}",
                Receiver.PhysicalLayer.DetectCollision));

            Simulation.Instance.EventManager.RemoveEvent(EndReceiving);
        }
    }

    private double CalculateDeliveryProbability(Sensor sensor)
    {
        return 1;

        // взяты значения параметров модели среды для тестового моделирования
        return DeliveryProbabilityCalculator.Calculate(60.0, 12.8, sensor.Position, Emitter.Position, 25.0);
    }
}