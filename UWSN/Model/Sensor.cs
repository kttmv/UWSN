using System.Numerics;
using Newtonsoft.Json;
using UWSN.Model.DataLink;
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
            Id = id;
        }

        public Sensor()
        {
            Physical = new PhysicalProtocol();
            DataLink = (DataLinkProtocol)(
                Activator.CreateInstance(Simulation.Instance.DataLinkProtocolType) ??
                throw new NullReferenceException("Тип сетевого протокола не определен"));
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
                () => DataLink.SendFrame(frame)));

        }
    }
}
