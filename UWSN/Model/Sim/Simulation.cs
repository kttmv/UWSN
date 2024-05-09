using System.Numerics;
using Newtonsoft.Json;
using UWSN.Model.Modems;
using UWSN.Utilities;

namespace UWSN.Model.Sim;

public class Simulation
{
    private const int MAX_PROCESSED_EVENTS = 1_000_000;
    public const int MAX_CYCLES = 1_000;

    private const int PRINT_EVERY_NTH_EVENT = 1000;

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
    public void Run(bool verbose)
    {
        var timeStart = DateTime.Now;

        Result = new SimulationResult();

        foreach (var sensor in Instance.Environment.Sensors)
        {
            sensor.WakeUp();
        }

        int eventNumber = 1;
        while (eventNumber < MAX_PROCESSED_EVENTS)
        {
            if (!verbose && eventNumber % PRINT_EVERY_NTH_EVENT == 0)
            {
                Console.WriteLine($"Обработано событий: {eventNumber}.");
                Console.WriteLine($"Текущее время симуляции: {Time:dd.MM.yyyy HH:mm:ss.fff}");
                Console.WriteLine($"Текущий цикл сбора данных: {CurrentCycle}");
            }

            var eventToInvoke = EventManager.PopFirst();
            if (eventToInvoke == null)
            {
                Logger.ShouldWriteToConsole = true;
                Logger.WriteLine("Больше событий нет.");
                break;
            }

            Time = eventToInvoke.Time;

            Logger.WriteLine($"\n=============================");
            Logger.WriteLine($"Событие №{eventNumber}. {eventToInvoke.Description}", true);

            // обрабатываем событие
            eventToInvoke.Invoke();
            eventNumber++;

            // проверяем, жива ли сеть
            int deadSensorsCount = Environment.Sensors.Where(s => s.IsDead).Count();

            if (deadSensorsCount / (double)Environment.Sensors.Count >= DeadSensorsPercent)
            {
                Logger.ShouldWriteToConsole = true;
                Logger.WriteLine("Сеть мертва");

                break;
            }
        }

        if (eventNumber >= MAX_PROCESSED_EVENTS)
        {
            Logger.ShouldWriteToConsole = true;
            Logger.WriteLine("Был достигнут лимит событий.");
        }

        Logger.WriteLine("\nСимуляция остановлена.", true);

        Result.TotalEvents = eventNumber;
        Result.TotalCycles = CurrentCycle;

        var timeFinish = DateTime.Now;
        Result.RealTimeToSimulate = timeFinish - timeStart;

        PrintResults();
    }

    public void PrintResults()
    {
        Logger.WriteLine($"\n=============================");
        Logger.WriteLine("Результаты симуляции:");

        Logger.WriteLine(
            $"Реальное время, потраченное на симуляцию: "
                + $"{Result!.RealTimeToSimulate:hh\\:mm\\:ss}"
        );

        Logger.WriteLine(
            $"\tКоличество обработанных событий: {Result.TotalEvents}"
                + (Result.TotalEvents >= MAX_PROCESSED_EVENTS ? " (был достигнут лимит)" : "")
        );

        Logger.WriteLine(
            $"\tКоличество отработанных сетью циклов: {Result.TotalCycles}"
                + (Result.TotalCycles >= MAX_CYCLES ? " (был достигнут лимит)" : "")
        );

        Logger.WriteLine($"\tКоличество отправленных сообщений: {Result.TotalSends}");
        Logger.WriteLine($"\tКоличество полученных сообщений: {Result.TotalReceives}");
        Logger.WriteLine($"\tКоличество коллизий: {Result.TotalCollisions}");
    }

    public void Clusterize()
    {
        SensorSettings.ClusterizationAlgorithm.Clusterize();
    }
}
