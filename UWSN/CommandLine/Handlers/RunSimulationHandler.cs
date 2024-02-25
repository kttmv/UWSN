using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWSN.CommandLine.Options;
using UWSN.Model;

namespace UWSN.CommandLine.Handlers
{
    public class RunSimulationHandler
    {
        public static void Handle(RunSimulationOptions o)
        {
            var env = new Loader(o.FilePath).LoadEnv();

            var sim = new Simulation(env);
            var frame = new Frame
            {
                IdSend = env.Sensors.First().Id,
                IdReceive = env.Sensors.Last().Id
            };
            Signal.Emit(env.Sensors.First(), frame);
            sim.Run();
        }
    }
}
