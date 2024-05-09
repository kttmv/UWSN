using Newtonsoft.Json;

namespace UWSN.Model.Modems
{
    public abstract class ModemBase
    {
        [JsonIgnore]
        public string Name { get; set; } = "";

        [JsonIgnore]
        public double CenterFrequency { get; set; }

        [JsonIgnore]
        public double Bandwidth { get; set; }

        [JsonIgnore]
        public double Bitrate { get; set; }

        [JsonIgnore]
        public double Range { get; set; }

        [JsonIgnore]
        public double PowerTX { get; set; }

        [JsonIgnore]
        public double PowerRX { get; set; }

        [JsonIgnore]
        public double PowerSP { get; set; }

        [JsonIgnore]
        public double PowerATWU { get; set; }
    }
}
