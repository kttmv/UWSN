namespace UWSN.Model
{
    public static class Signal
    {
        public static void Emit(Sensor emittingSensor, Packet packet)
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

                double deliveryProb = 0.9; // здесь будет вычисление по формуле

                if (new Random().NextDouble() <= deliveryProb)
                {
                    Simulation.Instance.AddEvent(new Event(Simulation.Instance.Time.AddSeconds(Simulation.Instance.Environment.Sensors.IndexOf(sensor)), new Action(() => sensor.PhysicalLayer.ReceivePacket(packet, sensor))));
                }
            }
        }
    }
}