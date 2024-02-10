using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model
{
    public static class Signal
    {
        public static void Emit(Sensor emittingSensor, Packet packet)
        {
            foreach (var sensor in Simulation.instance.Environment.Sensors)
            {
                double distance = Math.Sqrt(
                       Math.Pow(sensor.Position.X - emittingSensor.Position.X, 2) +
                       Math.Pow(sensor.Position.Y - emittingSensor.Position.Y, 2) +
                       Math.Pow(sensor.Position.Z - emittingSensor.Position.Z, 2)
                                           );

                double deliveryProb = 0.9; // здесь будет вычисление по формуле

                if (new Random().NextDouble() <= deliveryProb)
                {
                    //Simulation.instance.AddEvent(new Event(DateTime.MinValue, new Action(() => sensor.Physical.ReceivePacket(packetCopy))));
                }
            }
        }
    }
}