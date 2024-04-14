using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UWSN.Model.Modems
{
    public class AquaCommMarlin : ModemBase
    {
        public AquaCommMarlin()
        {
            Name = nameof(AquaCommMarlin);
            CenterFrequency = 23.0;
            Bandwidth = 14.0;
            Bitrate = 480.0;
            Range = 1.0;
            PowerTX = 1.8;
            PowerRX = 0.252;
            PowerSP = 1.8;
            PowerATWU = 25.2;
        }
    }
}
