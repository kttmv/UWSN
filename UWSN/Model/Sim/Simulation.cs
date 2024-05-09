using System.Numerics;
using Newtonsoft.Json;
using UWSN.Model.Modems;
using UWSN.Utilities;

namespace UWSN.Model.Sim;

public class Simulation
{
    private const int MAX_PROCESSED_EVENTS = 1_000_000;
    public const int MAX_CYCLES = 1_000;

    #region Simulation Singleton

    private static Simulation? SimulationInstance { get; set; }

    public static Simulation Instance
    {
        get
        {
            if (SimulationInstance == null)
            {
                throw new Exception("Экземпляр класса Simulation не был создан.");
            }

            return SimulationInstance;
        }
    }

    #endregion Simulation Singleton

    #region Properties

    public SensorSettings SensorSettings { get; set; } = new();

    [JsonIgnore]
    public List<ModemBase> AvailableModems { get; set; } =
        new List<ModemBase>()
        {
            new AquaCommMako(),
            new AquaCommMarlin(),
            new AquaCommOrca(),
            new AquaModem1000(),
            new AquaModem500(),
            new MicronModem(),
            new SMTUTestModem()
        };

    /// <summary>
    /// % мёртвых сенсоров, при достижении которого мы считаем сеть мертвой
    /// </summary>
    public double DeadSensorsPercent { get; set; } = 0.33;

    [JsonIgnore]
    public int CurrentCycle { get; set; } = 0;

    public Vector3Range AreaLimits { get; set; } =
        new() { Min = new Vector3(0, 0, 0), Max = new Vector3(10_000, 10_000, 10_000) };

    public bool ShouldSkipHello { get; set; } = false;

    public ChannelManager ChannelManager { get; set; } = new();

    /// <summary>
    /// Окружение симуляции
    /// </summary>
    public Environment Environment { get; set; } = new();

    public SimulationResult? Result { get; set; } = null;

    [JsonIgnore]
    public EventManager EventManager { get; set; } = new();

    /// <summary>
    /// Текущее время симуляции
    /// </summary>
    [JsonIgnore]
    public DateTime Time { get; set; }

    #endregion Properties

    public Simulation()
    {
        if (SimulationInstance != null)
            throw new Exception("Экземпляр класса Simulation уже создан.");

        SimulationInstance = this;
    }

    /// <summary>
    /// Метод запуска симуляции
    /// </summary>
    public void Run()
    {
        Result = new SimulationResult();

        foreach (var sensor in Instance.Environment.Sensors)
        {
            sensor.WakeUp();
        }

        int i = 1;
        while (i < MAX_PROCESSED_EVENTS)
        {
            var e = EventManager.RemoveFirst();

            if (e == null)
            {
                Logger.WriteLine("Больше событий нет.");
                break;
            }

            Time = e.Time;

            Logger.WriteLine($"=============================");
            Logger.WriteLine($"Событие №{i}. {e.Description}", true);

            e.Invoke();

            Logger.WriteLine("");

            i++;

            if (
                Environment.Sensors.Where(s => s.IsDead).Count() / (double)Environment.Sensors.Count
                >= DeadSensorsPercent
            )
            {
                Logger.WriteLine("Сеть мертва");

                break;
            }
        }

        if (i >= MAX_PROCESSED_EVENTS)
        {
            Logger.WriteLine("Был достигнут лимит событий.");
        }

        Logger.WriteLine("");
        Logger.WriteLine("Симуляция остановлена.", true);

        Logger.WriteLine("");
        Logger.WriteLine($"=============================");
        Logger.WriteLine("Результаты симуляции:");
        Logger.WriteLine($"\tКоличество отправленных сообщений: {Result.TotalSends}");
        Logger.WriteLine($"\tКоличество полученных сообщений: {Result.TotalReceives}");
        Logger.WriteLine($"\tКоличество коллизий: {Result.TotalCollisions}");
        Logger.WriteLine($"\tКоличество отработанных циклов: {CurrentCycle}");
    }

    public void Clusterize()
    {
        SensorSettings.ClusterizationAlgorithm.Clusterize();
    }
}
