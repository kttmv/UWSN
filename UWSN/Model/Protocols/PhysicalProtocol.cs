using UWSN.Model.Sim;
using UWSN.Utilities;
using static UWSN.Model.Sim.SimulationDelta;

namespace UWSN.Model.Protocols
{
    public class PhysicalProtocol : ProtocolBase
    {
        public Sensor.State OriginalState { get; set; }

        public void StartReceiving(Frame frame)
        {
            if (Sensor.IsDead)
                return;

            OriginalState = Sensor.CurrentState;

            Sensor.CurrentState = Sensor.State.Receiving;

            if (Simulation.Instance.Verbose)
                Logger.WriteSensorLine(Sensor, $"(Physical) начал принимать кадр от #{frame.SenderId}");
        }

        public void EndReceiving(Frame frame)
        {
            Sensor.CurrentState = OriginalState;

            if (Simulation.Instance.Verbose)
                Logger.WriteSensorLine(Sensor, $"(Physical) принял кадр от #{frame.SenderId}");

            Sensor.FrameBuffer.Add(frame);
            Sensor.DataLink.ReceiveFrame(frame);

            Simulation.Instance.Result!.TotalReceives += 1;
        }

        public void StartSending(Frame frame, int channelId)
        {
            if (Sensor.IsDead && frame.Type != Frame.FrameType.Warning)
                return;

            OriginalState = Sensor.CurrentState;

            Sensor.CurrentState = Sensor.State.Emitting;

            if (Simulation.Instance.Verbose)
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
            Sensor.CurrentState = OriginalState;

            if (Simulation.Instance.Verbose)
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

            if (Simulation.Instance.Verbose)
            {
                Logger.WriteSensorLine(
                    Sensor,
                    $"(Physical) обнаружил коллизию и прекратил отправку/принятие кадра"
                );
            }
        }
    }
}