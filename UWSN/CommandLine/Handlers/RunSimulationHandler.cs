using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWSN.CommandLine.Options;
using UWSN.Model;
using UWSN.Model.Sim;
using UWSN.Utilities;

namespace UWSN.CommandLine.Handlers
{
    public class RunSimulationHandler
    {
        public static void Handle(RunSimulationOptions o)
        {
            SerializationHelper.LoadSimulation(o.FilePath);

            foreach (var sensor in Simulation.Instance.Environment.Sensors)
            {
                sensor.WakeUp();
            }

            Simulation.Instance.Run();

            //for (int i = 0; i < 4000; i += 100)
            //{
            //    var prob = DeliveryProbabilityCalculator.Calculate(60.0, 12.8, new Vector3(0, 0, 0), new Vector3(0, 0, i), 25);
            //    Console.Write(Math.Pow(prob, 256.0) + ",");
            //}

        }
    }
}
