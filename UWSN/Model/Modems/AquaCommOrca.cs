using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model.Modems
{
    public class AquaCommOrca : ModemBase
    {
        public AquaCommOrca()
        {
            Name = nameof(AquaCommOrca);
            CenterFrequency = 23.0;
            Bandwidth = 14.0;
            Bitrate = 100.0;
            Range = 3.0;
            PowerTX = 1.8;
            PowerRX = 0.252;
            PowerSP = 1.8;
            PowerATWU = 25.2;
        }
    }
}
