using Newtonsoft.Json;
using static UWSN.Model.Sim.SimulationDelta;

namespace UWSN.Model.Sim;

public class SimulationResult
{
    public struct SignalResult
    {
        public required int FrameId { get; set; }
        public required int SenderId { get; set; }
        public required int ReceiverId { get; set; }
    }

    public static bool ShouldCreateAllDeltas { get; set; }

    public TimeSpan RealTimeToSimulate { get; set; }
    public int TotalEvents { get; set; }
    public int TotalCycles { get; set; }
    public int TotalSends { get; set; }
    public int TotalReceives { get; set; }
    public int TotalCollisions { get; set; }
    public List<Frame> AllFrames { get; set; } = new();
    public List<SignalResult> AllSignals { get; set; } = new();

    [JsonIgnore]
    public Dictionary<DateTime, SimulationDelta> AllDeltas { get; set; } = new();

    public List<SimulationDelta> Deltas
    {
        get { return AllDeltas.ToList().Select(x => x.Value).OrderBy(x => x.Time).ToList(); }
    }

    private SimulationDelta GetOrCreateSimulationDelta(DateTime time)
    {
        if (!AllDeltas.TryGetValue(time, out SimulationDelta? value))
        {
            var delta = new SimulationDelta(time);
            value = delta;
            AllDeltas.Add(time, value);
            return delta;
        }

        return value;
    }

    public void AddSensorDelta(SensorDelta delta, bool force)
    {
        if (ShouldCreateAllDeltas || force)
        {
            var simulationDelta = GetOrCreateSimulationDelta(Simulation.Instance.Time);
            simulationDelta.SensorDeltas.Add(delta);
        }
    }

    public void AddSignalDelta(SignalDelta delta, DateTime time, bool force)
    {
        if (ShouldCreateAllDeltas || force)
        {
            var simulationDelta = GetOrCreateSimulationDelta(time);
            simulationDelta.SignalDeltas.Add(delta);
        }
    }

    public void AddFrame(Frame frame, bool force)
    {
        if (ShouldCreateAllDeltas || force)
        {
            AllFrames.Add(frame);
        }
    }

    public void AddSignalResult(SignalResult signal, bool force)
    {
        if (ShouldCreateAllDeltas || force)
        {
            AllSignals.Add(signal);
        }
    }
}