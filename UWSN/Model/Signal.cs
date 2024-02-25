namespace UWSN.Model
{
    public static class Signal
    {
        public static void Emit(Sensor emittingSensor, Frame packet, int channelId = 0)
        {
            foreach (var sensor in Simulation.Instance.Environment.Sensors)
            {
                if (sensor == emittingSensor)
                {
                    continue;
                }

                double distance = Math.Sqrt(
                       Math.Pow(sensor.Position.X - emittingSensor.Position.X, 2) +
                       Math.Pow(sensor.Position.Y - emittingSensor.Position.Y, 2) +
                       Math.Pow(sensor.Position.Z - emittingSensor.Position.Z, 2)
                                           );

                double deliveryProb = 1d; // здесь будет вычисление по формуле

                if (new Random().NextDouble() <= deliveryProb)
                {
                    // создаем ивент получения сенсором кадра
                    var time = Simulation.Instance.Time.AddSeconds(Simulation.Instance.Environment.Sensors.IndexOf(sensor) + (new Random()).NextDouble());
                    var action = new Action(() => 
                    {
                        // опустошаем канал
                        Simulation.Instance.ChannelSortedEmits[channelId] = null;
                        sensor.PhysicalLayer.ReceiveFrame(packet, sensor);
                    });

                    var e = new Event(time, action);

                    // обработка коллизии
                    if (Simulation.Instance.ChannelSortedEmits[channelId] != null)
                    {
                        Simulation.Instance.ChannelSortedEmits[channelId] = null;
                        Simulation.Instance.RemoveEvent(e);
                        
                        continue;
                    }

                    // занимаем канал
                    Simulation.Instance.ChannelSortedEmits[channelId] = e;
                    Simulation.Instance.AddEvent(e);
                }
            }
        }
    }
}