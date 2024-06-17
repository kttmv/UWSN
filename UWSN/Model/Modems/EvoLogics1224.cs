using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UWSN.Model.Modems
{
    internal class EvoLogics1224 : ModemBase
    {
        public EvoLogics1224()
        {
            Name = nameof(EvoLogics1224);
            CenterFrequency = 18.5;
            Bandwidth = 11.0;
            Bitrate = 9200.0;
            Range = 6.0;
            PowerTX = 15.0;
            PowerRX = 0.3;
            PowerSP = 0.3;
            PowerATWU = double.NaN;
        }
    }
}
