using Newtonsoft.Json;
using UWSN.Model.Sim;

namespace UWSN.Model.Protocols;

public abstract class ProtocolBase
{
    [JsonIgnore]
    public int? SensorId { get; set; }

    [JsonIgnore]
    private Sensor? _sensor;

    [JsonIgnore]
    public Sensor Sensor
    {
        get
        {
            if (_sensor == null)
            {
                if (!SensorId.HasValue)
                    throw new Exception("Не указан ID сенсора");

                _sensor = Simulation.Instance.Environment.Sensors.FirstOrDefault(s => s.Id == SensorId)
                    ?? throw new Exception("Не удалось найти сенсор с указанным ID");
            }

            return _sensor;
        }
    }
}