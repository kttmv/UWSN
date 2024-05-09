﻿using Newtonsoft.Json;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.Model.Protocols.DataLink
{
    public class PureAloha : DataLinkProtocol
    {
        private const int CHANNEL_ID = 0;

        public int Timeout { get; set; } = 10;
        public double TimeoutRelativeDeviation { get; set; } = 0.5;
        public int AckTimeout { get; set; } = 20;
        public int AckRetries { get; set; } = 3;

        [JsonIgnore]
        private Event? WaitingForAckEvent { get; set; }

        [JsonIgnore]
        private List<int> SensorsAwaitingAck { get; set; }

        public PureAloha()
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
                Sensor.StopAllAction();
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
                    (new Random().NextDouble() - 0.5) * Timeout * TimeoutRelativeDeviation;
                double timeout = Timeout + rngTimeout;

                if (timeout <= 0)
                {
                    throw new Exception(
                        "Значение времени ожидания отрицательное. "
                            + "Вероятно, выставлено слишком большое относительное отклонение времени ожидания."
                    );
                }

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

                Sensor.AddEvent(
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
                CreateAckTimeout(frame, AckRetries);
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
                $"(PureAloha) жду ACK от #{frame.ReceiverId} в течение {AckTimeout} сек."
            );

            WaitingForAckEvent = new Event(
                Simulation.Instance.Time.AddSeconds(AckTimeout),
                $"Проверка получения ACK сенсором #{Sensor.Id}",
                () => ResendFrame(frame, attemptsLeft)
            );

            Sensor.AddEvent(WaitingForAckEvent);
        }
    }
}
