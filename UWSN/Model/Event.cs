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
        private readonly Action _action;

        public Event(DateTime time, Action action)
        {
            Time = time;
            _action = action;
        }

        public void Invoke()
        {
            _action.Invoke();
        }
    }
}