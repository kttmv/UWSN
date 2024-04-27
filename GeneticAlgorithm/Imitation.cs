using System.Numerics;

namespace GeneticAlgorithm;

public class Imitation
{
    public class ImitationResult
    {
        public required Sensor[,] Sensors { get; set; }
        public required List<Sensor> ReferenceSensors { get; set; }

        public required float AreaEnergyDispersion { get; set; }

        public required int TotalSends { get; set; }
        public required int TotalLostSends { get; set; }
        public required int SuccessfullMessages { get; set; }
        public required int SendToReferenceTries { get; set; }

        public int FitnessValue { get { return TotalSends; } }
    }

    public static ImitationResult Imitate
    (
        int n, int maxN, List<Vector2> referencePoints, float probability
    )
    {
        var world = new World(n, maxN, probability);

        foreach (var point in referencePoints)
        {
            world.SetReference(point);
        }

        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                var sensor = world.GetSensor(new(x, y))
                    ?? throw new Exception("Что-то пошло не так");
                sensor.FindPivot();
            }
        }

        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                var sensor = world.GetSensor(new(x, y))
                    ?? throw new Exception("Что-то пошло не так");
                sensor.StartSendingSignal();
            }
        }

        float areaEnergyDispersion = 0;

        float averageDistribution = n * n / world.ReferenceSensors.Count;
        foreach (var distribution in world.AreaCounter)
        {
            areaEnergyDispersion +=
                (float)Math.Pow(distribution - averageDistribution, 2);
        }

        areaEnergyDispersion /= world.ReferenceSensors.Count;

        var result = new ImitationResult()
        {
            SuccessfullMessages = world.SuccessfullMessages,
            TotalSends = world.TotalSends,
            TotalLostSends = world.TotalLostSends,
            SendToReferenceTries = world.SendToReferenceTries,
            Sensors = (Sensor[,])world.Sensors.Clone(),
            ReferenceSensors = new(world.ReferenceSensors),
            AreaEnergyDispersion = areaEnergyDispersion
        };

        return result;
    }
}