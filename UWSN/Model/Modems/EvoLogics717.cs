using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model.Modems
{
    internal class EvoLogics717 : ModemBase
    {
        public EvoLogics717()
        {
            Name = nameof(EvoLogics717);
            CenterFrequency = 12.0;
            Bandwidth = 10.0;
            Bitrate = 6900.0;
            Range = 8.0;
            PowerTX = 40.0;
            PowerRX = 0.3;
            PowerSP = 0.3;
            PowerATWU = double.NaN;
        }
    }
}
