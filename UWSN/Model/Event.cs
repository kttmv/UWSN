namespace UWSN.Model
{
    public class Event
    {
        /// <summary>
        /// Время
        /// </summary>
        public DateTime Time { get; }

        /// <summary>
        /// Действие, вызываемое в момент исполнения события
        /// </summary>
        private Action Action { get; }

        public Event(DateTime time, Action action)
        {
            Time = time;
            Action = action;
        }

        public void Invoke()
        {
            Action.Invoke();
        }
    }
}