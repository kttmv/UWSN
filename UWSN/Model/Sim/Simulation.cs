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
    public int DeadSensorsCount { get; set; } = 0;

    [JsonIgnore]
    private int EventNumber { get; set; } = 0;

    [JsonIgnore]
    private bool Verbose { get; set; } = false;

    [JsonIgnore]
    private List<CycleData> Cycles { get; set; } = new();

    [JsonIgnore]
    private Dictionary<Sensor, double> BatteriesPreCycle { get; set; } = new();

    [JsonIgnore]
    private DateTime TimeBeforeCycle { get; set; } = default;

    [JsonIgnore]
    private Dictionary<Sensor, double> BatteriesPostCycle { get; set; } =
        new Dictionary<Sensor, double>();

    [JsonIgnore]
    private DateTime TimeAfterCycle { get; set; } = default;

    [JsonIgnore]
    private int NextSkipCycle { get; set; } = 0;

    /// <summary>
    /// Количество циклов подряд, во время которых до каких-либо
    /// референсов не дошла информация с датчиков.
    /// </summary>
    private int BadCycles { get; set; }

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

        Verbose = verbose;

        WakeSensorsUp();

        while (EventNumber < SimulationSettings.MaxProcessedEvents)
        {
            if (CurrentCycle == 0 && EventNumber % SimulationSettings.PrintEveryNthEvent == 0)
                PrintCurrentState(EventNumber);

            var eventToInvoke = EventManager.PopFirst();
            if (eventToInvoke == null)
            {
                Logger.WriteLine("Больше событий нет.", false, true);

                PrintCurrentState(EventNumber);

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
                    break;
                }

                CheckLastCycleIsBad();

                // TODO: вынести константу в свойство
                if (BadCycles >= 10)
                    break;

                CurrentCycle++;

                CollectSensorsData();

                continue;
            }

            Time = eventToInvoke.Time;
            PrintEventHeader(EventNumber, eventToInvoke);

            // обрабатываем событие
            eventToInvoke.Invoke();
            EventNumber++;

            // проверяем, мертва ли сеть
            if (CheckNetworkIsDead())
                break;
        }

        Result.TotalEvents = EventNumber;
        Result.TotalCycles = CurrentCycle;

        var timeFinish = DateTime.Now;
        Result.RealTimeToSimulate = timeFinish - timeStart;

        PrintResults();
    }

    private void CheckLastCycleIsBad()
    {
        bool isBad = false;

        foreach (var sensor in Environment.Sensors)
        {
            if (!sensor.IsReference.HasValue)
                throw new Exception("У сенсора должна быть вычислена кластеризация");

            if (!sensor.IsReference.Value)
                continue;

            var currentCycleData = sensor
                .ReceivedData.GroupBy(d => d.CycleId)
                .Where(group => group.Key == CurrentCycle)
                .FirstOrDefault();

            if (currentCycleData == null)
            {
                isBad = true;
                break;
            }

            var sensorsInCluster = Environment
                .Sensors.Where(s => s.ClusterId == sensor.ClusterId)
                .ToList();

            foreach (var data in currentCycleData)
            {
                if (sensorsInCluster.FirstOrDefault(s => s.Id == data.SensorId) == null)
                {
                    isBad = true;
                    break;
                }
            }

            if (isBad)
            {
                break;
            }
        }

        if (isBad)
        {
            BadCycles++;
            Logger.WriteLine(
                "\nБыл обнаружен плохой цикл: не все референсные "
                    + "узлы получили данные со всех сенсоров "
                    + "своего кластера. Текущее количество "
                    + $"плохих циклов подряд: {BadCycles}",
                false,
                true
            );
        }
        else
        {
            BadCycles = 0;
        }
    }

    private bool CheckNetworkIsDead()
    {
        bool result = false;

        if (
            DeadSensorsCount / (double)Environment.Sensors.Count
            >= (double)SimulationSettings.DeadSensorsPercent / 100
        )
        {
            Logger.WriteLine("Сеть мертва", false, true);
            result = true;
        }

        return result;
    }

    private void PrintCurrentState(int eventNumber)
    {
        Logger.WriteLine($"\nОбработано событий: {eventNumber}.", false, true);
        Logger.WriteLine($"Текущее время симуляции: {Time:dd.MM.yyyy HH:mm:ss.fff}", false, true);
        Logger.WriteLine($"Текущий цикл сбора данных: {CurrentCycle}", false, true);

        int deadCount = Environment.Sensors.Where(s => s.IsDead).Count();
        double deadPercentage = (double)deadCount / Environment.Sensors.Count * 100;

        Logger.WriteLine(
            $"Количество мертвых сенсоров: {deadCount} ({deadPercentage:0.#} %)",
            false,
            true
        );

        double totalInitialEnergy = Environment.Sensors.Count * SensorSettings.InitialSensorBattery;
        double totalEnergy = 0;
        foreach (var sensor in Environment.Sensors)
        {
            totalEnergy += sensor.Battery;
        }

        double totalEnergyPercentage = (double)totalEnergy / totalInitialEnergy * 100;

        Logger.WriteLine(
            "Остаточное количество энергии в сенсорах: "
                + $"{totalEnergy} Дж ({totalEnergyPercentage:0.#} %)",
            false,
            true
        );
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
        Logger.WriteLine($"\n=============================", false, false);
        Logger.WriteLine($"Событие №{eventNumber}. {eventToInvoke.Description}", true, false);
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
                double cost =
                    BatteriesPreCycle.First(b => b.Key.Id == sensor.Key.Id).Value - sensor.Value;
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
                    double newBc =
                        averageBatteryChanges.First(b => b.Key.Id == id).Value
                        + batteryChange.Value;

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

                double averageCost = averageCycle
                    .BatteryChange.First(b => b.Key.Id == sensor.Id)
                    .Value;
                double availableBattery =
                    sensor.Battery - SensorSettings.BatteryDeadCharge - averageCost * 2;

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

                    double cycleCost = averageCycle
                        .BatteryChange.First(i => i.Key.Id == sensor.Id)
                        .Value;
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

                Logger.WriteLine($"\nБыло пропущено {minSkipsCount} циклов.", false, true);
            }
        }
    }

    private void AddSensorBatteryDelta(Sensor sensor)
    {
        Result!.AddSensorDelta(
            new SimulationDelta.SensorDelta { Id = sensor.Id, Battery = sensor.Battery },
            true
        );
    }

    public void PrintResults()
    {
        if (EventNumber >= SimulationSettings.MaxProcessedEvents)
            Logger.WriteLine("Был достигнут лимит событий.", false, true);

        if (CurrentCycle >= SimulationSettings.MaxCycles)
            Logger.WriteLine("Достигнуто максимальное количество циклов.", false, true);

        // TODO: вынести константу в свойство
        if (BadCycles >= 10)
            Logger.WriteLine($"Было обнаружено {BadCycles} плохих циклов подряд.", false, true);

        Logger.WriteLine("\nСимуляция остановлена.", true, true);
        Logger.WriteLine($"\n=============================", false, true);
        Logger.WriteLine("Результаты симуляции:", false, true);

        Logger.WriteLine(
            $"\tРеальное время, потраченное на симуляцию: "
                + $"{Result!.RealTimeToSimulate:hh\\:mm\\:ss}",
            false,
            true
        );

        Logger.WriteLine(
            $"\tКоличество обработанных событий: {Result.TotalEvents}"
                + (
                    Result.TotalEvents >= SimulationSettings.MaxProcessedEvents
                        ? " (был достигнут лимит)"
                        : ""
                ),
            false,
            true
        );

        Logger.WriteLine(
            $"\tКоличество отработанных сетью циклов: {Result.TotalCycles}"
                + (
                    Result.TotalCycles >= SimulationSettings.MaxCycles
                        ? " (был достигнут лимит)"
                        : ""
                ),
            false,
            true
        );

        Logger.WriteLine($"\tКоличество отправленных сообщений: {Result.TotalSends}", false, true);
        Logger.WriteLine($"\tКоличество полученных сообщений: {Result.TotalReceives}", false, true);

        Logger.WriteLine($"\tКоличество коллизий: {Result.TotalCollisions}", false, true);
    }
}
