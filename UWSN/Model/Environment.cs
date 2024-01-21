using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model
{
    public class Environment
    {
        public Vector3[] AreaLimits { get; set; }
        
        public required List<Sensor> Sensors { get; set; }
    }
}
