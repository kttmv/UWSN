using UWSN.CommandLine.Options;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.CommandLine.Handlers
{
    public class RunSimulationHandler
    {
        public static void Handle(RunSimulationOptions o)
        {
            SerializationHelper.LoadSimulation(o.FilePath);

            Logger.ShouldWriteToConsole = o.Verbose;

            Simulation.Instance.Run(o.Verbose);

            SerializationHelper.SaveSimulation(o.FilePath);
            Logger.Save();
        }
    }
}
