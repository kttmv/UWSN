using CommandLine;
using UWSN.CommandLine.Handlers;
using UWSN.CommandLine.Options;

namespace UWSN;

public class Program
{
    private static readonly Dictionary<Type, Action<object>> handlers =
        new()
        {
            {
                typeof(InitializationOptions),
                options => InitializationHandler.Handle((InitializationOptions)options)
            },
            {
                typeof(PlaceSensorsOrthOptions),
                options => PlaceSensorsOrthHandler.Handle((PlaceSensorsOrthOptions)options)
            },
            {
                typeof(PlaceSensorsRandomStepOptions),
                options =>
                    PlaceSensorsRandomStepHandler.Handle((PlaceSensorsRandomStepOptions)options)
            },
            {
                typeof(PlaceSensorsBySpecsOptions),
                options => PlaceSensorsBySpecsHandler.Handle((PlaceSensorsBySpecsOptions)options)
            },
            {
                typeof(PlaceSensorsPoissonOptions),
                options => PlaceSensorsPoissonHandler.Handle((PlaceSensorsPoissonOptions)options)
            },
            {
                typeof(PlaceSensorsFromFileOptions),
                options => PlaceSensorsFromFileHandler.Handle((PlaceSensorsFromFileOptions)options)
            },
            {
                typeof(RunSimulationOptions),
                options => RunSimulationHandler.Handle((RunSimulationOptions)options)
            }
        };

    private static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Parser
            .Default.ParseArguments(args, handlers.Keys.ToArray())
            .WithParsed(options => handlers[options.GetType()].Invoke(options));
    }
}
