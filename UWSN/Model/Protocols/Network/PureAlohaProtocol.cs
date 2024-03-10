using Newtonsoft.Json;
using UWSN.Model.Network;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.Model.Protocols.Network
{
    public class PureAlohaProtocol : NetworkProtocol
    {
        private const int CHANNEL_ID = 0;
        private const int CHANNEL_TIMEOUT_IN_SECONDS = 4;
        private const int ACK_TIMEOUT_IN_SECONDS = 20;

        [JsonIgnore]
        private Event? WaitingForAckEvent { get; set; }

        [JsonIgnore]
        private List<int> SensorsAwaitingAck { get; set; }

        public PureAlohaProtocol()
        {
            SensorsAwaitingAck = new List<int>();
        }

        public override void ReceiveFrame(Frame frame)
        {
            if (WaitingForAckEvent != null
                && frame.FrameType == Frame.Type.Ack
                && frame.IdReceive == Sensor.Id)
            {
                Simulation.Instance.EventManager.RemoveEvent(WaitingForAckEvent);
                WaitingForAckEvent = null;
                Sensor.Physical.CurrentState = PhysicalProtocol.State.Idle;
                Logger.WriteSensorLine(Sensor, $"(PureAloha) получил ACK от #{frame.IdSend}");

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

                Logger.WriteSensorLine(Sensor, $"(PureAloha) начинаю отправку ACK для #{ack.IdReceive}");

                SensorsAwaitingAck.Add(ack.IdReceive);

                Sensor.Network.SendFrame(ack);
            }
        }

        public override void SendFrame(Frame frame)
        {
            SendFrame(frame, true);
        }

        public void SendFrame(Frame frame, bool firstTime)
        {
            if (firstTime)
                Logger.WriteSensorLine(Sensor, $"(PureAloha) отправляю кадр для #{frame.IdReceive}");
            else
                Logger.WriteSensorLine(Sensor, $"(PureAloha) повторно отправляю кадр для #{frame.IdReceive}");

            bool ackIsBlocking = SensorsAwaitingAck.Count > 0 && frame.FrameType != Frame.Type.Ack;

            // если канал занят или необходимо отправить ACK, то ждем и повторяем попытку
            if (Simulation.Instance.ChannelManager.IsChannelBusy(CHANNEL_ID) ||
                ackIsBlocking)
            {
                if (ackIsBlocking)
                    Logger.WriteSensorLine(Sensor, "(PureAloha) невозможно совершить отправку, " +
                        "так как есть неотправленные пакеты ACK. " +
                        $"начинаю ожидание в {CHANNEL_TIMEOUT_IN_SECONDS} сек.");
                else
                    Logger.WriteSensorLine(Sensor, $"(PureAloha) Канал {CHANNEL_ID} занят, " +
                        $"начинаю ожидание в {CHANNEL_TIMEOUT_IN_SECONDS} сек.");

                Simulation.Instance.EventManager.AddEvent(new Event(
                    Simulation.Instance.Time.AddSeconds(CHANNEL_TIMEOUT_IN_SECONDS),
                    $"Повторная попытка отправки кадра сенсором #{Sensor.Id}",
                    () => SendFrame(frame, firstTime)));

                return;
            }

            Sensor.Physical.StartSending(frame, 0);

            if (frame.FrameType == Frame.Type.Ack)
                SensorsAwaitingAck.Remove(frame.IdReceive);

            // не ждем ответа на отправленный нами ACK
            if (frame.FrameType == Frame.Type.Ack)
                return;

            if (firstTime)
            {
                Logger.WriteSensorLine(Sensor, $"(PureAloha) начинаю ожидать ACK от #{frame.IdReceive}");
                CreateAckTimeout(frame, 3);
            }
        }

        private void ResendFrame(Frame frame, int attemptsLeft)
        {
            if (attemptsLeft == 0)
            {
                Logger.WriteSensorLine(Sensor, $"(PureAloha) не получил ACK от #{frame.IdReceive}. Ожидание прекращено.");
                return;
            }

            Logger.WriteSensorLine(Sensor, $"(PureAloha) ACK не был получен. Повторно отправляю кадр сенсору #{frame.IdReceive}. " +
                $"Попыток осталось: {attemptsLeft}");

            SendFrame(frame, false);

            CreateAckTimeout(frame, attemptsLeft - 1);
        }

        private void CreateAckTimeout(Frame frame, int attemptsLeft)
        {
            Logger.WriteSensorLine(Sensor, $"(PureAloha) жду ACK от #{frame.IdReceive} в течение {ACK_TIMEOUT_IN_SECONDS} сек.");

            WaitingForAckEvent = new Event(
                Simulation.Instance.Time.AddSeconds(ACK_TIMEOUT_IN_SECONDS),
                $"Проверка получения ACK сенсором #{Sensor.Id}",
                () => ResendFrame(frame, attemptsLeft));

            Simulation.Instance.EventManager.AddEvent(WaitingForAckEvent);
        }
    }
}
