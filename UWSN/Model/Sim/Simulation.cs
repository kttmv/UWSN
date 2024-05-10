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

    private List<CycleData> Cycles { get; set; } = new();
    private Dictionary<Sensor, double> BatteriesPreCycle { get; set; } = new();
    private DateTime TimeBeforeCycle { get; set; } = default;
    private Dictionary<Sensor, double> BatteriesPostCycle { get; set; } = new Dictionary<Sensor, double>();
    private DateTime TimeAfterCycle { get; set; } = default;
    private int NextSkipCycle { get; set; }

    #endregion Properties

    public Simulation()
    {
        if (SimulationInstance != null)
            throw new Exception("Экземпляр класса Simulation уже создан.");

        SimulationInstance = this;

        NextSkipCycle = SimulationSettings.CyclesCountBeforeSkip;
    }

    public void Clusterize()
    {
        SensorSettings.ClusterizationAlgorithm.Clusterize();
    }

    /// <summary>
    /// Метод запуска симуляции
    /// </summary>
    public void Run(bool verbose, bool fullResult)
    {
        var timeStart = DateTime.Now;
        SimulationResult.ShouldCreateAllDeltas = fullResult;

        Result = new SimulationResult();

        WakeSensorsUp();

        int eventNumber = 1;
        while (eventNumber < SimulationSettings.MaxProcessedEvents)
        {
            if (CurrentCycle == 0 && eventNumber % SimulationSettings.PrintEveryNthEvent == 0)
                PrintCurrentState(verbose, eventNumber);

            var eventToInvoke = EventManager.PopFirst();
            if (eventToInvoke == null)
            {
                Logger.WriteLine("Больше событий нет.");

                PrintCurrentState(verbose, eventNumber);

                // так как при !fullResult не сохраняются изменения
                // зарядов сенсоров, сохраняем их вручную после каждого
                // цикла.
                if (!fullResult)
                {
                    foreach (var sensor in Environment.Sensors)
                    {
                        AddSensorBatteryDelta(sensor);
                    }
                }

                CalculateCyclesSkip();

                if (CurrentCycle >= SimulationSettings.MaxCycles)
                {
                    Logger.ShouldWriteToConsole = true;
                    Logger.WriteLine("Достигнуто максимальное количество циклов.");
                    break;
                }

                CurrentCycle++;

                CollectSensorsData();

                continue;
            }

            Time = eventToInvoke.Time;
            PrintEventHeader(eventNumber, eventToInvoke);

            // обрабатываем событие
            eventToInvoke.Invoke();
            eventNumber++;

            // проверяем, мертва ли сеть
            if (CheckNetworkIsDead())
                break;
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

    private bool CheckNetworkIsDead()
    {
        bool result = false;

        if (
            DeadSensorsCount / (double)Environment.Sensors.Count
            >= (double)SimulationSettings.DeadSensorsPercent / 100
        )
        {
            Logger.ShouldWriteToConsole = true;
            Logger.WriteLine("Сеть мертва");
            result = true;
        }

        return result;
    }

    private void PrintCurrentState(bool verbose, int eventNumber)
    {
        if (!verbose)
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
    }

    private void WakeSensorsUp()
    {
        foreach (var sensor in Environment.Sensors)
        {
            sensor.WakeUp();
        }
    }

    private void CollectSensorsData()
    {
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
    }

    private static void PrintEventHeader(int eventNumber, Event eventToInvoke)
    {
        Logger.WriteLine($"\n=============================");
        Logger.WriteLine($"Событие №{eventNumber}. {eventToInvoke.Description}", true);
    }

    private void CalculateCyclesSkip()
    {
        if (CurrentCycle == 0)
        {
            foreach (var sensor in Environment.Sensors)
            {
                BatteriesPreCycle.Add(sensor, sensor.Battery);
            }

            TimeBeforeCycle = Time;
        }
        else if (CurrentCycle <= NextSkipCycle)
        {
            BatteriesPostCycle.Clear();

            foreach (var sensor in Environment.Sensors)
            {
                BatteriesPostCycle.Add(sensor, sensor.Battery);
            }

            TimeAfterCycle = Time;

            var cycleCosts = new Dictionary<Sensor, double>();
            var cycleData = new CycleData();

            foreach (var sensor in BatteriesPostCycle)
            {
                double cost = BatteriesPreCycle.First(b => b.Key.Id == sensor.Key.Id).Value - sensor.Value;
                cycleData.BatteryChange.Add(sensor.Key, cost);
            }

            var cycleTimeSpent = TimeAfterCycle - TimeBeforeCycle;

            cycleData.CycleTime = cycleTimeSpent;
            Cycles.Add(cycleData);

            BatteriesPreCycle.Clear();

            foreach (var sensor in Environment.Sensors)
            {
                BatteriesPreCycle.Add(sensor, sensor.Battery);
            }

            TimeBeforeCycle = Time;
        }

        if (CurrentCycle == NextSkipCycle)
        {
            var averageCycle = new CycleData();
            var averageBatteryChanges = new Dictionary<Sensor, double>();
            foreach (var sensor in Environment.Sensors)
            {
                averageBatteryChanges.Add(sensor, 0.0);
            }
            var averageTimeSpan = TimeSpan.Zero;

            // сложить всё
            foreach (var cycle in Cycles)
            {
                averageTimeSpan += cycle.CycleTime;
                foreach (var batteryChange in cycle.BatteryChange)
                {
                    int id = batteryChange.Key.Id;
                    double newBc = averageBatteryChanges.First(b => b.Key.Id == id).Value + batteryChange.Value;

                    averageBatteryChanges.Remove(batteryChange.Key);
                    averageBatteryChanges.Add(batteryChange.Key, newBc);
                }
            }

            // среднее арифметическое
            var temp = new Dictionary<Sensor, double>();
            foreach (var average in averageBatteryChanges)
            {
                double newAverage = average.Value / Cycles.Count;
                temp.Add(average.Key, newAverage);
            }

            averageTimeSpan /= Cycles.Count;

            averageCycle.BatteryChange = temp;
            averageCycle.CycleTime = averageTimeSpan;

            // подсчет количества пропусков
            int minSkipsCount = int.MaxValue;
            foreach (var sensor in Environment.Sensors)
            {
                if (sensor.IsDead)
                    continue;

                double averageCost = averageCycle.BatteryChange.First(b => b.Key.Id == sensor.Id).Value;
                double availableBattery = sensor.Battery - SensorSettings.BatteryDeadCharge - averageCost * 2;

                int skippedCycles = (int)Math.Floor(availableBattery / averageCost);

                if (skippedCycles < minSkipsCount)
                    minSkipsCount = skippedCycles;

                if (minSkipsCount == 0)
                    break;
            }

            //var maxAvgCost = avgCycle.BatteryChange.Where(b => !b.Key.IsDead).OrderBy(b => b.Value).Last();
            // maxAvgCost.Value * 2 - это сколько батареи мы хотим оставить,
            // чтобы досчитать остаток вручную и посмотреть на то, как умирают сенсоры

            // сам процесс пропуска циклов
            if (minSkipsCount > 0 && minSkipsCount != int.MaxValue)
            {
                foreach (var sensor in Environment.Sensors)
                {
                    if (sensor.IsDead)
                        continue;

                    double cycleCost = averageCycle.BatteryChange.First(i => i.Key.Id == sensor.Id).Value;
                    sensor.Battery -= cycleCost * minSkipsCount;

                    for (int i = 0; i < minSkipsCount; i++)
                    {
                        AddSensorBatteryDelta(sensor);
                    }
                }

                Time += averageCycle.CycleTime * minSkipsCount;
                CurrentCycle += minSkipsCount;

                // номер цикла, на котором будет следующая попытка пропуска циклов
                NextSkipCycle = CurrentCycle + SimulationSettings.CyclesCountBeforeSkip;

                Cycles.Clear();
                TimeAfterCycle = default;
                BatteriesPreCycle.Clear();
                BatteriesPreCycle.Clear();

                foreach (var sensor in Environment.Sensors)
                {
                    BatteriesPreCycle.Add(sensor, sensor.Battery);
                }

                TimeBeforeCycle = Time;

                Console.WriteLine($"\nБыло пропущено {minSkipsCount} циклов.");
            }
        }
    }

    private void AddSensorBatteryDelta(Sensor sensor)
    {
        Result!.AddSensorDelta(
            new SimulationDelta.SensorDelta
            {
                Id = sensor.Id,
                Battery = sensor.Battery
            },
            true);
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