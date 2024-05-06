using System.Numerics;
using Newtonsoft.Json;
using UWSN.Model.Clusterization;
using UWSN.Model.Modems;
using UWSN.Model.Protocols.DataLink;
using UWSN.Utilities;

namespace UWSN.Model.Sim
{
    public class Simulation
    {
        private const int MAX_PROCESSED_EVENTS = 1_000_000;

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

        public ModemBase? Modem { get; set; }

        [JsonIgnore]
        public List<ModemBase>? AvailableModems { get; set; }

        /// <summary>
        /// % мёртвых сенсоров, при достижении которого мы считаем сеть мертвой
        /// </summary>
        public double DeadSensorsPercent { get; set; }

        [JsonIgnore]
        public int CurrentCycle { get; set; }

        [JsonIgnore]
        public bool ShouldSkipHello { get; set; }

        public double InitialSensorBattery { get; set; } = 100.0;

        public TimeSpan SensorSampleInterval { get; set; }

        public DateTime StartSamplingTime { get; set; } = new DateTime().AddDays(1);

        public Type DataLinkProtocolType { get; set; }

        public Type ClusterizationAlgorithmType { get; set; }

        [JsonIgnore]
        public IClusterization ClusterizationAlgorithm { get; set; }

        public Vector3Range AreaLimits { get; set; }

        public ChannelManager ChannelManager { get; set; }

        /// <summary>
        /// Окружение симуляции
        /// </summary>
        public Environment Environment { get; set; }

        public SimulationResult? Result { get; set; }

        [JsonIgnore]
        public EventManager EventManager { get; set; }

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

            //todo: если не задавать количество каналов в конструкторе, то оно всегда 0,
            //как будто не считывается с Json
            ChannelManager = new ChannelManager(16);
            EventManager = new EventManager();

            Environment = new Environment();

            ShouldSkipHello = false;

            AreaLimits = new Vector3Range(new Vector3(), new Vector3());

            DataLinkProtocolType = typeof(MultiChanneledAloha);
            ClusterizationAlgorithmType = typeof(RetardedClusterization);

            ClusterizationAlgorithm = (IClusterization)(
                Activator.CreateInstance(Instance.ClusterizationAlgorithmType)
                ?? throw new NullReferenceException("Тип алгоритма кластеризации не определен")
            );

            SensorSampleInterval = new TimeSpan(0, 30, 0);

            Modem ??= new AquaModem1000();
        }

        /// <summary>
        /// Метод запуска симуляции
        /// </summary>
        public void Run()
        {
            Result = new SimulationResult();

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

                if (Environment.Sensors.Where(s => s.IsDead).Count() / (double)Environment.Sensors.Count >= DeadSensorsPercent)
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
            ClusterizationAlgorithm.Clusterize(Environment.Sensors, AreaLimits, 4);
        }
    }
}
