using System.Numerics;
using Dew.Math;
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

    [JsonIgnore]
    private DateTime _time;

    /// <summary>
    /// Текущее время симуляции
    /// </summary>
    [JsonIgnore]
    public DateTime Time
    {
        get { return _time; }
        set
        {
            if (value < _time)
                throw new Exception("Невозможно назначить время меньше текущего");
            _time = value;
        }
    }

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

    [JsonIgnore]
    public int DeadSensorsCount { get; set; }

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

        var cycles = new List<CycleData>();
        var batteriesPreCycle = new Dictionary<Sensor, double>();
        var timeBeforeCycle = Instance.Time;
        var batteriesPostCycle = new Dictionary<Sensor, double>();
        DateTime timeAfterCycle = default;
        int nextSkipCycle = Instance.SimulationSettings.CyclesCountBeforeSkip;

        int eventNumber = 1;
        while (eventNumber < SimulationSettings.MaxProcessedEvents)
        {
            if (!verbose && eventNumber % SimulationSettings.PrintEveryNthEvent == 0)
            {
                Console.WriteLine($"\nОбработано событий: {eventNumber}.");
                Console.WriteLine($"Текущее время симуляции: {Time:dd.MM.yyyy HH:mm:ss.fff}");
                Console.WriteLine($"Текущий цикл сбора данных: {CurrentCycle}");

                int deadCount = Environment.Sensors.Where(s => s.IsDead).Count();
                double deadPercentage = (double)deadCount / Environment.Sensors.Count * 100;

                Console.WriteLine($"Количество мертвых сенсоров: {deadCount} ({deadPercentage:0.#} %)");

                double totalInitialEnergy = Environment.Sensors.Count * SensorSettings.InitialSensorBattery;
                double totalEnergy = 0;
                foreach (var sensor in Environment.Sensors)
                {
                    totalEnergy += sensor.Battery;
                }

                double totalEnergyPercentage = (double)totalEnergy / totalInitialEnergy * 100;

                Console.WriteLine($"Остаточное количество энергии в сенсорах: {totalEnergy} Дж ({totalEnergyPercentage:0.#} %)");
            }

            var eventToInvoke = EventManager.PopFirst();
            if (eventToInvoke == null)
            {
                Logger.WriteLine("Больше событий нет.");

                if (CurrentCycle == 0)
                {
                    foreach (var s in Instance.Environment.Sensors)
                    {
                        batteriesPreCycle.Add(s, s.Battery);
                    }
                    timeBeforeCycle = Instance.Time;
                }
                else if (CurrentCycle <= nextSkipCycle)
                {
                    batteriesPostCycle.Clear();
                    foreach (var s in Instance.Environment.Sensors)
                    {
                        batteriesPostCycle.Add(s, s.Battery);
                    }
                    timeAfterCycle = Instance.Time;

                    var cycleCosts = new Dictionary<Sensor, double>();
                    var cycleData = new CycleData();

                    foreach (var s in batteriesPostCycle)
                    {
                        double cost = batteriesPreCycle.First(b => b.Key.Id == s.Key.Id).Value - s.Value;
                        cycleData.BatteryChange.Add(s.Key, cost);
                    }

                    var cycleTimeSpent = timeAfterCycle - timeBeforeCycle;

                    cycleData.CycleTime = cycleTimeSpent;
                    cycles.Add(cycleData);

                    batteriesPreCycle.Clear();
                    foreach (var s in Instance.Environment.Sensors)
                    {
                        batteriesPreCycle.Add(s, s.Battery);
                    }
                    timeBeforeCycle = Instance.Time;
                }
                if (CurrentCycle == nextSkipCycle)
                {
                    var avgCycle = new CycleData();
                    var avgBatteriesChange = new Dictionary<Sensor, double>();
                    foreach (var s in Instance.Environment.Sensors)
                    {
                        avgBatteriesChange.Add(s, 0.0);
                    }
                    var avgTimeSpan = TimeSpan.Zero;

                    // сложить всё
                    foreach (var cycle in cycles)
                    {
                        avgTimeSpan += cycle.CycleTime;
                        foreach (var bc in cycle.BatteryChange)
                        {
                            int id = bc.Key.Id;
                            double newBc = avgBatteriesChange.First(b => b.Key.Id == id).Value + bc.Value;
                            avgBatteriesChange.Remove(bc.Key);
                            avgBatteriesChange.Add(bc.Key, newBc);
                        }
                    }

                    // среднее арифметическое
                    var temp = new Dictionary<Sensor, double>();
                    foreach (var avgBc in avgBatteriesChange)
                    {
                        double newAvg = avgBc.Value / cycles.Count;
                        temp.Add(avgBc.Key, newAvg);
                    }
                    avgTimeSpan /= cycles.Count;

                    avgCycle.BatteryChange = temp;
                    avgCycle.CycleTime = avgTimeSpan;

                    // сам процесс пропуска циклов
                    int minSkipsCount = int.MaxValue;
                    foreach (var s in Instance.Environment.Sensors)
                    {
                        if (s.IsDead)
                            continue;

                        double avgCost = avgCycle.BatteryChange.First(b => b.Key.Id == s.Id).Value;
                        double availableBattery = s.Battery - SensorSettings.BatteryDeadCharge - avgCost * 2;
                        int skippedCycles = (int)Math.Floor(availableBattery / avgCost);
                        if (skippedCycles < minSkipsCount)
                            minSkipsCount = skippedCycles;

                        if (minSkipsCount == 0)
                            break;
                    }

                    //var maxAvgCost = avgCycle.BatteryChange.Where(b => !b.Key.IsDead).OrderBy(b => b.Value).Last();
                    // maxAvgCost.Value * 2 - это сколько батареи мы хотим оставить,
                    // чтобы досчитать остаток вручную и посмотреть на то, как умирают сенсоры

                    if (minSkipsCount > 0 && minSkipsCount != int.MaxValue)
                    {
                        foreach (var s in Instance.Environment.Sensors)
                        {
                            if (s.IsDead)
                                continue;

                            double cycleCost = avgCycle.BatteryChange.First(i => i.Key.Id == s.Id).Value;
                            s.Battery -= cycleCost * minSkipsCount;
                        }

                        Instance.Time += avgCycle.CycleTime * minSkipsCount;
                        CurrentCycle += minSkipsCount;
                        // номер цикла, на котором будет следующая попытка пропуска циклов
                        nextSkipCycle = CurrentCycle + Instance.SimulationSettings.CyclesCountBeforeSkip;

                        cycles.Clear();
                        timeAfterCycle = default;
                        batteriesPreCycle.Clear();
                        batteriesPreCycle.Clear();
                        foreach (var s in Instance.Environment.Sensors)
                        {
                            batteriesPreCycle.Add(s, s.Battery);
                        }
                        timeBeforeCycle = Instance.Time;
                    }
                }

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
            if (
                DeadSensorsCount / (double)Environment.Sensors.Count
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