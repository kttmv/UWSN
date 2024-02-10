namespace UWSN.Model
{
    public class SensorPlacementOrthogonalGrid : ISensorPlacementModel
    {
        private readonly List<Sensor> _sensors;
        private readonly float _step;

        public List<Sensor> PlaceSensors()
        {
            int placedCount = 0;
            int cubicEdge = (int)Math.Ceiling(Math.Pow(_sensors.Count, 1.0 / 3.0));

            for (int i = 0; i < cubicEdge; i++)
            {
                for (int j = 0; j < cubicEdge; j++)
                {
                    for (int k = 0; k < cubicEdge; k++)
                    {
                        if (placedCount >= _sensors.Count)
                        {
                            break;
                        }

                        _sensors[placedCount].Position = new System.Numerics.Vector3(i * _step, j * _step, k * _step);
                        placedCount++;
                    }
                }
            }

            return _sensors;
        }

        public SensorPlacementOrthogonalGrid(List<Sensor> sensors, float step)
        {
            _sensors = sensors;
            _step = step;
        }
    }
}