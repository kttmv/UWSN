using Newtonsoft.Json;
using UWSN.Model.Protocols;

namespace UWSN.Model.Sim;

public class SimulationDelta
{
    public enum SignalDeltaType
    {
        Add,
        Remove
    }

    public struct SignalDelta
    {
        public required int SignalId { get; set; }
        public required SignalDeltaType Type { get; set; }
    }

    public struct SensorDelta
    {
        public int Id { get; set; }
        public int? ClusterId { get; set; }
        public bool? IsReference { get; set; }
        public double? Battery { get; set; }
        public PhysicalProtocol.State? PhysicalProtocolState { get; set; }
    }

    public DateTime Time { get; set; } = new();

    public List<SignalDelta> SignalDeltas { get; set; } = new();

    public bool ShouldSerializeSignalDeltas()
    {
        return SignalDeltas.Count > 0;
    }

    public List<SensorDelta> SensorDeltas { get; set; } = new();

    public bool ShouldSerializeSensorDeltas()
    {
        return SensorDeltas.Count > 0;
    }

    public SimulationDelta(DateTime time)
    {
        Time = time;
    }
}