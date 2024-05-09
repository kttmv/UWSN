using CommandLine;

namespace UWSN.CommandLine.Options
{
    [Verb("runSim", HelpText = "Запустить симуляцию")]
    public class RunSimulationOptions : BaseCommandLineOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Печатать весь вывод консоли")]
        public bool Verbose { get; set; }
    }
}
