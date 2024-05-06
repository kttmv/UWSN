using Newtonsoft.Json;
using UWSN.Model.Protocols;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.Model.Protocols.DataLink
{
    public class PureAlohaProtocol : DataLinkProtocol
    {
        private const int CHANNEL_ID = 0;
        private const int CHANNEL_TIMEOUT_IN_SECONDS = 10;
        private const double CHANNEL_TIMEOUT_RELATIVE_DEVIATION = 0.5;
        private const int ACK_TIMEOUT_IN_SECONDS = 20;

        [JsonIgnore]
        private Event? WaitingForAckEvent { get; set; }

        [JsonIgnore]
        private List<int> SensorsAwaitingAck { get; set; }

        public PureAlohaProtocol()
        {
            SensorsAwaitingAck = new List<int>();
        }

        public override void StopAllAction()
        {
            if (WaitingForAckEvent == null)
                return;

            Simulation.Instance.EventManager.RemoveEvent(WaitingForAckEvent);
            WaitingForAckEvent = null;
        }

        public override void ReceiveFrame(Frame frame)
        {
            if (
                WaitingForAckEvent != null
                && frame.Type == Frame.FrameType.Ack
                && frame.ReceiverId == Sensor.Id
            )
            {
                StopAllAction();
                Sensor.Physical.CurrentState = PhysicalProtocol.State.Listening;
                Logger.WriteSensorLine(Sensor, $"(PureAloha) получил ACK от #{frame.SenderId}");

                return;
            }

            if (frame.ReceiverId == Sensor.Id && frame.AckIsNeeded)
            {
                var ack = new Frame
                {
                    SenderId = Sensor.Id,
                    SenderPosition = Sensor.Position,
                    ReceiverId = frame.SenderId,
                    Type = Frame.FrameType.Ack,
                    TimeSend = Simulation.Instance.Time,
                    AckIsNeeded = false,
                    NeighboursData = null,
                    BatteryLeft = double.NaN,
                    DeadSensors = null,
                    Data = null,
                };

                Logger.WriteSensorLine(
                    Sensor,
                    $"(PureAloha) начинаю отправку ACK для #{ack.ReceiverId}"
                );

                SensorsAwaitingAck.Add(ack.ReceiverId);

                Sensor.DataLink.SendFrame(ack);
            }

            Sensor.Network.ReceiveFrame(frame);
        }

        public override void SendFrame(Frame frame)
        {
            SendFrame(frame, true);
        }

        public void SendFrame(Frame frame, bool firstTime)
        {
            if (firstTime)
            {
                if (frame.ReceiverId == -1)
                    Logger.WriteSensorLine(Sensor, $"(PureAloha) отправляю кадр для всех");
                else
                    Logger.WriteSensorLine(
                        Sensor,
                        $"(PureAloha) отправляю кадр для #{frame.ReceiverId}"
                    );
            }
            else
            {
                if (frame.ReceiverId == -1)
                    Logger.WriteSensorLine(Sensor, $"(PureAloha) повторно отправляю кадр для всех");
                else
                    Logger.WriteSensorLine(
                        Sensor,
                        $"(PureAloha) повторно отправляю кадр для #{frame.ReceiverId}"
                    );
            }

            bool ackIsBlocking = SensorsAwaitingAck.Count > 0 && frame.Type != Frame.FrameType.Ack;

            // если канал занят или необходимо отправить ACK, то ждем и повторяем попытку
            if (Simulation.Instance.ChannelManager.IsChannelBusy(CHANNEL_ID) || ackIsBlocking)
            {
                double rngTimeout =
                    (new Random().NextDouble() - 0.5)
                    * CHANNEL_TIMEOUT_IN_SECONDS
                    * CHANNEL_TIMEOUT_RELATIVE_DEVIATION;
                double timeout = CHANNEL_TIMEOUT_IN_SECONDS + rngTimeout;

                if (ackIsBlocking)
                    Logger.WriteSensorLine(
                        Sensor,
                        "(PureAloha) невозможно совершить отправку, "
                            + "так как есть неотправленные пакеты ACK. "
                            + $"Попробую еще раз через {timeout} сек."
                    );
                else
                    Logger.WriteSensorLine(
                        Sensor,
                        $"(PureAloha) Канал {CHANNEL_ID} занят, "
                            + $"начинаю ожидание в {timeout} сек."
                    );

                Simulation.Instance.EventManager.AddEvent(
                    new Event(
                        Simulation.Instance.Time.AddSeconds(timeout),
                        $"Повторная попытка отправки кадра сенсором #{Sensor.Id}",
                        () => SendFrame(frame, firstTime)
                    )
                );

                return;
            }

            Sensor.Physical.StartSending(frame, CHANNEL_ID);

            if (frame.Type == Frame.FrameType.Ack)
            {
                SensorsAwaitingAck.Remove(frame.ReceiverId);

                // не ждем ответа на отправленный нами ACK
                return;
            }

            if (firstTime && frame.AckIsNeeded)
            {
                Logger.WriteSensorLine(
                    Sensor,
                    $"(PureAloha) начинаю ожидать ACK от #{frame.ReceiverId}"
                );
                CreateAckTimeout(frame, 3);
            }
        }

        private void ResendFrame(Frame frame, int attemptsLeft)
        {
            if (Sensor.Physical.CurrentState == PhysicalProtocol.State.Idle)
                return;

            if (attemptsLeft == 0)
            {
                Logger.WriteSensorLine(
                    Sensor,
                    $"(PureAloha) не получил ACK от #{frame.ReceiverId}. Ожидание прекращено."
                );

                WaitingForAckEvent = null;
                return;
            }

            Logger.WriteSensorLine(
                Sensor,
                $"(PureAloha) ACK не был получен. Повторно отправляю кадр сенсору #{frame.ReceiverId}. "
                    + $"Попыток осталось: {attemptsLeft}"
            );

            SendFrame(frame, false);

            CreateAckTimeout(frame, attemptsLeft - 1);
        }

        private void CreateAckTimeout(Frame frame, int attemptsLeft)
        {
            Logger.WriteSensorLine(
                Sensor,
                $"(PureAloha) жду ACK от #{frame.ReceiverId} в течение {ACK_TIMEOUT_IN_SECONDS} сек."
            );

            WaitingForAckEvent = new Event(
                Simulation.Instance.Time.AddSeconds(ACK_TIMEOUT_IN_SECONDS),
                $"Проверка получения ACK сенсором #{Sensor.Id}",
                () => ResendFrame(frame, attemptsLeft)
            );

            Simulation.Instance.EventManager.AddEvent(WaitingForAckEvent);
        }
    }
}
