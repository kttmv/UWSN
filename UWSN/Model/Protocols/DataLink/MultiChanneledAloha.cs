using Newtonsoft.Json;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.Model.Protocols.DataLink
{
    public class MultiChanneledAloha : DataLinkProtocol
    {
        public int Timeout { get; set; } = 10;
        public double TimeoutRelativeDeviation { get; set; } = 0.5;
        public int AckTimeout { get; set; } = 20;

        [JsonIgnore]
        private Event? WaitingForAckEvent { get; set; }

        [JsonIgnore]
        private List<int> SensorsAwaitingAck { get; set; }

        public MultiChanneledAloha()
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
                Logger.WriteSensorLine(
                    Sensor,
                    $"(MultiChanneledAloha) получил ACK от #{frame.SenderId}"
                );

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
                    $"(MultiChanneledAloha) начинаю отправку ACK для #{ack.ReceiverId}"
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
                    Logger.WriteSensorLine(
                        Sensor,
                        $"(MultiChanneledAloha) отправляю кадр для всех"
                    );
                else
                    Logger.WriteSensorLine(
                        Sensor,
                        $"(MultiChanneledAloha) отправляю кадр для #{frame.ReceiverId}"
                    );
            }
            else
            {
                if (frame.ReceiverId == -1)
                    Logger.WriteSensorLine(
                        Sensor,
                        $"(MultiChanneledAloha) повторно отправляю кадр для всех"
                    );
                else
                    Logger.WriteSensorLine(
                        Sensor,
                        $"(MultiChanneledAloha) повторно отправляю кадр для #{frame.ReceiverId}"
                    );
            }

            bool ackIsBlocking = SensorsAwaitingAck.Count > 0 && frame.Type != Frame.FrameType.Ack;

            // таймаут для ожидания, если появится необходимость подождать
            double rngTimeout =
                (new Random().NextDouble() - 0.5) * Timeout * TimeoutRelativeDeviation;
            double timeout = Timeout + rngTimeout;

            var freeChannels = Simulation.Instance.ChannelManager.FreeChannels;
            if (freeChannels == null || freeChannels?.Count == 0 || ackIsBlocking)
            {
                if (ackIsBlocking)
                    Logger.WriteSensorLine(
                        Sensor,
                        "(MultiChanneledAloha) невозможно совершить отправку, "
                            + "так как есть неотправленные пакеты ACK. "
                            + $"Попробую еще раз через {timeout} сек."
                    );
                else
                    Logger.WriteSensorLine(
                        Sensor,
                        "(MultiChanneledAloha)Все каналы заняты, "
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

            Sensor.Physical.StartSending(frame, freeChannels.First());

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
                    $"(MultiChanneledAloha) начинаю ожидать ACK от #{frame.ReceiverId}"
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
                    $"(MultiChanneledAloha) не получил ACK от #{frame.ReceiverId}. Ожидание прекращено."
                );

                WaitingForAckEvent = null;
                return;
            }

            Logger.WriteSensorLine(
                Sensor,
                $"(MultiChanneledAloha) ACK не был получен. Повторно отправляю кадр сенсору #{frame.ReceiverId}. "
                    + $"Попыток осталось: {attemptsLeft}"
            );

            SendFrame(frame, false);

            CreateAckTimeout(frame, attemptsLeft - 1);
        }

        private void CreateAckTimeout(Frame frame, int attemptsLeft)
        {
            Logger.WriteSensorLine(
                Sensor,
                $"(MultiChanneledAloha) жду ACK от #{frame.ReceiverId} в течение {AckTimeout} сек."
            );

            WaitingForAckEvent = new Event(
                Simulation.Instance.Time.AddSeconds(AckTimeout),
                $"Проверка получения ACK сенсором #{Sensor.Id}",
                () => ResendFrame(frame, attemptsLeft)
            );

            Simulation.Instance.EventManager.AddEvent(WaitingForAckEvent);
        }
    }
}
