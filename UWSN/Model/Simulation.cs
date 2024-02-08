using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model
{
    public class Simulation
    {
        public static Simulation instance;

        public Environment Environment { get; set; }

        public DateTime Time { get; set; }

        /// <summary>
        /// Отсортированный по времени список событый
        /// </summary>
        public SortedList<DateTime, Event> EventScheduler { get; set; }

        public Simulation(Environment env)
        {
            instance = this;
            Environment = env;
            EventScheduler = new SortedList<DateTime, Event>();
        }

        /// <summary>
        /// Добавить событие
        /// </summary>
        /// <param name="e">Событие</param>
        public void AddEvent(Event e)
        {
            EventScheduler.Add(e.Time, e);
        }

        /// <summary>
        /// Метод запуска симуляции
        /// </summary>
        public void Run()
        {
            while (EventScheduler.Count > 0)
            {
                Time = EventScheduler.First().Key;
                EventScheduler.First().Value.Action.Invoke();
                EventScheduler.RemoveAt(0);
            }
        }
    }
}
