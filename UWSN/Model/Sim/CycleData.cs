using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model.Sim
{
    public class CycleData
    {
        public Dictionary<Sensor, double> BatteryChange { get; set; }
        public TimeSpan CycleTime { get; set; }

        public CycleData() 
        {
            BatteryChange = new Dictionary<Sensor, double>();
            CycleTime = TimeSpan.Zero;
        }
    }
}
