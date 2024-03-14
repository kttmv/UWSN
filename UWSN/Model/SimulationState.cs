using UWSN.Model.Sim;

namespace UWSN.Model;

public class SimulationState
{
    public DateTime Time { get; set; }

    public Event CurrentEvent { get; set; }

    public SimulationState(DateTime time, Event currentEvent)
    {
        Time = time;
        CurrentEvent = currentEvent;

    }
}
