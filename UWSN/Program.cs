using CommandLine;
using Dew.Math;
using System.Numerics;
using UWSN.CommandLine;
using UWSN.Model;

namespace UWSN;

public class Program
{
    public static Model.Environment Environment { get; set; }

    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<InitOptions, PlaceSensorsOrthOptions, PlaceSensorsRandomStepOptions, PlaceSensorsPoissonOptions>(args)
            .WithParsed<InitOptions>(o =>
            {
                var areaLimits = o.AreaLimits.ToList();
                Environment = new Model.Environment
                {

                    AreaLimits = new Tuple<Vector3, Vector3>
                    (new Vector3(areaLimits[0], areaLimits[1], areaLimits[2]),
                     new Vector3(areaLimits[3], areaLimits[4], areaLimits[5])),

                    Sensors = new List<Model.Sensor>(),
                    PlacementType = null
                };

                Environment.SaveEnv(o.FilePath);

            }).WithParsed<PlaceSensorsOrthOptions>(o =>
            {
                var loader = new Loader(o.FilePath);
                Environment = loader.LoadEnv();

                Environment.Sensors.Clear();
                for (int i = 0; i < o.SensorsCount; i++)
                {
                    Environment.Sensors.Add(new Sensor(i));
                }

                Environment.PlacementType = new SensorPlacementOrthogonalGrid(Environment.Sensors, o.OrthogonalStep);
                Environment.Sensors = Environment.PlacementType.PlaceSensors();

                Environment.SaveEnv(o.FilePath);

            }).WithParsed<PlaceSensorsRandomStepOptions>(o =>
            {
                var loader = new Loader(o.FilePath);
                Environment = loader.LoadEnv();

                Environment.Sensors.Clear();
                for (int i = 0; i < o.SensorsCount; i++)
                {
                    Environment.Sensors.Add(new Sensor(i));
                }
                
                Environment.PlacementType = new SensorPlacementRandomStep(Environment.Sensors, o.StepRange, o.DistrType, o.UniParameterA, o.UniParameterB);
                Environment.Sensors = Environment.PlacementType.PlaceSensors();

                Environment.SaveEnv(o.FilePath);

            }).WithParsed<PlaceSensorsPoissonOptions>(o =>
            {
                var loader = new Loader(o.FilePath);
                Environment = loader.LoadEnv();

                Environment.Sensors.Clear();
                for (int i = 0; i < o.SensorsCount; i++)
                {
                    Environment.Sensors.Add(new Sensor(i));
                }

                Vector3[] areaLimits = new Vector3[2]
                {
                    new Vector3(Environment.AreaLimits.Item1.X, Environment.AreaLimits.Item1.Y, Environment.AreaLimits.Item1.Z),
                    new Vector3(Environment.AreaLimits.Item2.X, Environment.AreaLimits.Item2.Y, Environment.AreaLimits.Item2.Z)
                };

                Environment.PlacementType = new SensorPlacementPoisson(Environment.Sensors, o.LambdaParameter, areaLimits);
                Environment.Sensors = Environment.PlacementType.PlaceSensors();

                Environment.SaveEnv(o.FilePath);
            });
    }
}
