using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UWSN.Model;

public class BaseLayer
{
    public int SensorId { get; set; }

    [JsonIgnore]
    public Sensor Sensor
    {
        get
        {
            return Simulation.Instance.Environment.Sensors.FirstOrDefault(s => s.Id == SensorId)
                ?? throw new Exception("Не удалось найти сенсор с указанным ID");
        }
    }
}