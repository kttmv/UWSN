using Newtonsoft.Json;
using UWSN.Model.Sim;
using UWSN.Utilities;
using static UWSN.Model.Sim.SimulationDelta;

namespace UWSN.Model.Protocols
{
    public class PhysicalProtocol : ProtocolBase
    {
        [JsonIgnore]
        public Sensor.State OriginalState { get; set; }

        [JsonIgnore]
        public bool CanStartSending
        {
            get
            {
                return Sensor.CurrentState == Sensor.State.Idle
                    || Sensor.CurrentState == Sensor.State.Listening;
            }
        }

        public void StartReceiving(Frame frame)
        {
            if (Sensor.CurrentState != Sensor.State.Listening)
            {
                throw new Exception(
                    "Сенсор не может начать принимать кадр, так как не находится в состоянии прослушивания."
                );
            }

            if (Sensor.IsDead)
                return;

            OriginalState = Sensor.CurrentState;

            Sensor.CurrentState = Sensor.State.Receiving;

            if (Simulation.Instance.SimulationSettings.Verbose)
                Logger.WriteSensorLine(
                    Sensor,
                    $"(Physical) начал принимать кадр от #{frame.SenderId}"
                );
        }

        public void EndReceiving(Frame frame, double transmissionTime)
        {
            if (Sensor.CurrentState != Sensor.State.Receiving)
            {
                throw new Exception(
                    "Сенсор не может закончить принимать кадр, так как не находится в состоянии приема."
                );
            }

            if (OriginalState == Sensor.State.Emitting)
            {
                throw new Exception(
                    "Невозможно перейти в состояние передачи данных после получения."
                );
            }

            if (OriginalState == Sensor.State.Receiving)
            {
                throw new Exception(
                    "Невозможно перейти в состояние получения данных после получения."
                );
            }

            Sensor.Battery -= Simulation.Instance.SensorSettings.Modem.PowerRX * transmissionTime;

            Sensor.CurrentState = OriginalState;

            if (Simulation.Instance.SimulationSettings.Verbose)
                Logger.WriteSensorLine(Sensor, $"(Physical) принял кадр от #{frame.SenderId}");

            Sensor.FrameBuffer.Add(frame);

            Sensor.DataLink.ReceiveFrame(frame);

            Simulation.Instance.Result!.TotalReceives += 1;
        }

        public void StartSending(Frame frame, int channelId)
        {
            if (!CanStartSending)
            {
                throw new Exception(
                    "Сенсор не может начать отправку данных."
                );
            }

            if (Sensor.IsDead && frame.Type != Frame.FrameType.Warning)
                return;

            OriginalState = Sensor.CurrentState;

            Sensor.CurrentState = Sensor.State.Emitting;

            if (Simulation.Instance.SimulationSettings.Verbose)
            {
                if (frame.ReceiverId == -1)
                    Logger.WriteSensorLine(Sensor, $"(Physical) начал отправку кадра для всех");
                else
                    Logger.WriteSensorLine(
                        Sensor,
                        $"(Physical) начал отправку кадра для #{frame.ReceiverId}"
                    );
            }

            if (frame.ReceiverId != -1)
                _ = new Signal(Sensor, frame, channelId, true);
            else
                _ = new Signal(Sensor, frame, channelId, false);
        }

        public void EndSending(Frame frame, double transmissionTime)
        {
            if (Sensor.CurrentState != Sensor.State.Emitting)
            {
                throw new Exception(
                    "Сенсор не может закончить отправлять кадр, так как не находится в состоянии передачи."
                );
            }

            if (OriginalState == Sensor.State.Emitting)
            {
                throw new Exception(
                    "Невозможно перейти из состояния передачи в состояние передачи."
                );
            }

            if (OriginalState == Sensor.State.Receiving)
            {
                throw new Exception(
                    "Невозможно перейти из состояния передачи в состояние приема."
                );
            }

            Sensor.Battery -=
                Simulation.Instance.SensorSettings.Modem.PowerTX * transmissionTime;

            Sensor.CurrentState = OriginalState;

            if (Simulation.Instance.SimulationSettings.Verbose)
            {
                if (frame.ReceiverId == -1)
                    Logger.WriteSensorLine(Sensor, $"(Physical) закончил отправку кадра для всех");
                else
                    Logger.WriteSensorLine(
                        Sensor,
                        $"(Physical) закончил отправку кадра для #{frame.ReceiverId}"
                    );
            }

            Simulation.Instance.Result!.TotalSends += 1;
        }

        public void DetectCollision()
        {
            Sensor.CurrentState = Sensor.State.Listening;

            if (Simulation.Instance.SimulationSettings.Verbose)
            {
                Logger.WriteSensorLine(
                    Sensor,
                    $"(Physical) обнаружил коллизию и прекратил отправку/принятие кадра"
                );
            }
        }

        public void StopAllAction()
        {
            Sensor.CurrentState = Sensor.State.Idle;
        }
    }
}