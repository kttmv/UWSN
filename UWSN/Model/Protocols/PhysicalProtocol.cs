using UWSN.Model.Sim;
using UWSN.Utilities;
using static UWSN.Model.Sim.SimulationDelta;

namespace UWSN.Model.Protocols
{
    public class PhysicalProtocol : ProtocolBase
    {
        public enum State
        {
            Idle,
            Listening,
            Receiving,
            Emitting
        }

        private State _currentState;
        public State CurrentState
        {
            get { return _currentState; }
            set
            {
                _currentState = value;
                var delta = SimulationResult.GetOrCreateSimulationDelta(Simulation.Instance.Time);
                delta.SensorDeltas.Add(
                    new SensorDelta { Id = Sensor.Id, PhysicalProtocolState = value }
                );
            }
        }

        public State OriginalState { get; set; }

        public void StartReceiving(Frame frame)
        {
            if (Sensor.IsDead)
                return;

            OriginalState = Sensor.Physical.CurrentState;

            CurrentState = State.Receiving;

            Logger.WriteSensorLine(Sensor, $"(Physical) начал принимать кадр от #{frame.SenderId}");
        }

        public void EndReceiving(Frame frame)
        {
            CurrentState = OriginalState;

            Logger.WriteSensorLine(Sensor, $"(Physical) принял кадр от #{frame.SenderId}");

            Sensor.FrameBuffer.Add(frame);
            Sensor.DataLink.ReceiveFrame(frame);

            Simulation.Instance.Result!.TotalReceives += 1;
        }

        public void StartSending(Frame frame, int channelId)
        {
            if (Sensor.IsDead && frame.Type != Frame.FrameType.Warning)
                return;

            OriginalState = Sensor.Physical.CurrentState;

            CurrentState = State.Emitting;

            if (frame.ReceiverId == -1)
                Logger.WriteSensorLine(Sensor, $"(Physical) начал отправку кадра для всех");
            else
                Logger.WriteSensorLine(
                    Sensor,
                    $"(Physical) начал отправку кадра для #{frame.ReceiverId}"
                );

            if (frame.ReceiverId != -1)
            {
                _ = new Signal(Sensor, frame, channelId, true);
            }
            else
            {
                _ = new Signal(Sensor, frame, channelId, false);
            }
        }

        public void EndSending(Frame frame)
        {
            CurrentState = OriginalState;

            if (frame.ReceiverId == -1)
                Logger.WriteSensorLine(Sensor, $"(Physical) закончил отправку кадра для всех");
            else
                Logger.WriteSensorLine(
                    Sensor,
                    $"(Physical) закончил отправку кадра для #{frame.ReceiverId}"
                );

            Simulation.Instance.Result!.TotalSends += 1;
        }

        public void DetectCollision()
        {
            CurrentState = State.Listening;

            Logger.WriteSensorLine(
                Sensor,
                $"(Physical) обнаружил коллизию и прекратил отправку/принятие кадра"
            );
        }
    }
}
