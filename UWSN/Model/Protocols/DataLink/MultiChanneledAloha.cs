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
        public int AckRetries { get; set; } = 3;

        [JsonIgnore]
        private Event? WaitingForAckEvent { get; set; }

        [JsonIgnore]
        private List<int> SensorsAwaitingAck { get; set; }

        [JsonIgnore]
        private static readonly Random Random = new();

        public MultiChanneledAloha()
        {
            SensorsAwaitingAck = new List<int>();
        }

        public override void StopAllAction()
        {
            WaitingForAckEvent = null;
            SensorsAwaitingAck = new();
        }

        public override void ReceiveFrame(Frame frame)
        {
            if (
                WaitingForAckEvent != null
                && frame.Type == Frame.FrameType.Ack
                && frame.ReceiverId == Sensor.Id
            )
            {
                WaitingForAckEvent = null;
                Sensor.CurrentState = Sensor.State.Listening;

                LogReceivedAck(frame);

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
                    CollectedData = null,
                };

                LogStartingSendingAck(ack);

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
            LogSendingFrame(frame, firstTime);

            bool ackIsBlocking = SensorsAwaitingAck.Count > 0 && frame.Type != Frame.FrameType.Ack;

            // таймаут для ожидания, если появится необходимость подождать
            double rngTimeout = (Random.NextDouble() - 0.5) * Timeout * TimeoutRelativeDeviation;
            double timeout = Timeout + rngTimeout;

            if (timeout <= 0)
            {
                throw new Exception(
                    "Значение времени ожидания отрицательное. "
                        + "Вероятно, выставлено слишком большое относительное отклонение времени ожидания."
                );
            }

            var freeChannels = Simulation.Instance.ChannelManager.FreeChannels;
            if (
                freeChannels == null
                || freeChannels.Count == 0
                || ackIsBlocking
                || !Sensor.Physical.CanStartSending
            )
            {
                if (ackIsBlocking)
                    LogAckIsBlocking(timeout);
                else if (!Sensor.Physical.CanStartSending)
                    LogCantStartSending(timeout);
                else
                    LogAllChannelsAreBusy(timeout);

                Sensor.AddEvent(
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
                LogStartWaitingForAck(frame);

                CreateAckTimeout(frame, AckRetries);
            }
        }

        private void ResendFrame(Frame frame, int attemptsLeft)
        {
            if (attemptsLeft == 0)
            {
                LogStoppedWaitingForAck(frame);

                WaitingForAckEvent = null;
                return;
            }

            LogAckRetry(frame, attemptsLeft);

            Simulation.Instance.Result!.TotalResends += 1;
            SendFrame(frame, false);

            CreateAckTimeout(frame, attemptsLeft - 1);
        }

        private void CreateAckTimeout(Frame frame, int attemptsLeft)
        {
            LogWaitingForAck(frame);

            WaitingForAckEvent = new Event(
                Simulation.Instance.Time.AddSeconds(AckTimeout),
                $"Проверка получения ACK сенсором #{Sensor.Id}",
                () => ResendFrame(frame, attemptsLeft)
            );

            Sensor.AddEvent(WaitingForAckEvent);
        }

        #region Logger

        private void LogSendingFrame(Frame frame, bool firstTime)
        {
            if (!Simulation.Instance.SimulationSettings.Verbose)
                return;

            if (firstTime)
            {
                if (frame.ReceiverId == -1)
                {
                    Logger.WriteSensorLine(
                        Sensor,
                        $"(MultiChanneledAloha) отправляю кадр для всех"
                    );
                }
                else
                {
                    Logger.WriteSensorLine(
                        Sensor,
                        $"(MultiChanneledAloha) отправляю кадр для #{frame.ReceiverId}"
                    );
                }
            }
            else
            {
                if (frame.ReceiverId == -1)
                {
                    Logger.WriteSensorLine(
                        Sensor,
                        $"(MultiChanneledAloha) повторно отправляю кадр для всех"
                    );
                }
                else
                {
                    Logger.WriteSensorLine(
                        Sensor,
                        $"(MultiChanneledAloha) повторно отправляю кадр для #{frame.ReceiverId}"
                    );
                }
            }
        }

        private void LogStartWaitingForAck(Frame frame)
        {
            if (!Simulation.Instance.SimulationSettings.Verbose)
                return;

            Logger.WriteSensorLine(
                Sensor,
                $"(MultiChanneledAloha) начинаю ожидать ACK от #{frame.ReceiverId}"
            );
        }

        private void LogAckRetry(Frame frame, int attemptsLeft)
        {
            if (!Simulation.Instance.SimulationSettings.Verbose)
                return;

            Logger.WriteSensorLine(
                Sensor,
                $"(MultiChanneledAloha) ACK не был получен. Повторно отправляю кадр сенсору #{frame.ReceiverId}. "
                    + $"Попыток осталось: {attemptsLeft}"
            );
        }

        private void LogStoppedWaitingForAck(Frame frame)
        {
            if (!Simulation.Instance.SimulationSettings.Verbose)
                return;

            Logger.WriteSensorLine(
                Sensor,
                $"(MultiChanneledAloha) не получил ACK от #{frame.ReceiverId}. Ожидание прекращено."
            );
        }

        private void LogWaitingForAck(Frame frame)
        {
            if (!Simulation.Instance.SimulationSettings.Verbose)
                return;

            Logger.WriteSensorLine(
                Sensor,
                $"(MultiChanneledAloha) жду ACK от #{frame.ReceiverId} в течение {AckTimeout} сек."
            );
        }

        private void LogStartingSendingAck(Frame ack)
        {
            if (!Simulation.Instance.SimulationSettings.Verbose)
                return;

            Logger.WriteSensorLine(
                Sensor,
                $"(MultiChanneledAloha) начинаю отправку ACK для #{ack.ReceiverId}"
            );
        }

        private void LogReceivedAck(Frame frame)
        {
            if (!Simulation.Instance.SimulationSettings.Verbose)
                return;

            Logger.WriteSensorLine(
                Sensor,
                $"(MultiChanneledAloha) получил ACK от #{frame.SenderId}"
            );
        }

        private void LogAllChannelsAreBusy(double timeout)
        {
            if (!Simulation.Instance.SimulationSettings.Verbose)
                return;

            Logger.WriteSensorLine(
                Sensor,
                "(MultiChanneledAloha) Все каналы заняты, " + $"начинаю ожидание в {timeout} сек."
            );
        }

        private void LogCantStartSending(double timeout)
        {
            if (!Simulation.Instance.SimulationSettings.Verbose)
                return;

            Logger.WriteSensorLine(
                Sensor,
                "(MultiChanneledAloha) Модем заблокирован, " + $"начинаю ожидание в {timeout} сек."
            );
        }

        private void LogAckIsBlocking(double timeout)
        {
            if (!Simulation.Instance.SimulationSettings.Verbose)
                return;

            Logger.WriteSensorLine(
                Sensor,
                "(MultiChanneledAloha) невозможно совершить отправку, "
                    + "так как есть неотправленные пакеты ACK. "
                    + $"Попробую еще раз через {timeout} сек."
            );
        }

        #endregion Logger
    }
}
