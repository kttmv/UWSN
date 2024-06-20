using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model.Modems
{
    internal class EvoLogicsS2CM1834 : ModemBase
    {
        public EvoLogicsS2CM1834()
        {
            Name = nameof(EvoLogicsS2CM1834);
            CenterFrequency = 27.0;
            Bandwidth = 16.0;
            Bitrate = 13900.0;
            Range = 3.5;
            PowerTX = 35.0;
            PowerRX = 0.8;
            PowerSP = 0.3;
            PowerATWU = double.NaN;
        }
    }
}
