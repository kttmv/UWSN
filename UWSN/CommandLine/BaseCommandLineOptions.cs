using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.CommandLine
{
    public class BaseCommandLineOptions
    {
        [Option('f', "file", Required = true, HelpText = "Путь к файлу")]
        public string FilePath { get; set; }
    }
}
