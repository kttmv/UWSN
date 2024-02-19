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
        public void ReceivePacket(Packet packet, Sensor sensor)
        {
            Sensor = sensor;
            Sensor.Buffer.Add(packet);
            Console.WriteLine("Долбаёб №" + Sensor.Id + " получил пакет");
        }

        public void SendPacket(Packet packet)
        {
            Signal.Emit(Sensor, packet);
        }

        public PhysicalLayer(Sensor sensor)
        {
            Sensor = sensor;
        }
    }
}
