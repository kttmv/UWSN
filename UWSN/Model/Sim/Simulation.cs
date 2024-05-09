using System.Numerics;
using Newtonsoft.Json;
using UWSN.Model.Modems;
using UWSN.Utilities;

namespace UWSN.Model.Sim;

public class Simulation
{
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

    public SimulationSettings SimulationSettings { get; set; } = new();

    public Vector3Range AreaLimits { get; set; } =
        new() { Min = new Vector3(0, 0, 0), Max = new Vector3(10_000, 10_000, 10_000) };

    public ChannelManager ChannelManager { get; set; } = new();

    /// <summary>
    /// Окружение симуляции
    /// </summary>
    public Environment Environment { get; set; } = new();

    public SimulationResult? Result { get; set; } = null;

    [JsonIgnore]
    public EventManager EventManager { get; set; } = new();

    [JsonIgnore]
    public int CurrentCycle { get; set; } = 0;

    /// <summary>
    /// Текущее время симуляции
    /// </summary>
    [JsonIgnore]
    public DateTime Time { get; set; }

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

    #endregion Properties

    public Simulation()
    {
        if (SimulationInstance != null)
            throw new Exception("Экземпляр класса Simulation уже создан.");

        SimulationInstance = this;
    }

    public void Clusterize()
    {
        SensorSettings.ClusterizationAlgorithm.Clusterize();
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
        while (eventNumber < SimulationSettings.MaxProcessedEvents)
        {
            if (!verbose && eventNumber % SimulationSettings.PrintEveryNthEvent == 0)
            {
                Console.WriteLine($"Обработано событий: {eventNumber}.");
                Console.WriteLine($"Текущее время симуляции: {Time:dd.MM.yyyy HH:mm:ss.fff}");
                Console.WriteLine($"Текущий цикл сбора данных: {CurrentCycle}");
            }

            var eventToInvoke = EventManager.PopFirst();
            if (eventToInvoke == null)
            {
                Logger.WriteLine("Больше событий нет.");

                if (CurrentCycle >= SimulationSettings.MaxCycles)
                {
                    Logger.ShouldWriteToConsole = true;
                    Logger.WriteLine("Достигнуто максимальное количество циклов.");
                    break;
                }

                CurrentCycle++;

                var time = Time.RoundUpToNearest(SensorSettings.SampleInterval);

                foreach (var sensor in Environment.Sensors.Shuffle())
                {
                    if (sensor.IsDead)
                        continue;

                    var e = new Event(
                        time,
                        $"Сбор данных с датчиков сенсором #{sensor.Id}",
                        () => sensor.CollectData()
                    );

                    sensor.AddEvent(e);
                }

                continue;
            }

            Time = eventToInvoke.Time;

            Logger.WriteLine($"\n=============================");
            Logger.WriteLine($"Событие №{eventNumber}. {eventToInvoke.Description}", true);

            // обрабатываем событие
            eventToInvoke.Invoke();
            eventNumber++;

            // проверяем, жива ли сеть
            int deadSensorsCount = Environment.Sensors.Where(s => s.IsDead).Count();

            if (
                deadSensorsCount / (double)Environment.Sensors.Count
                >= (double)SimulationSettings.DeadSensorsPercent / 100
            )
            {
                Logger.ShouldWriteToConsole = true;
                Logger.WriteLine("Сеть мертва");

                break;
            }
        }

        if (eventNumber >= SimulationSettings.MaxProcessedEvents)
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
            $"\tРеальное время, потраченное на симуляцию: "
                + $"{Result!.RealTimeToSimulate:hh\\:mm\\:ss}"
        );

        Logger.WriteLine(
            $"\tКоличество обработанных событий: {Result.TotalEvents}"
                + (
                    Result.TotalEvents >= SimulationSettings.MaxProcessedEvents
                        ? " (был достигнут лимит)"
                        : ""
                )
        );

        Logger.WriteLine(
            $"\tКоличество отработанных сетью циклов: {Result.TotalCycles}"
                + (
                    Result.TotalCycles >= SimulationSettings.MaxCycles
                        ? " (был достигнут лимит)"
                        : ""
                )
        );

        Logger.WriteLine($"\tКоличество отправленных сообщений: {Result.TotalSends}");
        Logger.WriteLine($"\tКоличество полученных сообщений: {Result.TotalReceives}");
        Logger.WriteLine($"\tКоличество коллизий: {Result.TotalCollisions}");
    }
}