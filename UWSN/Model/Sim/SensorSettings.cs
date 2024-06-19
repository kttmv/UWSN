using UWSN.Model.Clusterization;
using UWSN.Model.Modems;
using UWSN.Model.Protocols.DataLink;
using UWSN.Model.Protocols.Network;

namespace UWSN.Model.Sim;

public class SensorSettings
{
    public ModemBase Modem { get; set; } = new AquaModem1000();

    public double InitialSensorBattery { get; set; } = 864_000.0;
    public double BatteryDeadCharge { get; set; } = 100.0;

    public TimeSpan SampleInterval { get; set; } = TimeSpan.FromMinutes(30);

    public DataLinkProtocol DataLinkProtocol { get; set; } = new PureAloha();
    public NetworkProtocol NetworkProtocol { get; set; } = new BasicNetworkProtocol();

    public IClusterization ClusterizationAlgorithm { get; set; } = new AutoClusterization();
    //new RetardedClusterization() { XClusterCount = 3, ZClusterCount = 2 };
}
