namespace UWSN.Model
{
    public class Event
    {
        /// <summary>
        /// Время
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Действие событыя
        /// </summary>
        public Action Action { get; set; }

        public Event(DateTime time, Action action)
        {
            Time = time;
            Action = action;
        }
    }
}