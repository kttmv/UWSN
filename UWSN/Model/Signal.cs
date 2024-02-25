namespace UWSN.Model
{
    public static class Signal
    {
        public static void Emit(Sensor emittingSensor, Frame packet, int channelId = 0)
        {
            var simulation = Simulation.Instance;

            foreach (var sensor in simulation.Environment.Sensors)
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
                    var time = simulation.Time.AddSeconds(simulation.Environment.Sensors.IndexOf(sensor) + (new Random()).NextDouble());
                    var action = new Action(() =>
                    {
                        // опустошаем канал
                        simulation.ChannelSortedEmits[channelId] = null;
                        sensor.PhysicalLayer.ReceiveFrame(packet);
                    });

                    var e = new Event(time, action);

                    // обработка коллизии
                    if (simulation.ChannelSortedEmits[channelId] != null)
                    {
                        simulation.ChannelSortedEmits[channelId] = null;
                        simulation.RemoveEvent(e);

                        continue;
                    }

                    // занимаем канал
                    simulation.ChannelSortedEmits[channelId] = e;
                    simulation.AddEvent(e);
                }
            }
        }
    }
}