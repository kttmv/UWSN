using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model.Modems
{
    internal class LinkQuest3000 : ModemBase
    {
        public LinkQuest3000()
        {
            Name = nameof(LinkQuest3000);
            CenterFrequency = 10.0;
            Bandwidth = 5.0;
            Bitrate = 5000.0;
            Range = 3.0;
            PowerTX = 12.0;
            PowerRX = 0.3;
            PowerSP = 0.3;
            PowerATWU = double.NaN;
        }
    }
}
