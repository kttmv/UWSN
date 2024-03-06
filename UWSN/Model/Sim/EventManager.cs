using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWSN.Utilities;

namespace UWSN.Model.Sim;

public class EventManager
{
    /// <summary>
    /// Отсортированный по времени список событый
    /// </summary>
    private SortedList<DateTime, Event> EventScheduler { get; set; }

    public EventManager()
    {
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

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="e">Событие</param>
    public void RemoveEvent(Event e)
    {
        EventScheduler.Remove(e.Time);
    }

    /// <summary>
    /// Получить ближайшее по времени событие и удалить его из планировщика
    /// </summary>
    /// <returns>Ближайшее по времени событие</returns>
    public Event? RemoveFirst()
    {
        if (EventScheduler.Count == 0)
        {
            return null;
        }

        var e = EventScheduler.FirstOrDefault();
        EventScheduler.RemoveAt(0);

        return e.Value;
    }
}