using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.CommandLine
{
    [Verb("placeSensorsFile", HelpText = "Расположить сенсоры в акватории в виде ортогональной сетки")]
    public class PlaceSensorsFromFileOptions : BaseCommandLineOptions
    {
        [Value(0, Required = true, HelpText = "Путь до файла с координатами сенсоров")]
        public string SensorsFilePath { get; set; }
    }
}
