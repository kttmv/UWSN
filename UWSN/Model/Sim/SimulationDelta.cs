﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int SignalId { get; set; }
        public SignalDeltaType Type { get; set; }

        public SignalDelta(int signalId, SignalDeltaType type)
        {
            SignalId = signalId;
            Type = type;
        }
    }

    public DateTime Time { get; set; }
    public List<SignalDelta> SignalDeltas { get; set; }

    public SimulationDelta(DateTime time)
    {
        SignalDeltas = new();
        Time = time;
    }
}
