using System.Numerics;

namespace GeneticAlgorithm;

public class World
{
    public Sensor[,] Sensors { get; set; }
    public List<Sensor> ReferenceSensors { get; set; }

    public List<int> AreaCounter { get; set; }
    public List<float> AreaEnergy { get; set; }

    public int SendToReferenceTries { get; set; }
    public int TotalSends { get; set; }
    public int TotalLostSends { get; set; }
    public int SuccessfullMessages { get; set; }

    public int N { get; set; }
    public int MaxN { get; set; }
    public float Probability { get; private set; }

    public World(int n, int maxN, float probability)
    {
        N = n;
        Probability = probability;
        MaxN = maxN;

        ReferenceSensors = new();
        Sensors = new Sensor[n, n];
        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                Sensors[x, y] = new Sensor(new Vector2(x, y), this);
            }
        }

        AreaCounter = new();
        AreaEnergy = new();
    }

    public void SetReference(Vector2 position)
    {
        if (position.X < 0 || position.X >= N ||
            position.Y < 0 || position.Y >= N)
        {
            throw new Exception("Координаты выходят за границы");
        }

        var sensor = Sensors[(int)position.X, (int)position.Y];

        if (sensor.IsReference)
            return;

        sensor.IsReference = true;
        sensor.ReferenceId = ReferenceSensors.Count;
        ReferenceSensors.Add(sensor);

        AreaCounter.Add(1);
        AreaEnergy.Add(0);
    }

    public Sensor? GetSensor(Vector2 position)
    {
        if (position.X < 0 || position.X >= N ||
            position.Y < 0 || position.Y >= N)
        {
            return null;
        }

        return Sensors[(int)position.X, (int)position.Y];
    }
}