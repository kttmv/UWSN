using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model
{
    public interface ISensorPlacementModel
    {
        public List<Sensor> Sensors { get; set; }

        public List<Sensor> PlaceSensors();
    }
}
