using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UWSN.Model.Network;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.Model.Protocols.NetworkLayer
{
    public class PureAlohaProtocol : ProtocolBase, INetworkLayer
    {
        private const int CHANNEL_ID = 0;
        private const int CHANNEL_TIMEOUT_IN_SECONDS = 5;
        private const int ACK_TIMEOUT_IN_SECONDS = 10;

        [JsonIgnore]
        private bool WaitingForAck { get; set; }

        public PureAlohaProtocol(int id)
        {
            SensorId = id;
        }

        public void ReceiveFrame(Frame frame)
        {
            if (frame.FrameType == Frame.Type.Ack && frame.IdReceive == Sensor.Id)
            {
                WaitingForAck = false;
                Sensor.PhysicalLayer.CurrentState = PhysicalProtocol.State.Idle;
                Logger.WriteSensorLine(Sensor, $"(PureAloha) получил пакет ACK от №{frame.IdSend}");

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

                Logger.WriteSensorLine(Sensor, $"(PureAloha) начал отправку ACK cенсору №{frame.IdReceive}");
                Sensor.NetworkLayer.SendFrame(ack);
            }
        }

        public void SendFrame(Frame frame)
        {
            Logger.WriteSensorLine(Sensor, $"(PureAloha) пытается отправить кадр сенсору №{frame.IdReceive}");

            // если канал занят, то ждем и повторяем попытку
            if (Simulation.Instance.ChannelManager.IsChannelBusy(CHANNEL_ID))
            {
                Logger.WriteSensorLine(Sensor, $"(PureAloha) Канал {CHANNEL_ID} занят, " +
                    $"начинается ожидание {CHANNEL_TIMEOUT_IN_SECONDS} сек.");

                var time = Simulation.Instance.Time.AddSeconds(CHANNEL_TIMEOUT_IN_SECONDS);
                var action = new Action(() =>
                {
                    SendFrame(frame);
                });

                var e = new Event(time, action);
                Simulation.Instance.EventManager.AddEvent(e);

                return;
            }

            Sensor.PhysicalLayer.StartSending(frame, 0);

            if (frame.FrameType == Frame.Type.Ack)
            {
                return;
            }

            Logger.WriteSensorLine(Sensor, $"(PureAloha) начинает ожидать ACK от №{frame.IdReceive}");

            WaitingForAck = true;
            CreateAckTimeout(frame, 3);
        }

        private void ResendFrame(Frame frame, int attemptsLeft)
        {
            if (attemptsLeft == 0)
            {
                Logger.WriteSensorLine(Sensor, $"(PureAloha) не получил ACK от №{frame.IdReceive}.");
                return;
            }

            Logger.WriteSensorLine(Sensor, $"(PureAloha) пытается повторно отправить кадр сенсору №{frame.IdReceive}. " +
                $"Попыток осталось: {attemptsLeft}");

            SendFrame(frame);

            CreateAckTimeout(frame, attemptsLeft - 1);
        }

        private void CreateAckTimeout(Frame frame, int attemptsLeft)
        {
            Logger.WriteSensorLine(Sensor, $"(PureAloha) ждет ACK от №{frame.IdReceive} в течение {ACK_TIMEOUT_IN_SECONDS} сек.");
            var time = Simulation.Instance.Time.AddSeconds(ACK_TIMEOUT_IN_SECONDS);
            var action = new Action(() =>
            {
                if (!WaitingForAck)
                {
                    return;
                }

                ResendFrame(frame, attemptsLeft);
            });

            Simulation.Instance.EventManager.AddEvent(new Event(time, action));
        }
    }
}