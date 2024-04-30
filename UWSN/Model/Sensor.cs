using System.Numerics;
using Newtonsoft.Json;
using UWSN.Model.Protocols;
using UWSN.Model.Protocols.DataLink;
using UWSN.Model.Sim;

namespace UWSN.Model
{
    public class Sensor
    {
        #region Properties

        [JsonIgnore]
        public PhysicalProtocol Physical { get; set; }

        [JsonIgnore]
        public DataLinkProtocol DataLink { get; set; }

        [JsonIgnore]
        public NetworkProtocol Network { get; set; }

        public double Battery { get; set; }

        private int _id;

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                // костыль. при десериализации всегда вызывается конструктор
                // без параметров (Sensor()), после чего заполняются все его
                // свойства. получается, что на момент создания объекта мы не
                // знаем какой у него id, поэтому приходится делать так
                Physical.SensorId = Id;
                DataLink.SensorId = Id;
                Network.SensorId = Id;
            }
        }

        public Vector3 Position { get; set; }

        [JsonIgnore]
        public List<Frame> FrameBuffer { get; set; } = new List<Frame>();

        #endregion Properties

        public Sensor(int id)
        {
            Physical = new PhysicalProtocol();
            DataLink = (DataLinkProtocol)(
                Activator.CreateInstance(Simulation.Instance.DataLinkProtocolType) ??
                throw new NullReferenceException("Тип сетевого протокола не определен"));
            Network = new NetworkProtocol();
            Id = id;
            Battery = 100.0;
        }

        public Sensor()
        {
            Physical = new PhysicalProtocol();
            DataLink = (DataLinkProtocol)(
                Activator.CreateInstance(Simulation.Instance.DataLinkProtocolType) ??
                throw new NullReferenceException("Тип сетевого протокола не определен"));
            Network = new NetworkProtocol();
            Battery = 100.0;
        }

        public void WakeUp()
        {
            var frame = new Frame
            {
                SenderId = Id,
                SenderPosition = Position,
                ReceiverId = -1,
                Type = Frame.FrameType.Hello,
                TimeSend = Simulation.Instance.Time,
                AckIsNeeded = false,
                NeighboursData = Network.Neighbours,
                BatteryLeft = Battery
            };

            Simulation.Instance.EventManager.AddEvent(new Event(
                default,
                $"Отправка HELLO от #{frame.SenderId}",
                () => DataLink.SendFrame(frame)));
        }
    }
}