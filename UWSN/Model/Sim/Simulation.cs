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

    public SimulationSettings SimulationSettings { get; set; } = new();

    public SensorSettings SensorSettings { get; set; } = new();

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
    public bool Verbose { get; set; } = false;

    [JsonIgnore]
    private List<CycleData> Cycles { get; set; } = new();

    [JsonIgnore]
    private Dictionary<Sensor, double> BatteriesPreCycle { get; set; } = new();

    [JsonIgnore]
    private DateTime TimeBeforeCycle { get; set; } = default;

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
    }

    public void Clusterize()
    {
        SensorSettings.ClusterizationAlgorithm.Clusterize();
    }

    /// <summary>
    /// Метод запуска симуляции
    /// </summary>
    public void Run(bool fullResult)
    {
        var timeStart = DateTime.Now;
        SimulationResult.ShouldCreateAllDeltas = fullResult;

        Result = new SimulationResult();

        WakeSensorsUp();

        while (EventNumber < SimulationSettings.MaxProcessedEvents || SimulationSettings.MaxProcessedEvents <= 0)
        {
            if (CurrentCycle == 0 && EventNumber % SimulationSettings.PrintEveryNthEvent == 0)
                PrintCurrentState();

            var eventToInvoke = EventManager.PopFirst();
            if (eventToInvoke == null)
            {
                if (Verbose)
                    Logger.WriteLine("Больше событий нет.");

                PrintCurrentState();

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

        Result.SimulationEndTime = Time;

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
            Result!.TotalBadCycles++;
            Logger.WriteLine(
                "\nБыл обнаружен плохой цикл: не все референсные "
                    + "узлы получили данные со всех сенсоров "
                    + "своего кластера. Текущее количество "
                    + $"плохих циклов подряд: {BadCycles}",
                false
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
            result = true;
        }

        return result;
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

    private void CalculateCyclesSkip()
    {
        if (!SimulationSettings.ShouldSkipCycles)
            return;

        if (CurrentCycle == 0)
        {
            foreach (var sensor in Environment.Sensors)
            {
                BatteriesPreCycle.Add(sensor, sensor.Battery);
            }

            TimeBeforeCycle = Time;
            NextSkipCycle = SimulationSettings.CyclesCountBeforeSkip;

            return;
        }

        if (CurrentCycle <= NextSkipCycle)
        {
            TimeAfterCycle = Time;

            var currentCycleData = new CycleData();

            foreach (var sensor in Environment.Sensors)
            {
                double cost = BatteriesPreCycle[sensor] - sensor.Battery;
                currentCycleData.BatteryChange.Add(sensor, cost);
            }

            currentCycleData.CycleTime = TimeAfterCycle - TimeBeforeCycle;
            Cycles.Add(currentCycleData);

            foreach (var sensor in Environment.Sensors)
            {
                BatteriesPreCycle[sensor] = sensor.Battery;
            }

            TimeBeforeCycle = Time;

            return;
        }

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
        int cyclesToSkip = int.MaxValue;
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

            if (skippedCycles < cyclesToSkip)
                cyclesToSkip = skippedCycles;

            if (cyclesToSkip == 0)
                break;
        }

        //var maxAvgCost = avgCycle.BatteryChange.Where(b => !b.Key.IsDead).OrderBy(b => b.Value).Last();
        // maxAvgCost.Value * 2 - это сколько батареи мы хотим оставить,
        // чтобы досчитать остаток вручную и посмотреть на то, как умирают сенсоры

        // сам процесс пропуска циклов
        if (cyclesToSkip > 0 && cyclesToSkip != int.MaxValue)
        {
            foreach (var sensor in Environment.Sensors)
            {
                if (sensor.IsDead)
                    continue;

                double cycleCost = averageCycle
                    .BatteryChange.First(i => i.Key.Id == sensor.Id)
                    .Value;
                sensor.Battery -= cycleCost * cyclesToSkip;

                for (int i = 0; i < cyclesToSkip; i++)
                {
                    AddSensorBatteryDelta(sensor);
                }
            }

            Time += averageCycle.CycleTime * cyclesToSkip;
            CurrentCycle += cyclesToSkip;
            Result!.TotalSkippedCycles += cyclesToSkip;

            // номер цикла, на котором будет следующая попытка пропуска циклов
            NextSkipCycle = CurrentCycle + SimulationSettings.CyclesCountBeforeSkip + 2;

            Cycles.Clear();
            TimeAfterCycle = default;
            BatteriesPreCycle.Clear();
            BatteriesPreCycle.Clear();

            foreach (var sensor in Environment.Sensors)
            {
                BatteriesPreCycle.Add(sensor, sensor.Battery);
            }

            TimeBeforeCycle = Time;

            Logger.WriteLine($"\nБыло пропущено {cyclesToSkip} циклов.");
        }
    }

    private void AddSensorBatteryDelta(Sensor sensor)
    {
        Result!.AddSensorDelta(
            new SimulationDelta.SensorDelta { Id = sensor.Id, Battery = sensor.Battery },
            true
        );
    }

    private void PrintEventHeader(int eventNumber, Event eventToInvoke)
    {
        if (Verbose)
        {
            Logger.WriteLine($"\n=============================");
            Logger.WriteLine($"Событие №{eventNumber}. {eventToInvoke.Description}", true);
        }
    }

    private void PrintCurrentState()
    {
        Logger.WriteLine($"\nОбработано событий: {EventNumber}.");
        Logger.WriteLine($"Текущее время симуляции: {Time:dd.MM.yyyy HH:mm:ss.fff}");
        Logger.WriteLine($"Текущий цикл сбора данных: {CurrentCycle}");

        int deadCount = Environment.Sensors.Where(s => s.IsDead).Count();
        double deadPercentage = (double)deadCount / Environment.Sensors.Count * 100;

        Logger.WriteLine(
            $"Количество мертвых сенсоров: {deadCount} ({deadPercentage:0.#} %)",
            false
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
            false
        );
    }

    public void PrintResults()
    {
        PrintCurrentState();

        Logger.WriteLine($"\n=============================");
        Logger.WriteLine("\nСимуляция остановлена.");

        if (EventNumber >= SimulationSettings.MaxProcessedEvents)
            Logger.WriteLine("\nБыл достигнут лимит событий.");

        if (CurrentCycle >= SimulationSettings.MaxCycles)
            Logger.WriteLine("\nДостигнуто максимальное количество циклов.");

        if (CheckNetworkIsDead())
            Logger.WriteLine("\nСеть мертва");

        // TODO: вынести константу в свойство
        if (BadCycles >= 10)
            Logger.WriteLine($"\nБыло обнаружено {BadCycles} плохих циклов подряд.");

        Logger.WriteLine($"\n=============================");
        Logger.WriteLine("Результаты симуляции:\n");

        Logger.LeftPadding++;

        Logger.WriteLine(
            $"Реальное время, потраченное на симуляцию: {Result!.RealTimeToSimulate:hh\\:mm\\:ss}",
            false
        );

        Logger.WriteLine($"Конечное время симуляции: {Result.SimulationEndTime:dd.MM.yyyy HH:mm:ss.fff}");

        Logger.WriteLine("");

        Logger.WriteLine(
            $"Количество обработанных событий: {Result.TotalEvents}"
                + (
                    Result.TotalEvents >= SimulationSettings.MaxProcessedEvents
                        ? " (был достигнут лимит)"
                        : ""
                ),
            false
        );

        Logger.WriteLine(
            $"Количество отработанных сетью циклов: {Result.TotalCycles}"
                + (
                    Result.TotalCycles >= SimulationSettings.MaxCycles
                        ? " (был достигнут лимит)"
                        : ""
                ),
            false
        );

        Logger.LeftPadding++;
        Logger.WriteLine($"Обработано: {Result.TotalCycles - Result.TotalSkippedCycles}");
        Logger.WriteLine($"Пропущено: {Result.TotalSkippedCycles}");
        Logger.LeftPadding--;

        Logger.WriteLine("");

        Logger.WriteLine($"Количество сенсоров: {Environment.Sensors.Count}");
        Logger.WriteLine($"Количество умерших сенсоров: {Environment.Sensors.Count(s => s.IsDead)}");

        Logger.WriteLine("");

        Logger.WriteLine($"Количество отправленных сообщений: {Result.TotalSends}");
        Logger.WriteLine($"Количество полученных сообщений: {Result.TotalReceives}");

        Logger.WriteLine("");

        Logger.WriteLine($"Количество плохих циклов: {Result.TotalBadCycles}");
        Logger.WriteLine($"Количество коллизий: {Result.TotalCollisions}");
    }
}