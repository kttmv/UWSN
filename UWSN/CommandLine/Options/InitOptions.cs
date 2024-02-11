using CommandLine;

namespace UWSN.CommandLine.Options
{
    [Verb("init", HelpText = "Создать экземпляр среды")]
    public class InitOptions : BaseCommandLineOptions
    {
        [Value(0, Min = 6, Max = 6, Required = true, HelpText = "Пределы акватории")]
        public required IEnumerable<int> AreaLimits { get; set; }
    }
}