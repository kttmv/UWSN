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

            Simulation.Instance.Run();

            SerializationHelper.SaveSimulation(o.FilePath);

            if (Simulation.Instance.SimulationSettings.SaveOutput)
            {
                Logger.WriteLine("");
                Logger.WriteLine($"Полный вывод симуляции сохранен в файл {Logger.FilePath}");
            }
        }
    }
}
