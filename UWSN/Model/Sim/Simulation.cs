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
    private int BadCyclesInRow { get; set; }

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
    public void Run()
    {
        var timeStart = DateTime.Now;

        Result = new SimulationResult();

        WakeSensorsUp();

        while (
            EventNumber < SimulationSettings.MaxProcessedEvents
            || SimulationSettings.MaxProcessedEvents <= 0
        )
        {
            if (CurrentCycle == 0 && EventNumber % SimulationSettings.PrintEveryNthEvent == 0)
                PrintCurrentState();

            var eventToInvoke = EventManager.PopFirst();
            if (eventToInvoke == null)
            {
                PrintCurrentState();

                if (CurrentCycle == 0)
                    CheckHelloResult();

                // так как в данном режиме не сохраняются изменения зарядов сенсоров,
                // сохраняем их вручную после каждого цикла.
                if (!SimulationSettings.CreateAllDeltas)
                    CreateBatteryDeltas();

                Logger.WriteLine("");
                Logger.WriteLine("Цикл завершен");

                CheckCycleForErrors();
                AnalyzeCurrentCycle();
                TryToSkipCycles();

                // TODO: вынести константу в свойство
                if (BadCyclesInRow >= 10)
                    break;

                if (
                    CurrentCycle >= SimulationSettings.MaxCycles
                    && SimulationSettings.MaxCycles > 0
                )
                    break;

                StartNewCycle();

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

        if (!SimulationSettings.CreateAllDeltas)
            CreateBatteryDeltas();

        Result.TotalEvents = EventNumber;
        Result.TotalCycles = CurrentCycle;

        Result.SimulationEndTime = Time;

        var timeFinish = DateTime.Now;
        Result.RealTimeToSimulate = timeFinish - timeStart;

        PrintResults();
    }

    private void CheckHelloResult()
    {
        if (
            Environment.Sensors.Any(sensor =>
                sensor.ClusterId == null || sensor.IsReference == null
            )
        )
            throw new Exception(
                "Во время процесса HELLO не была вычислена кластеризация для всех сенсоров."
            );

        foreach (var sensor in Environment.Sensors)
        {
            if (sensor.Neighbours.Count != Environment.Sensors.Count)
                throw new Exception(
                    "Процесс HELLO был завершен ошибочно. Не все сенсоры обладают полной картой сети"
                );
        }

        Logger.WriteLine("");
        Logger.WriteLine("Процесс HELLO был завершен успешно");
    }

    private void StartNewCycle()
    {
        Logger.WriteLine("");
        Logger.WriteLine($"=============================");
        Logger.WriteLine("Начало нового цикла");
        CurrentCycle++;

        var clusterIds = Environment
            .Sensors.GroupBy(s => s.ClusterId!.Value)
            .Select(g => g.Key)
            .ToList();

        foreach (int cluster in clusterIds.Where(c => c != -1))
        {
            var result = Result!.GetOrCreateClusterCycleResult(CurrentCycle, cluster);

            result.ReferenceSensorId = Environment
                .Sensors.Where(s => s.ClusterId == cluster && s.IsReference!.Value)
                .First()
                .Id;
            result.SensorsCount = Environment.Sensors.Count(s => s.ClusterId == cluster);
        }

        CollectSensorsData();
    }

    private void CreateBatteryDeltas()
    {
        foreach (var sensor in Environment.Sensors)
        {
            AddSensorBatteryDelta(sensor);
        }
    }

    private void AnalyzeCurrentCycle()
    {
        if (CurrentCycle == 0)
            return;

        foreach (var sensor in Environment.Sensors.Where(s => s.IsReference.GetValueOrDefault()))
        {
            if (sensor.ClusterId == -1)
                throw new Exception("Сенсор является референсным для мертвого кластера.");

            int clusterId = sensor.ClusterId!.Value;

            var currentCycleDataGroup = sensor
                .ReceivedData.GroupBy(d => d.CycleId)
                .Where(group => group.Key == CurrentCycle)
                .FirstOrDefault();

            if (currentCycleDataGroup == null)
                break;

            var currentCycleData = currentCycleDataGroup.ToList();

            var clusterResult = Result!.GetOrCreateClusterCycleResult(CurrentCycle, clusterId);

            clusterResult.CollectedDataCount = currentCycleData.Count;

            var sensorInClusterIds = Environment
                .Sensors.Where(s => s.ClusterId == sensor.ClusterId)
                .Select(s => s.Id)
                .ToList();

            foreach (var data in currentCycleData)
            {
                if (sensorInClusterIds.Contains(data.SensorId))
                    clusterResult.CollectedDataSameClusterCount++;
                else
                    clusterResult.CollectedDataOtherClusterCount++;
            }
        }

        int clustersCount = Environment
            .Sensors.GroupBy(s => s.ClusterId)
            .Where(g => g.Key != -1)
            .Count();

        var cycleResult = Result!.GetOrCreateCycleResult(CurrentCycle);

        int missingClustersCount = cycleResult.ClusterResults.Count - clustersCount;
        int badClustersCount = missingClustersCount;

        foreach (var clusterResult in cycleResult.ClusterResults.Values)
        {
            if (clusterResult.CollectedDataSameClusterCount < clusterResult.SensorsCount)
            {
                badClustersCount++;
            }
        }

        Logger.WriteLine("");
        Logger.WriteLine($"Результаты цикла #{CurrentCycle:n0}:");
        Logger.WriteLine("");

        Logger.WriteLine($"Количество кластеров: {clustersCount:n0}");
        Logger.WriteLine(
            $"Количество сенсоров, учавствовавших в работе сети: {cycleResult.SensorsCount:n0}"
        );
        Logger.WriteLine(
            $"Количество кластеров, собравших неполный набор данных с датчиков: {badClustersCount:n0}"
        );

        Logger.WriteLine(
            "Количество собранных референсными сенсорами фреймов данных с датчиков других сенсоров:"
        );

        Logger.LeftPadding++;
        Logger.WriteLine($"Всего: {cycleResult.CollectedDataCount:n0}");
        Logger.WriteLine($"Со своего кластера: {cycleResult.CollectedDataSameClusterCount:n0}");
        Logger.WriteLine($"С чужих кластеров: {cycleResult.CollectedDataOtherClusterCount:n0}");

        double percentagesFromPerfect =
            (double)cycleResult.CollectedDataCount / cycleResult.SensorsCount * 100;

        Logger.WriteLine($"Процентов от идеального значения: {percentagesFromPerfect:n2}%");
        Logger.LeftPadding--;

        bool cycleIsBad = badClustersCount > 0;
        if (cycleIsBad)
        {
            BadCyclesInRow++;
            Result.TotalBadCycles++;
            Logger.WriteLine("");
            Logger.WriteLine("Цикл является плохим.");
            Logger.WriteLine($"Количество плохих циклов подряд: {BadCyclesInRow:n0}");
        }
        else
        {
            BadCyclesInRow = 0;
        }
    }

    private void CheckCycleForErrors()
    {
        if (
            Environment.Sensors.Any(s =>
                s.CurrentState != Sensor.State.Listening && s.CurrentState != Sensor.State.Idle
            )
        )
            throw new Exception("Не все сенсоры перешли в состояние прослушивания или ожидания.");

        if (
            Environment.Sensors.Any(sensor =>
                !sensor.IsDead
                && sensor.DeadSensors.Count < Environment.Sensors.Count(s => s.IsDead)
            )
        )
            throw new Exception("Не все сенсоры получили информацию о смерти других сенсоров.");
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

        if (!SimulationSettings.ShouldSkipHello)
        {
            Logger.WriteLine("");
            Logger.WriteLine("Запущен процесс HELLO");
            Logger.WriteLine("");
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

    private bool TryToSkipCycles()
    {
        if (!SimulationSettings.ShouldSkipCycles)
            return false;

        if (CurrentCycle == 0)
        {
            foreach (var sensor in Environment.Sensors)
            {
                BatteriesPreCycle.Add(sensor, sensor.Battery);
            }

            TimeBeforeCycle = Time;
            NextSkipCycle = SimulationSettings.CyclesCountBeforeSkip;

            return false;
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

            return false;
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
                    averageBatteryChanges.First(b => b.Key.Id == id).Value + batteryChange.Value;

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

            double averageCost = averageCycle.BatteryChange.First(b => b.Key.Id == sensor.Id).Value;

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

                if (sensor.IsDead)
                    throw new Exception("Процесс пропуска циклов произошел с ошибкой.");
            }

            Time += averageCycle.CycleTime * cyclesToSkip;

            CurrentCycle += cyclesToSkip;
            Result!.TotalSkippedCycles += cyclesToSkip;

            CreateBatteryDeltas();

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

            Logger.WriteLine("");
            Logger.WriteLine($"=============================");
            Logger.WriteLine($"Было пропущено {cyclesToSkip:n0} циклов.");
            Logger.WriteLine($"=============================");
            Logger.WriteLine("");
            return true;
        }

        return false;
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
        if (SimulationSettings.Verbose)
        {
            Logger.WriteLine("");
            Logger.WriteLine($"Событие №{eventNumber:n0}. {eventToInvoke.Description}", true);
        }
    }

    private void PrintCurrentState()
    {
        Logger.WriteLine("");
        Logger.WriteLine($"Текущий цикл сбора данных: {CurrentCycle:n0}");
        Logger.WriteLine($"Текущее время симуляции: {Time:dd.MM.yyyy HH:mm:ss.fff}");
        Logger.WriteLine($"Обработано событий: {EventNumber:n0}.");

        int aliveCount = Environment.Sensors.Where(s => !s.IsDead).Count();
        double alivePercentage = (double)aliveCount / Environment.Sensors.Count * 100;
        int deadCount = Environment.Sensors.Where(s => s.IsDead).Count();
        double deadPercentage = (double)deadCount / Environment.Sensors.Count * 100;

        Logger.WriteLine($"Количество живых сенсоров: {aliveCount} ({alivePercentage:n2}%)", false);
        Logger.WriteLine($"Количество мертвых сенсоров: {deadCount} ({deadPercentage:n2}%)", false);

        double totalInitialEnergy = Environment.Sensors.Count * SensorSettings.InitialSensorBattery;
        double totalEnergy = 0;
        foreach (var sensor in Environment.Sensors)
        {
            totalEnergy += sensor.Battery;
        }

        double totalEnergyPercentage = (double)totalEnergy / totalInitialEnergy * 100;

        Logger.WriteLine(
            "Остаточное количество энергии в сенсорах: "
                + $"{totalEnergy:n2} Дж ({totalEnergyPercentage:n2}%)",
            false
        );
    }

    public void PrintResults()
    {
        PrintCurrentState();

        Logger.WriteLine("");
        Logger.WriteLine($"=============================");
        Logger.WriteLine("");
        Logger.WriteLine("Симуляция остановлена.");

        if (
            EventNumber >= SimulationSettings.MaxProcessedEvents
            && SimulationSettings.MaxProcessedEvents > 0
        )
        {
            Logger.WriteLine("");
            Logger.WriteLine("Был достигнут лимит событий.");
        }

        if (CurrentCycle >= SimulationSettings.MaxCycles && SimulationSettings.MaxCycles > 0)
        {
            Logger.WriteLine("");
            Logger.WriteLine("Достигнуто максимальное количество циклов.");
        }

        if (CheckNetworkIsDead())
        {
            Logger.WriteLine("");
            Logger.WriteLine("Сеть мертва");
        }

        // TODO: вынести константу в свойство
        if (BadCyclesInRow >= 10)
        {
            Logger.WriteLine("");
            Logger.WriteLine($"Было обнаружено {BadCyclesInRow:n0} плохих циклов подряд.");
        }

        Logger.WriteLine("");
        Logger.WriteLine($"=============================");
        Logger.WriteLine("Результаты симуляции:");
        Logger.WriteLine("");

        Logger.LeftPadding++;

        Logger.WriteLine(
            $"Реальное время, потраченное на симуляцию: {Result!.RealTimeToSimulate:hh\\:mm\\:ss}",
            false
        );

        Logger.WriteLine(
            $"Конечное время симуляции: {Result.SimulationEndTime:dd.MM.yyyy HH:mm:ss.fff}"
        );

        Logger.WriteLine("");

        Logger.WriteLine(
            $"Количество обработанных событий: {Result.TotalEvents:n0}"
                + (
                    Result.TotalEvents >= SimulationSettings.MaxProcessedEvents
                    && SimulationSettings.MaxProcessedEvents > 0
                        ? " (был достигнут лимит)"
                        : ""
                ),
            false
        );

        Logger.WriteLine(
            $"Количество отработанных сетью циклов: {Result.TotalCycles:n0}"
                + (
                    Result.TotalCycles >= SimulationSettings.MaxCycles
                    && SimulationSettings.MaxCycles > 0
                        ? " (был достигнут лимит)"
                        : ""
                ),
            false
        );

        Logger.LeftPadding++;
        Logger.WriteLine($"Обработано: {(Result.TotalCycles - Result.TotalSkippedCycles):n0}");
        Logger.WriteLine($"Пропущено: {Result.TotalSkippedCycles:n0}");
        Logger.LeftPadding--;

        Logger.WriteLine("");

        Logger.WriteLine($"Количество сенсоров: {Environment.Sensors.Count:n0}");
        Logger.WriteLine(
            $"Количество умерших сенсоров: {Environment.Sensors.Count(s => s.IsDead):n0}"
        );

        Logger.WriteLine("");

        Logger.WriteLine($"Количество отправленных сообщений: {Result.TotalSends:n0}");
        Logger.WriteLine($"Количество полученных сообщений: {Result.TotalReceives:n0}");

        Logger.WriteLine("");

        Logger.WriteLine($"Количество плохих циклов: {Result.TotalBadCycles:n0}");
        Logger.WriteLine($"Количество коллизий: {Result.TotalCollisions:n0}");
    }
}