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

            foreach (var sensor in Simulation.Instance.Environment.Sensors)
            {
                sensor.WakeUp();
            }

            try
            {
                Simulation.Instance.Run();
            }
            catch (Exception e)
            {
                Logger.Save();
                throw e;
            }

            SerializationHelper.SaveSimulation(o.FilePath);
            Logger.Save();
        }
    }
}
