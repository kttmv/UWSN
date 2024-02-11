using CommandLine;

namespace UWSN.CommandLine.Options
{
    [Verb("placeSensorsPoisson", HelpText = "Расположить сенсоры в акватории по закону Пуассона")]
    public class PlaceSensorsPoissonOptions : BaseCommandLineOptions
    {
        [Value(0, Required = true, HelpText = "Параметр лямбда распределения Пуссона")]
        public double LambdaParameter { get; set; }

        [Value(0, Required = true, HelpText = "Количество сенсоров")]
        public int SensorsCount { get; set; }
    }
}