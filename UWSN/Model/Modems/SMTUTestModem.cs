using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UWSN.Model.Modems
{
    internal class SMTUTestModem : ModemBase
    {
        public SMTUTestModem()
        {
            Name = nameof(SMTUTestModem);
            CenterFrequency = 26.0;
            Bandwidth = 16.0;
            Bitrate = 13900.0;
            Range = 3.5;
            PowerTX = 25.0;
            PowerRX = 0.3;
            PowerSP = 0.3;
            PowerATWU = double.NaN;
        }
    }
}
