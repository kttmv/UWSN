using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UWSN.Utilities;

namespace UWSN.Model.Network
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

        public State CurrentState { get; set; }

        public void StartReceiving(Frame frame)
        {
            CurrentState = State.Receiving;

            Logger.WriteSensorLine(Sensor, $"(Physical) начал получать кадр от №{frame.IdSend}");
        }

        public void EndReceiving(Frame frame)
        {
            CurrentState = State.Listening;

            Logger.WriteSensorLine(Sensor, $"(Physical) получил кадр от №{frame.IdSend}");

            Sensor.FrameBuffer.Add(frame);
            Sensor.NetworkLayer.ReceiveFrame(frame);
        }

        public void StartSending(Frame frame, int channelId)
        {
            CurrentState = State.Emitting;

            Logger.WriteSensorLine(Sensor, $"(Physical) начал отправку кадра Сенсору №{frame.IdReceive}");

            var signal = new Signal(Sensor, frame, channelId);
            signal.Emit();
        }

        public void EndSending(Frame frame)
        {
            CurrentState = State.Listening;

            Logger.WriteSensorLine(Sensor, $"(Physical) закончил отправку кадра Сенсору №{frame.IdReceive}");
        }

        public void DetectCollision()
        {
            CurrentState = State.Listening;

            Logger.WriteSensorLine(Sensor, $"(Physical) обнаружил коллизию и прекратил передачу/получение сообщения");
        }

        public PhysicalProtocol(int id)
        {
            SensorId = id;
        }
    }
}