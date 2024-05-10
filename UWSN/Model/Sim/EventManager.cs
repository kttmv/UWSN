using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWSN.Utilities;

namespace UWSN.Model.Sim;

public class EventManager
{
    private SortedDictionary<DateTime, List<Event>> Events { get; set; }

    public EventManager()
    {
        Events = new();
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

        if (!Events.TryGetValue(e.Time, out List<Event>? eventsList))
        {
            Events.Add(e.Time, new() { e });
        }
        else
        {
            eventsList.Add(e);
        }
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="e">Событие</param>
    public void RemoveEvent(Event e)
    {
        if (!Events.TryGetValue(e.Time, out List<Event>? eventsList))
        {
            throw new KeyNotFoundException("Указанное для удаления событие не было найдено.");
        }

        if (!eventsList.Remove(e))
        {
            throw new KeyNotFoundException("Указанное для удаления событие не было найдено.");
        }

        if (eventsList.Count == 0)
        {
            Events.Remove(e.Time);
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

        var e = Events.First().Value.First();
        RemoveEvent(e);

        return e;
    }
}