namespace UWSN.Model.Sim;

public class SimulationSettings
{
    public int MaxProcessedEvents { get; set; } = 0;

    public int MaxCycles { get; set; } = 0;

    /// <summary>
    /// % мёртвых сенсоров, при достижении которого мы считаем сеть мертвой
    /// </summary>
    public int DeadSensorsPercent { get; set; } = 33;

    public bool ShouldSkipHello { get; set; } = false;

    public bool ShouldSkipCycles { get; set; } = false;

    public int CyclesCountBeforeSkip { get; set; } = 50;

    public int PrintEveryNthEvent { get; set; } = 10_000;

    public bool Verbose { get; set; } = false;

    public bool CreateAllDeltas { get; set; } = false;

    public bool SaveOutput { get; set; } = false;

    /// <summary>
    /// Скорость ветра (м/с) для уравнения пассивного сонара модели вероятности
    /// </summary>
    public double PassiveSonarEqParameterW { get; set; } = 0.0;

    /// <summary>
    /// Фактор судоходства (безразмерная величина) для уравнения пассивного сонара модели вероятности
    /// </summary>
    public double PassiveSonarEqParameterS { get; set; } = 0.5;
}
