using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model
{
    public class PhysicalLayer
    {
        [JsonIgnore]
        public Sensor Sensor { get; set; }

        /// <summary>
        /// ПОЧИНИТЬ ИСПРАВИТЬ НЕЛОМАТЬ ЗДЕЛАТЬ
        /// </summary>
        /// <param name="sensor">GIGA костыль</param>
        public void ReceiveFrame(Frame frame, Sensor sensor)
        {
            Sensor = sensor;
            Sensor.Buffer.Add(frame);
            // Sensor.NertworkLayer.ReceiveFrame(frame);
            Console.WriteLine("Долбаёб №" + Sensor.Id + " получил пакет (PhL)");

            sensor.NetworkLayer.ReceiveFrame(frame, sensor);
        }

        public void SendFrame(Frame frame)
        {
            Signal.Emit(Sensor, frame);
        }

        public PhysicalLayer(Sensor sensor)
        {
            Sensor = sensor;
        }
    }
}
