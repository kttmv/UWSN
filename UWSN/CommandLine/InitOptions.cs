using CommandLine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.CommandLine
{
    [Verb("init", HelpText = "Создать экземпляр среды")]
    public class InitOptions : BaseCommandLineOptions
    {
        [Value(0, Min = 6, Max = 6, Required = true, HelpText = "Пределы акватории")]
        public IEnumerable<int> AreaLimits { get; set; }
    }
}
