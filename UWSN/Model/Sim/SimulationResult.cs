using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public int TotalSends { get; set; }
    public int TotalReceives { get; set; }
    public int TotalCollisions { get; set; }
    public List<Frame> AllFrames { get; set; }
    public List<SignalResult> AllSignals { get; set; }

    [JsonIgnore]
    public Dictionary<DateTime, SimulationDelta> AllDeltas { get; set; }

    public List<SimulationDelta> Deltas
    {
        get { return AllDeltas.ToList().Select(x => x.Value).OrderBy(x => x.Time).ToList(); }
    }

    public SimulationResult()
    {
        AllFrames = new();
        AllSignals = new();
        AllDeltas = new();
    }
}
