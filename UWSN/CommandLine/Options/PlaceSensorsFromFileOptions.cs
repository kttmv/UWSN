using CommandLine;

namespace UWSN.CommandLine.Options
{
    [Verb("placeSensorsFile", HelpText = "Расположить сенсоры в акватории в виде ортогональной сетки")]
    public class PlaceSensorsFromFileOptions : BaseCommandLineOptions
    {
        [Value(0, Required = true, HelpText = "Путь до файла с координатами сенсоров")]
        public required string SensorsFilePath { get; set; }
    }
}