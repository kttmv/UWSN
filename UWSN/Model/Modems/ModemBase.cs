using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model.Modems
{
    public abstract class ModemBase
    {
        public string Name { get; set; }
        public double CenterFrequency { get; set; }
        public double Bandwidth { get; set; }
        public double Bitrate { get; set; }
        public double Range { get; set; }
        public double PowerTX { get; set; }
        public double PowerRX { get; set; }
        public double PowerSP { get; set; }
        public double PowerATWU { get; set; }
    }
}