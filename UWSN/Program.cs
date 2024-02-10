using System.Numerics;
using CommandLine;
using UWSN.CommandLine;
using UWSN.Model;

namespace UWSN;

public class Program
{
    private static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Parser.Default.ParseArguments
            <
                InitOptions,
                PlaceSensorsOrthOptions,
                PlaceSensorsRandomStepOptions,
                PlaceSensorsPoissonOptions,
                PlaceSensorsFromFileOptions
            >
            (args)
            .WithParsed<InitOptions>(o =>
            {
                var areaLimits = o.AreaLimits.ToList();
                var v1 = new Vector3(areaLimits[0], areaLimits[1], areaLimits[2]);
                var v2 = new Vector3(areaLimits[3], areaLimits[4], areaLimits[5]);

                var environment = new Model.Environment(v1, v2, new List<Sensor>());

                Console.WriteLine("Инициализация окружения проведена успешно.");
                Console.WriteLine($"Границы окружения: {v1}, {v2}");

                float length = Math.Abs(v1.X - v2.X);
                float width = Math.Abs(v1.Y - v2.Y);
                float height = Math.Abs(v1.Z - v2.Z);
                float volume = length * width * height;

                Console.WriteLine($"Объем окружения: {volume} м³");

                environment.SaveEnv(o.FilePath);
            })
            .WithParsed<PlaceSensorsOrthOptions>(o =>
            {
                var loader = new Loader(o.FilePath);
                var environment = loader.LoadEnv();

                environment.Sensors.Clear();
                for (int i = 0; i < o.SensorsCount; i++)
                {
                    environment.Sensors.Add(new Sensor(i));
                }

                environment.Sensors = new SensorPlacementOrthogonalGrid(environment.Sensors, o.OrthogonalStep).PlaceSensors();

                Console.WriteLine($"Расстановка сенсоров ({o.SensorsCount}) на ортогональной сетке прошла успешно.");

                environment.SaveEnv(o.FilePath);
            })
            .WithParsed<PlaceSensorsRandomStepOptions>(o =>
            {
                var loader = new Loader(o.FilePath);
                var environment = loader.LoadEnv();
                environment.Sensors.Clear();

                for (int i = 0; i < o.SensorsCount; i++)
                {
                    environment.Sensors.Add(new Sensor(i));
                }

                var placement = new SensorPlacementRandomStep(
                    environment.Sensors,
                    o.StepRange,
                    o.DistributionType,
                    o.UniParameterA,
                    o.UniParameterB);

                environment.Sensors = placement.PlaceSensors();

                string distributionType = o.DistributionType switch
                {
                    (DistributionType.Normal) => "нормальному",
                    (DistributionType.Uniform) => "непрерывному равномерному",
                    _ => throw new NotImplementedException(),
                };

                Console.WriteLine($"Расстановка сенсоров ({o.SensorsCount}) по {distributionType} распределению прошла успешно.");

                environment.SaveEnv(o.FilePath);
            })
            .WithParsed<PlaceSensorsPoissonOptions>(o =>
            {
                var loader = new Loader(o.FilePath);
                var environment = loader.LoadEnv();
                environment.Sensors.Clear();

                for (int i = 0; i < o.SensorsCount; i++)
                {
                    environment.Sensors.Add(new Sensor(i));
                }

                Vector3[] areaLimits = new Vector3[2]
                {
                    new Vector3(environment.AreaLimits.Item1.X, environment.AreaLimits.Item1.Y, environment.AreaLimits.Item1.Z),
                    new Vector3(environment.AreaLimits.Item2.X, environment.AreaLimits.Item2.Y, environment.AreaLimits.Item2.Z)
                };

                environment.Sensors = new SensorPlacementPoisson(environment.Sensors, o.LambdaParameter, areaLimits).PlaceSensors();

                Console.WriteLine($"Расстановка сенсоров ({o.SensorsCount}) по закону Пуассона прошла успешно.");

                environment.SaveEnv(o.FilePath);
            })
            .WithParsed<PlaceSensorsFromFileOptions>(o =>
            {
                var loader = new Loader(o.FilePath);
                var environment = loader.LoadEnv();
                environment.Sensors.Clear();

                environment.Sensors = new SensorPlacementFromFile(o.SensorsFilePath).PlaceSensors();
                Console.WriteLine($"Расстановка сенсоров ({environment.Sensors.Count}) из пользовательского файла прошла успешно.");

                environment.SaveEnv(o.FilePath);
            });
    }
}