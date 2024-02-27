using System.Drawing;

namespace UWSN.Model
{
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

        public ChannelManager ChannelManager { get; set; }

        /// <summary>
        /// Окружение симуляции
        /// </summary>
        public Environment Environment { get; set; }

        /// <summary>
        /// Текущее время симуляции
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Отсортированный по времени список событый
        /// </summary>
        private SortedList<DateTime, Event> EventScheduler { get; set; }

        #endregion Properties

        public Simulation()
        {
            if (SimulationInstance != null)
            {
                throw new Exception("Экземпляр класса Simulation уже создан.");
            }

            SimulationInstance = this;

            ChannelManager = new ChannelManager();

            Environment = new Environment();
            EventScheduler = new SortedList<DateTime, Event>(new DuplicateKeyComparer<DateTime>());
        }

        /// <summary>
        /// Добавить событие
        /// </summary>
        /// <param name="e">Событие</param>
        public void AddEvent(Event e)
        {
            EventScheduler.Add(e.Time, e);
        }

        public void RemoveEvent(Event e)
        {
            // todo СЛОМАЕЦА ЕСЛИ ВРЕМЯ НЕ РАЗНЫЕ
            EventScheduler.Remove(e.Time);
        }

        /// <summary>
        /// Метод запуска симуляции
        /// </summary>
        public void Run()
        {
            while (EventScheduler.Count > 0)
            {
                var e = EventScheduler.First();
                EventScheduler.RemoveAt(0);

                Time = e.Key;

                e.Value.Invoke();
            }
        }
    }
}