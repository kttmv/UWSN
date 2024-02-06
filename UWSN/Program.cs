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
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Parser.Default.ParseArguments<InitOptions, PlaceSensorsOrthOptions, PlaceSensorsRandomStepOptions, PlaceSensorsPoissonOptions, PlaceSensorsFromFileOptions>(args)
            .WithParsed<InitOptions>(o =>
            {
                var areaLimits = o.AreaLimits.ToList();
                var v1 = new Vector3(areaLimits[0], areaLimits[1], areaLimits[2]);
                var v2 = new Vector3(areaLimits[3], areaLimits[4], areaLimits[5]);
                Environment = new Model.Environment
                {
                    AreaLimits = new Tuple<Vector3, Vector3>(v1, v2),
                    Sensors = new List<Model.Sensor>()
                };

                Console.WriteLine("Инициализация окружения проведена успешно.");
                Console.WriteLine($"Границы окружения: {v1}, {v2}");

                float length = Math.Abs(v1.X - v2.X);
                float width = Math.Abs(v1.Y - v2.Y);
                float height = Math.Abs(v1.Z - v2.Z);
                float volume = length * width * height;
                Console.WriteLine($"Объем окружения: {volume} м³");

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

                Environment.Sensors = new SensorPlacementOrthogonalGrid(Environment.Sensors, o.OrthogonalStep).PlaceSensors();

                Console.WriteLine($"Расстановка сенсоров ({o.SensorsCount}) на ортогональной сетке прошла успешно.");

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

                var placement = new SensorPlacementRandomStep(
                    Environment.Sensors, o.StepRange, o.DistrType, o.UniParameterA, o.UniParameterB
                );
                Environment.Sensors = placement.PlaceSensors();

                var distributionType = o.DistrType == "Normal" ? "нормальному" : "непрерывному равномерному";

                Console.WriteLine($"Расстановка сенсоров ({o.SensorsCount}) по {distributionType} распределению прошла успешно.");

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

                Environment.Sensors = new SensorPlacementPoisson(Environment.Sensors, o.LambdaParameter, areaLimits).PlaceSensors();

                Console.WriteLine($"Расстановка сенсоров ({o.SensorsCount}) по закону Пуассона прошла успешно.");

                Environment.SaveEnv(o.FilePath);

            }).WithParsed<PlaceSensorsFromFileOptions>(o =>
            {
                var loader = new Loader(o.FilePath);
                Environment = loader.LoadEnv();

                Environment.Sensors.Clear();

                Environment.Sensors = new SensorPlacementFromFile(o.SensorsFilePath).PlaceSensors();
                Console.WriteLine($"Расстановка сенсоров ({Environment.Sensors.Count}) из пользовательского файла прошла успешно.");

                Environment.SaveEnv(o.FilePath);
            });
    }
}
