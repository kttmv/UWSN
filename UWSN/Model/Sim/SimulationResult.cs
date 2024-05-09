using Newtonsoft.Json;

namespace UWSN.Model.Sim;

public class SimulationResult
{
    public struct SignalResult
    {
        public int FrameId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }

        public SignalResult(int frameId, int senderId, int receiverId)
        {
            FrameId = frameId;
            SenderId = senderId;
            ReceiverId = receiverId;
        }
    }

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

    public static SimulationDelta GetOrCreateSimulationDelta(DateTime time)
    {
        var allDeltas = Simulation.Instance.Result!.AllDeltas;
        if (!allDeltas.ContainsKey(time))
        {
            var delta = new SimulationDelta(time);
            allDeltas.Add(time, delta);
            return delta;
        }

        return allDeltas[time];
    }
}
