using System;
using System.Numerics;
using System.Threading.Channels;

namespace UWSN.Model;

public class Signal
{
    private Sensor Emitter { get; }
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

        // событие окончания отправки кадра
        var timeEndSending = Simulation.Instance.Time.AddSeconds(transmissionTime);
        EndSending = new Event(timeEndSending, new Action(() =>
        {
            Emitter.PhysicalLayer.EndSending(Frame);
        }));

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

            // событие начала получения сенсором кадра
            var startReceiving = new Event(timeStartReceiving, new Action(() =>
            {
                if (sensor.PhysicalLayer.CurrentState == PhysicalLayer.State.Listening)
                    sensor.PhysicalLayer.StartReceiving(Frame);
            }));

            // событие окончания получения сенсором кадра
            var endReceiving = new Event(timeEndReceiving, new Action(() =>
            {
                sensor.PhysicalLayer.EndReceiving(Frame);
            }));

            ReceivingEvents.Add(new(sensor, startReceiving, endReceiving));
            Simulation.Instance.AddEvent(startReceiving);
            Simulation.Instance.AddEvent(endReceiving);
        }

        Simulation.Instance.AddEvent(EndSending);

        // TODO: уточнить когда освобождать
        // возможно когда сигнал выходит за границы окружения?
        var timeSignalRemoval = timeEndReceivingMax.AddSeconds(1);
        var signalRemoval = new Event(timeSignalRemoval, new Action(() =>
        {
            Simulation.Instance.ChannelManager.FreeChannel(ChannelId);
        }));

        Simulation.Instance.AddEvent(signalRemoval);
    }

    public void Emit()
    {
        Simulation.Instance.ChannelManager.OccupyChannel(ChannelId, this);
    }

    public void DetectCollision()
    {
        Simulation.Instance.RemoveEvent(EndSending);
        Emitter.PhysicalLayer.DetectCollision();

        foreach (var tuple in ReceivingEvents)
        {
            var collisionDetectTime = tuple.StartReceiving.Time.AddMilliseconds(1);
            var collisionDetectEvent = new Event(collisionDetectTime, new Action(() =>
            {
                tuple.Receiver.PhysicalLayer.DetectCollision();
            }));

            Simulation.Instance.AddEvent(collisionDetectEvent);

            Simulation.Instance.RemoveEvent(tuple.EndReceiving);
        }
    }

    private double CalculateDeliveryProbability(Sensor sensor)
    {
        double distance = Vector3.Distance(sensor.Position, Emitter.Position);
        // здесь будет вычисление по формулам
        return 1;
    }
}