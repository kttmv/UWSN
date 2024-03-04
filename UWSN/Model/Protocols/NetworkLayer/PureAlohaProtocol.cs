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
                Logger.WriteSimulationLine($"(NetworkLayer)  Сенсор №{Sensor.Id} получил пакет ACK от №{frame.IdSend}");

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

                Logger.WriteSimulationLine($"(NetworkLayer)  Сенсор №{Sensor.Id} начал отправку ACK Сенсору №{frame.IdReceive}");
                Sensor.NetworkLayer.SendFrame(ack);
            }
        }

        public void SendFrame(Frame frame)
        {
            // если канал занят, то ждем 5с
            if (Simulation.Instance.ChannelManager.IsChannelBusy(0))
            {
                var time = frame.TimeSend.AddSeconds(5);
                var action = new Action(() =>
                {
                    SendFrame(frame);
                });

                var e = new Event(time, action);
                Simulation.Instance.AddEvent(e);

                return;
            }

            Sensor.PhysicalLayer.StartSending(frame, 0);

            if (frame.FrameType == Frame.Type.Ack)
            {
                return;
            }

            WaitingForAck = true;
            CreateAckTimeout(frame, 3);
        }

        private void ResendFrame(Frame frame, int attemptsLeft)
        {
            if (attemptsLeft == 0)
            {
                return;
            }

            SendFrame(frame);

            CreateAckTimeout(frame, attemptsLeft - 1);
        }

        private void CreateAckTimeout(Frame frame, int attemptsLeft)
        {
            var time = Simulation.Instance.Time.AddSeconds(10); // ждем 10 секунд
            var action = new Action(() =>
            {
                if (!WaitingForAck)
                {
                    return;
                }

                ResendFrame(frame, attemptsLeft);
            });

            Simulation.Instance.AddEvent(new Event(time, action));
        }
    }
}