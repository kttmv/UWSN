﻿using System.Drawing;
using System.Numerics;
using UWSN.Utilities;

namespace UWSN.Model.Sim
{
    public class Simulation
    {
        private const int MAX_PROCESSED_EVENTS = 200;

        #region Simulation Singleton

        private static Simulation? SimulationInstance { get; set; }

        public static Simulation Instance
        {
            get
            {
                if (SimulationInstance == null)
                {
                    throw new Exception("Экземпляр класса Simulation не был создан.");
                }

                return SimulationInstance;
            }
        }

        #endregion Simulation Singleton

        #region Properties

        public ChannelManager ChannelManager { get; set; }

        /// <summary>
        /// Окружение симуляции
        /// </summary>
        public Environment Environment { get; set; }

        public EventManager EventManager { get; set; }

        /// <summary>
        /// Текущее время симуляции
        /// </summary>
        public DateTime Time { get; set; }

        public Vector3Range AreaLimits { get; set; }

        #endregion Properties

        public Simulation()
        {
            if (SimulationInstance != null)
                throw new Exception("Экземпляр класса Simulation уже создан.");

            SimulationInstance = this;

            ChannelManager = new ChannelManager();
            EventManager = new EventManager();

            Environment = new Environment();

            AreaLimits = new Vector3Range(new Vector3(), new Vector3());
        }

        /// <summary>
        /// Метод запуска симуляции
        /// </summary>
        public void Run()
        {
            int i = 1;
            while (i < MAX_PROCESSED_EVENTS)
            {
                var e = EventManager.RemoveFirst();

                if (e == null)
                {
                    Logger.WriteLine("Больше событий нет. Симуляция окончена.");
                    break;
                }

                Time = e.Time;

                Logger.WriteLine($"Событие №{i}. {e.Description}", true);

                e.Invoke();

                Logger.WriteLine("");

                i++;
            }

            if (i >= MAX_PROCESSED_EVENTS)
            {
                Logger.WriteLine("Достигнут лимит событий. Симуляция остановлена");
            }
        }
    }
}
