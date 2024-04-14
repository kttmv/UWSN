using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model.Modems
{
    public class AquaModem1000 : ModemBase
    {
        public AquaModem1000()
        {
            Name = nameof(AquaModem1000);
            CenterFrequency = 9.75;
            Bandwidth = 4.5;
            Bitrate = 1000.0;
            Range = 5.0;
            PowerTX = 20.0;
            PowerRX = 0.6;
            PowerSP = 1.0;
            PowerATWU = 5.0;
        }
    }
}
