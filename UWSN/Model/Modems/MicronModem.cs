using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model.Modems
{
    public class MicronModem : ModemBase
    {
        public MicronModem()
        {
            Name = nameof(MicronModem);
            CenterFrequency = 22.0;
            Bandwidth = 4.0;
            Bitrate = 40.0;
            Range = 0.5;
            PowerTX = 7.92;
            PowerRX = 0.72;
            PowerSP = double.NaN;
            PowerATWU = double.NaN;
        }
    }
}
