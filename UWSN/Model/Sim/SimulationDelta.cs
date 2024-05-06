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
        public required int Id { get; set; }
        public required int? ClusterId { get; set; }
        public required bool? IsReference { get; set; }
        public required double? Battery { get; set; }
    }

    public DateTime Time { get; set; } = new();
    public List<SignalDelta> SignalDeltas { get; set; } = new();
    public List<SensorDelta> SensorDeltas { get; set; } = new();

    public SimulationDelta(DateTime time)
    {
        Time = time;
    }
}
