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

    public class ClusterCycleResult
    {
        public int ClusterId { get; set; }
        public int ReferenceSensorId { get; set; }
        public int SensorsCount { get; set; }
        public int CollectedDataCount { get; set; }
        public int CollectedDataSameClusterCount { get; set; }
        public int CollectedDataOtherClusterCount { get; set; }
    }

    public class CycleResult
    {
        public int CycleId { get; set; }
        public Dictionary<int, ClusterCycleResult> ClusterResults { get; set; } = new();

        public int SensorsCount
        {
            get { return ClusterResults.Values.Select(r => r.SensorsCount).Sum(); }
        }
        public int CollectedDataCount
        {
            get { return ClusterResults.Values.Select(r => r.CollectedDataCount).Sum(); }
        }
        public int CollectedDataSameClusterCount
        {
            get { return ClusterResults.Values.Select(r => r.CollectedDataSameClusterCount).Sum(); }
        }
        public int CollectedDataOtherClusterCount
        {
            get
            {
                return ClusterResults.Values.Select(r => r.CollectedDataOtherClusterCount).Sum();
            }
        }
    }

    public DateTime SimulationEndTime { get; set; }
    public TimeSpan RealTimeToSimulate { get; set; }
    public int TotalEvents { get; set; }
    public int TotalCycles { get; set; }
    public int TotalSends { get; set; }
    public int TotalReceives { get; set; }
    public int TotalCollisions { get; set; }
    public int TotalSkippedCycles { get; set; }
    public int TotalBadCycles { get; set; }

    public List<Frame> AllFrames { get; set; } = new();
    public List<SignalResult> AllSignals { get; set; } = new();
    public Dictionary<int, CycleResult> CycleResults { get; set; } = new();

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
            var delta = new SimulationDelta(time, Simulation.Instance.CurrentCycle);
            AllDeltas.Add(time, delta);
            return delta;
        }

        return value;
    }

    public void AddSensorDelta(SensorDelta delta, bool force)
    {
        if (Simulation.Instance.SimulationSettings.CreateAllDeltas || force)
        {
            var simulationDelta = GetOrCreateSimulationDelta(Simulation.Instance.Time);
            simulationDelta.SensorDeltas.Add(delta);
        }
    }

    public void AddSignalDelta(SignalDelta delta, DateTime time, bool force)
    {
        if (Simulation.Instance.SimulationSettings.CreateAllDeltas || force)
        {
            var simulationDelta = GetOrCreateSimulationDelta(time);
            simulationDelta.SignalDeltas.Add(delta);
        }
    }

    public void AddFrame(Frame frame, bool force)
    {
        if (Simulation.Instance.SimulationSettings.CreateAllDeltas || force)
        {
            AllFrames.Add(frame);
        }
    }

    public void AddSignalResult(SignalResult signal, bool force)
    {
        if (Simulation.Instance.SimulationSettings.CreateAllDeltas || force)
        {
            AllSignals.Add(signal);
        }
    }

    public CycleResult GetOrCreateCycleResult(int cycle)
    {
        if (!CycleResults.TryGetValue(cycle, out var cycleResult))
        {
            cycleResult = new() { CycleId = cycle };
            CycleResults.Add(cycle, cycleResult);
        }

        return cycleResult;
    }

    public ClusterCycleResult GetOrCreateClusterCycleResult(int cycle, int clusterId)
    {
        var cycleResult = GetOrCreateCycleResult(cycle);

        if (!cycleResult.ClusterResults.TryGetValue(clusterId, out var clusterCycleResult))
        {
            clusterCycleResult = new ClusterCycleResult { ClusterId = clusterId };

            cycleResult.ClusterResults.Add(clusterId, clusterCycleResult);
        }

        return clusterCycleResult;
    }
}
