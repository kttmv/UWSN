namespace UWSN.Model
{
    public class Simulation
    {
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

        public Environment Environment { get; set; }

        public DateTime Time { get; set; }

        /// <summary>
        /// Отсортированные по каналам эммиты
        /// </summary>
        public Event?[] ChannelSortedEmits { get; set; }

        /// <summary>
        /// Отсортированный по времени список событый
        /// </summary>
        private SortedList<DateTime, Event> EventScheduler { get; set; }

        public Simulation(Environment env)
        {
            if (_instance != null)
            {
                throw new Exception("Экземпляр класса Simulation уже создан.");
            }

            _instance = this;
            Environment = env;
            EventScheduler = new SortedList<DateTime, Event>();
            ChannelSortedEmits = new Event?[4];
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

                Time = e.Key;

                e.Value.Invoke();
                EventScheduler.RemoveAt(0);
            }
        }
    }
}