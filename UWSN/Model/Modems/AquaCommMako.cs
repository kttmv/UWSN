using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model.Modems
{
    public class AquaCommMako : ModemBase
    {
        public AquaCommMako()
        {
            Name = nameof(AquaCommMako);
            CenterFrequency = 23.0;
            Bandwidth = 14.0;
            Bitrate = 240.0;
            Range = 1.0;
            PowerTX = 1.8;
            PowerRX = 0.252;
            PowerSP = 1.8;
            PowerATWU = 25.2;
        }
    }
}
