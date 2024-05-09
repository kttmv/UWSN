namespace UWSN.Model.Sim
{
    public class Event
    {
        private static int LastEventId { get; set; } = 0;

        public int Id { get; }

        /// <summary>
        /// Время
        /// </summary>
        public DateTime Time { get; }

        /// <summary>
        /// Действие, вызываемое в момент исполнения события
        /// </summary>
        private Action Action { get; set; }

        /// <summary>
        /// Описание события
        /// </summary>
        public string Description { get; }

        public Event(DateTime time, string description, Action action)
        {
            Id = LastEventId + 1;
            LastEventId += 1;

            Time = time;
            Description = description;
            Action = action;
        }

        /// <summary>
        /// Метод переназначения действия события. Очень опасно, использовать
        /// с осторожностью.
        /// </summary>
        public void SetAction(Action action)
        {
            Action = action;
        }

        public void Invoke()
        {
            Action.Invoke();
        }
    }
}