namespace UWSN.Model.Sim
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

        /// <summary>
        /// Описание события
        /// </summary>
        public string Description { get; }

        public Event(DateTime time, string description, Action action)
        {
            Time = time;
            Action = action;
            Description = description;
        }

        public void Invoke()
        {
            Action.Invoke();
        }
    }
}