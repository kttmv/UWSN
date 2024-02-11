using CommandLine;

namespace UWSN.CommandLine.Options
{
    public class BaseCommandLineOptions
    {
        [Option('f', "file", Required = true, HelpText = "Путь к файлу")]
        public required string FilePath { get; set; }
    }
}