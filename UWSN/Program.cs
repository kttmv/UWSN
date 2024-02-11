using System;
using System.Collections.Generic;
using CommandLine;
using UWSN.CommandLine.Handlers;
using UWSN.CommandLine.Options;
using UWSN.Model;

namespace UWSN;

public class Program
{
    private static readonly Dictionary<Type, Action<object>> handlers = new Dictionary<Type, Action<object>>
    {
        { typeof(InitOptions), options => InitHandler.Handle((InitOptions)options) },
        { typeof(PlaceSensorsOrthOptions), options => PlaceSensorsOrthHandler.Handle((PlaceSensorsOrthOptions)options) },
        { typeof(PlaceSensorsRandomStepOptions), options => PlaceSensorsRandomStepHandler.Handle((PlaceSensorsRandomStepOptions)options) },
        { typeof(PlaceSensorsPoissonOptions), options => PlaceSensorsPoissonHandler.Handle((PlaceSensorsPoissonOptions)options) },
        { typeof(PlaceSensorsFromFileOptions), options => PlaceSensorsFromFileHandler.Handle((PlaceSensorsFromFileOptions)options) }
    };

    private static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Parser.Default.ParseArguments(args, handlers.Keys.ToArray())
            .WithParsed(options => handlers[options.GetType()].Invoke(options));
    }
}