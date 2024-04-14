using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model.Modems
{
    public class AquaModem500 : ModemBase
    {
        public AquaModem500()
        {
            Name = nameof(AquaModem500);
            CenterFrequency = 29.0;
            Bandwidth = 4.0;
            Bitrate = 100.0;
            Range = double.NaN;
            PowerTX = double.NaN;
            PowerRX = double.NaN;
            PowerSP = double.NaN;
            PowerATWU = double.NaN;
        } 
    }
}
