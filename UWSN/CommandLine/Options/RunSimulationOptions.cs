using CommandLine;

namespace UWSN.CommandLine.Options
{
    [Verb("runSim", HelpText = "Запустить симуляцию")]
    public class RunSimulationOptions : BaseCommandLineOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Печатать весь вывод симуляции в консоль. Сильно замедляет процесс симуляции.")]
        public bool Verbose { get; set; }

        [Option('o', "output", Required = false, HelpText = "Сохранение вывода симуляции в файл в папке с .exe симулятора. Файлы могут весить гигабайты, а также это сильно замедляет программу.")]
        public bool Output { get; set; }

        [Option('r', "result", Required = false, HelpText = "Сохраняет в результат симуляции абсолютно все. Может привести к очень большому количеству дельт и размеру файла.")]
        public bool FullResult { get; set; }
    }
}