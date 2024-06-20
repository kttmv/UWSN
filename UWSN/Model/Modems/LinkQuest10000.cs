using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model.Modems
{
    internal class LinkQuest10000 : ModemBase
    {
        public LinkQuest10000()
        {
            Name = nameof(LinkQuest10000);
            CenterFrequency = 10.0;
            Bandwidth = 5.0;
            Bitrate = 5000.0;
            Range = 3.0;
            PowerTX = 40.0;
            PowerRX = 0.8;
            PowerSP = 0.3;
            PowerATWU = double.NaN;
        }
    }
}
