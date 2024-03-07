using System.Numerics;
using Newtonsoft.Json;
using UWSN.Model.Network;
using UWSN.Model.Protocols.NetworkLayer;
using UWSN.Model.Sim;

namespace UWSN.Model
{
    public class Sensor
    {
        public PhysicalProtocol PhysicalLayer { get; set; }
        public INetworkLayer NetworkLayer { get; set; }

        public int Id { get; set; }
        public Vector3 Position { get; set; }

        [JsonIgnore]
        public List<Frame> FrameBuffer { get; set; }

        public Sensor(int id)
        {
            Id = id;
            Position = new Vector3();
            PhysicalLayer = new PhysicalProtocol(Id);
            NetworkLayer = new PureAlohaProtocol(Id);
            FrameBuffer = new List<Frame>();
        }

        public void WakeUp()
        {
            var frame = new Frame
            {
                IdSend = Id,
                IdReceive = Id + 1
            };

            Simulation.Instance.EventManager.AddEvent(new Event(
                default,
                $"Отправка кадра от #{frame.IdSend} для #{frame.IdReceive}",
                () => NetworkLayer.SendFrame(frame)));

        }
    }
}
