using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model
{
    public class PureAlohaProtocol : INetworkLayer
    {
        [JsonIgnore]
        public Sensor Sensor { get; set; }

        private Event? AckTimeoutEvent { get; set; }

        public void ReceiveFrame(Frame frame, Sensor sensor)
        {
            Sensor = sensor;

            if (frame.FrameType == Frame.Type.Ack && frame.IdReceive == Sensor.Id)
            {
                AckTimeoutEvent = null;
                Console.WriteLine("Долбаёб №" + Sensor.Id + $" получил ack от {frame.IdSend} (NL)");

                return;
            }

            if (frame.IdReceive == Sensor.Id)
            {
                var ack = new Frame
                {
                    FrameType = Frame.Type.Ack,
                    IdSend = Sensor.Id,
                    IdReceive = frame.IdSend
                };

                Sensor.PhysicalLayer.SendFrame(ack);
            }
        }

        public void SendFrame(Frame frame, Sensor sensor)
        {
            Sensor = sensor;

            // если канал занят, то ждем 5.2с
            if (Simulation.Instance.ChannelSortedEmits[0] != null)
            {
                var time = frame.TimeSend.AddSeconds(5.2);
                var action = new Action(() =>
                {
                    SendFrame(frame, Sensor);
                });

                var e = new Event(time, action);
                Simulation.Instance.AddEvent(e);

                return;
            }

            // если канал свободен, то отправляем кадр и ждем аск в течение 6.1с
            Sensor.PhysicalLayer.SendFrame(frame);

            CreateAckTimeout(frame, 3);
        }

        private void ResendFrame(Frame frame, Sensor sensor, int attemptsLeft)
        {
            if (attemptsLeft == 0)
            {
                return;
            }

            SendFrame(frame, sensor);

            CreateAckTimeout(frame, attemptsLeft - 1);
        }

        private void CreateAckTimeout(Frame frame, int attemptsLeft)
        {
            var time1 = Simulation.Instance.Time.AddSeconds(6.1);
            var action1 = new Action(() =>
            {
                if (AckTimeoutEvent == null)
                {
                    return;
                }

                ResendFrame(frame, Sensor, attemptsLeft);
            });

            var e1 = new Event(time1, action1);
            Simulation.Instance.AddEvent(e1);
            AckTimeoutEvent = e1;
        }

        public PureAlohaProtocol(Sensor sensor)
        {
            Sensor = sensor;
            AckTimeoutEvent = null;
        }
    }
}
