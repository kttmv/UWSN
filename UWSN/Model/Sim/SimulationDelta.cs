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

    private List<SignalDelta>? _signalDeltas;

    public List<SignalDelta> SignalDeltas
    {
        get
        {
            _signalDeltas ??= new();

            return _signalDeltas;
        }
    }

    private List<SensorDelta>? _sensorDeltas;

    public List<SensorDelta> SensorDeltas
    {
        get
        {
            _sensorDeltas ??= new();

            return _sensorDeltas;
        }
    }

    public SimulationDelta(DateTime time)
    {
        Time = time;
    }
}