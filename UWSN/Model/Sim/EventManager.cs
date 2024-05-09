using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWSN.Utilities;

namespace UWSN.Model.Sim;

public class EventManager
{
    private List<Event> Events { get; set; }

    public EventManager()
    {
        Events = new List<Event>();
        //EventScheduler = new SortedList<DateTime, Event>(new DuplicateKeyComparer<DateTime>());
    }

    /// <summary>
    /// Добавить событие
    /// </summary>
    /// <param name="e">Событие</param>
    public void AddEvent(Event e)
    {
        if (e.Time < Simulation.Instance.Time)
        {
            throw new ArgumentException(
                "Была произведена попытка создания события в прошлом."
                    + $"Текущее время симуляции: {Simulation.Instance.Time:dd.MM.yyyy HH:mm:ss.fff}."
                    + $"Время добавляемого события: {e.Time:dd.MM.yyyy HH:mm:ss.fff}"
            );
        }

        Events.Add(e);
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="e">Событие</param>
    public void RemoveEvent(Event e)
    {
        if (!Events.Remove(e))
        {
            throw new KeyNotFoundException("Указанное для удаления событие не было найдено.");
        }
    }

    /// <summary>
    /// Получить ближайшее по времени событие и удалить его из планировщика
    /// </summary>
    /// <returns>Ближайшее по времени событие</returns>
    public Event? PopFirst()
    {
        if (Events.Count == 0)
        {
            return null;
        }

        var e = Events.OrderBy(x => x.Time).First();
        Events.Remove(e);

        return e;
    }
}