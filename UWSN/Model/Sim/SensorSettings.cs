using UWSN.Model.Clusterization;
using UWSN.Model.Modems;
using UWSN.Model.Protocols.DataLink;

namespace UWSN.Model.Sim;

public class SensorSettings
{
    public ModemBase Modem { get; set; } = new AquaModem1000();

    public double InitialSensorBattery { get; set; } = 864_000.0;
    public double BatteryDeadCharge { get; set; } = 100.0;

    public TimeSpan SensorSampleInterval { get; set; } = TimeSpan.FromMinutes(10);

    public DateTime StartSamplingTime { get; set; } = new DateTime().AddDays(1);

    public DataLinkProtocol DataLinkProtocol { get; set; } = new PureAloha();

    public IClusterization ClusterizationAlgorithm { get; set; } =
        new RetardedClusterization() { NumberOfClusters = 6 };
}