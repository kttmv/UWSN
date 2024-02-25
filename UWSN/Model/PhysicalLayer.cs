using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UWSN.Model
{
    public class PhysicalLayer : BaseLayer
    {
        //protected int SensorId { get; set; }

        //[JsonIgnore]
        //public Sensor Sensor
        //{
        //    get
        //    {
        //        return Simulation.Instance.Environment.Sensors.FirstOrDefault(s => s.Id == SensorId)
        //            ?? throw new Exception("Не удалось найти сенсор с указанным ID");
        //    }
        //}

        public void ReceiveFrame(Frame frame)
        {
            Sensor.Buffer.Add(frame);
            Console.WriteLine("Долбаёб №" + Sensor.Id + " получил пакет (PhL)");

            Sensor.NetworkLayer.ReceiveFrame(frame);
        }

        public void SendFrame(Frame frame)
        {
            Signal.Emit(Sensor, frame);
        }

        public PhysicalLayer(int id)
        {
            SensorId = id;
        }
    }
}