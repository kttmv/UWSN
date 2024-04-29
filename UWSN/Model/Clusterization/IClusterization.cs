using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWSN.Utilities;

namespace UWSN.Model.Clusterization
{
    public interface IClusterization
    {
        public List<Sensor> Clusterize(List<Sensor> sensors, Vector3Range areaLimits);
    }
}
