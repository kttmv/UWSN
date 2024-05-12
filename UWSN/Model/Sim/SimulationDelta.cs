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
        public Sensor.State? State { get; set; }
    }

    public DateTime Time { get; set; }
    public int CycleId { get; set; }

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

    public SimulationDelta(DateTime time, int cycleId)
    {
        Time = time;
        CycleId = cycleId;
    }
}
