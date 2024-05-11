namespace UWSN.Model.Sim;

public class SimulationSettings
{
    public int MaxProcessedEvents { get; set; } = 5_000_000;

    public int MaxCycles { get; set; } = 1_000;

    public int PrintEveryNthEvent { get; set; } = 10_000;

    /// <summary>
    /// % мёртвых сенсоров, при достижении которого мы считаем сеть мертвой
    /// </summary>
    public int DeadSensorsPercent { get; set; } = 33;

    public bool ShouldSkipHello { get; set; } = false;

    public bool ShouldSkipCycles { get; set; } = false;

    public int CyclesCountBeforeSkip { get; set; } = 5;
}