using CommandLine;
namespace UWSN.CommandLine.Options
{
    [Verb("placeSensorsBySpecs", HelpText = "Расположить сенсоры в акватории в виде ортогональной сетки со случайным отклонением с шагом, вычесленным из их тех. характеристик")]
    public class PlaceSensorsBySpecsOptions : BaseCommandLineOptions
    {
        [Value(0, Required = true, HelpText = "Модель модема")]
        public string ModemModel { get; set; }

        [Value(0, Required = true, HelpText = "Коэффициент максимального допустимого расстояния между модемами, " +
                                              "чтобы не растягивать их на максимум их возможностей (от 0 до 1)")]
        public double DistanceCoeff { get; set; }
    }
}
