﻿using UWSN.CommandLine.Options;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.CommandLine.Handlers
{
    public class RunSimulationHandler
    {
        public static void Handle(RunSimulationOptions o)
        {
            SerializationHelper.LoadSimulation(o.FilePath);

            foreach (var sensor in Simulation.Instance.Environment.Sensors)
            {
                sensor.WakeUp();
            }

            Simulation.Instance.Run();

            SerializationHelper.SaveSimulation(o.FilePath);
            Logger.Save();
        }
    }
}
