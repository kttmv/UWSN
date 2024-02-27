using System;
using System.Collections.Generic;
using System.Linq;
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

            var env = Simulation.Instance.Environment;

            var frame1 = new Frame
            {
                IdSend = env.Sensors[0].Id,
                IdReceive = env.Sensors[1].Id
            };

            //var frame2 = new Frame
            //{
            //    IdSend = env.Sensors[2].Id,
            //    IdReceive = env.Sensors[1].Id
            //};

            //var frame3 = new Frame
            //{
            //    IdSend = env.Sensors[3].Id,
            //    IdReceive = env.Sensors[4].Id
            //};

            env.Sensors[0].NetworkLayer.SendFrame(frame1);
            //env.Sensors[1].NetworkLayer.SendFrame(frame2);
            //env.Sensors[3].NetworkLayer.SendFrame(frame3);
            Simulation.Instance.Run();
        }
    }
}