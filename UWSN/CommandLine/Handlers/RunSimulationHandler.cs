using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWSN.CommandLine.Options;
using UWSN.Model;
using UWSN.Model.Clusterization;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.CommandLine.Handlers
{
    public class RunSimulationHandler
    {
        public static void Handle(RunSimulationOptions o)
        {
            SerializationHelper.LoadSimulation(o.FilePath);

            Simulation.Instance.Result!.AllDeltas.Add(Simulation.Instance.Time, new SimulationDelta(Simulation.Instance.Time));

            foreach (var sensor in Simulation.Instance.Environment.Sensors)
            {
                sensor.WakeUp();
            }

            Simulation.Instance.Run();

            SerializationHelper.SaveSimulation(o.FilePath);
        }
    }
}