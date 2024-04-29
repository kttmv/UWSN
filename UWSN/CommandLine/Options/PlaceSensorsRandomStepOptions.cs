using CommandLine;

namespace UWSN.CommandLine.Options
{
    [Verb("placeSensorsRndStep", HelpText = "Расположить сенсоры в акватории в виде ортогональной сетки со случайным отклонением")]
    public class PlaceSensorsRandomStepOptions : BaseCommandLineOptions
    {
        [Value(0, Required = true, HelpText = "Вид функции распределения")]
        public DistributionType DistributionType { get; set; }

        [Value(0, Required = true, HelpText = "Количество сенсоров по Х")]
        public int Count_X { get; set; }

        [Value(0, Required = true, HelpText = "Количество сенсоров по У")]
        public int Count_Y { get; set; }

        [Value(0, Required = true, HelpText = "Количество сенсоров по Z")]
        public int Count_Z { get; set; }

        [Value(0, Required = true, HelpText = "Параметр А равномерного распределения")]
        public double UniParameterA { get; set; }

        [Value(0, Required = true, HelpText = "Параметр B равномерного распределения")]
        public double UniParameterB { get; set; }

        [Value(0, Required = true, HelpText = "Количество сенсоров")]
        public int SensorsCount { get; set; }
    }

    [Flags]
    public enum DistributionType
    {
        Normal,
        Uniform
    }
}