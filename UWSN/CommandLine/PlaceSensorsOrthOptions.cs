using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.CommandLine
{
    [Verb("placeSensorsOrth", HelpText = "Расположить сенсоры в акватории в виде ортогональной сетки")]
    public class PlaceSensorsOrthOptions : BaseCommandLineOptions
    {
        [Value(0, Required = true, HelpText = "Шаг ортогональной сетки")]
        public float OrthogonalStep { get; set; }

        [Value(0, Required = true, HelpText = "Количество сенсоров")]
        public int SensorsCount { get; set; }
    }
}
