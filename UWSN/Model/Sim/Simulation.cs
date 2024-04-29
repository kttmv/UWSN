using System.Numerics;
using Newtonsoft.Json;
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

        public ModemBase Modem { get; set; }

        public TimeSpan SensorSampleInterval { get; set; }

        public Type DataLinkProtocolType { get; set; }

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

            ChannelManager = new ChannelManager();
            EventManager = new EventManager();

            Environment = new Environment();

            AreaLimits = new Vector3Range(new Vector3(), new Vector3());

            DataLinkProtocolType = typeof(PureAlohaProtocol);

            SensorSampleInterval = new TimeSpan(0, 30, 0);

            Modem = new AquaModem1000();
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
                    Logger.WriteLine("Больше событий нет. Симуляция окончена.");
                    break;
                }

                Time = e.Time;

                Logger.WriteLine($"Событие №{i}. {e.Description}", true);

                e.Invoke();

                Logger.WriteLine("");

                i++;
            }

            if (i >= MAX_PROCESSED_EVENTS)
            {
                Logger.WriteLine("Достигнут лимит событий. Симуляция остановлена");
            }

            //foreach (var sensor in Environment.Sensors)
            //{
            //    Logger.WriteLine($"Sensor: {sensor.Id}");
            //    foreach (var (Id, _) in sensor.Network.Neighbours)
            //    {
            //        Logger.WriteLine($"Neighbour: {Id}");
            //    }

            //    break;
            //}

            Logger.WriteLine("");
            Logger.WriteLine("Результаты симуляции:");
            Logger.WriteLine($"\tКоличество отправленных сообщений: {Result.TotalSends}");
            Logger.WriteLine($"\tКоличество полученных сообщений: {Result.TotalReceives}");
            Logger.WriteLine($"\tКоличество коллизий: {Result.TotalCollisions}");
        }
    }
}