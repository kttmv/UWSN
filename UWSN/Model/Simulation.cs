namespace UWSN.Model
{
    public class Simulation
    {
        #region Singleton

        private static Simulation? _instance;

        public static Simulation Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new Exception("Экземпляр класса Simulation не был создан.");
                }

                return _instance;
            }
        }

        #endregion Singleton

        #region Properties

        /// <summary>
        /// Окружение симуляции
        /// </summary>
        public Environment Environment { get; set; }

        /// <summary>
        /// Текущее время симуляции
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Отсортированные по каналам эммиты
        /// </summary>
        public Event?[] ChannelSortedEmits { get; set; }

        /// <summary>
        /// Отсортированный по времени список событый
        /// </summary>
        private SortedList<DateTime, Event> EventScheduler { get; set; }

        // TODO: Не использовать дефолтное значение и выставлять его при инициализации (или вместе с протоколами)
        /// <summary>
        /// Количество доступных каналов
        /// </summary>
        public int NumberOfChannels { get; set; } = 1;

        #endregion Properties

        public Simulation()
        {
            if (_instance != null)
            {
                throw new Exception("Экземпляр класса Simulation уже создан.");
            }

            _instance = this;
            Environment = new Environment();
            EventScheduler = new SortedList<DateTime, Event>(new DuplicateKeyComparer<DateTime>());
            ChannelSortedEmits = new Event?[NumberOfChannels];
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