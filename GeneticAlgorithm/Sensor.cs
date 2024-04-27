using System.Numerics;

namespace GeneticAlgorithm;

public class Sensor
{
    public Vector2 Position { get; set; }
    public bool IsReference { get; set; }
    public int ReferenceId { get; set; }
    public World World { get; set; }

    public Sensor(Vector2 position, World world)
    {
        Position = position;
        ReferenceId = -1;
        World = world;
    }

    public void StartSendingSignal()
    {
        if (!IsReference)
            World.SendToReferenceTries += 1;

        SendSignal(this);
    }

    public void FindPivot()
    {
        if (IsReference)
            return;

        float minDistance = float.PositiveInfinity;
        float minAreaSize = float.PositiveInfinity;

        var directions = new Vector2[]
        {
            new(0, 1), new(1, 0), new(-1, 0), new(0, -1)
        };

        foreach (var direction in directions)
        {
            var targetPosition = Position + direction;

            foreach (var reference in World.ReferenceSensors)
            {
                var diff = reference.Position - targetPosition;
                float distance = diff.X + diff.Y;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    ReferenceId = reference.ReferenceId;
                    minAreaSize = World.AreaCounter[ReferenceId];
                }
                else if (Math.Abs(distance - minDistance) < 0.001
                    && minAreaSize > World.AreaCounter[ReferenceId])
                {
                    ReferenceId = reference.ReferenceId;
                    minAreaSize = World.AreaCounter[ReferenceId];
                }
            }
        }

        World.AreaCounter[ReferenceId] += 1;
    }

    public void SendSignal(Sensor sender)
    {
        if (IsReference)
        {
            if (this != sender)
                World.SuccessfullMessages += 1;

            return;
        }

        int tries = 0;
        while (World.MaxN != -1 && tries <= World.MaxN)
        {
            var random = new Random().NextDouble();
            if (random <= World.Probability)
                break;

            World.TotalSends += 1;
            World.AreaEnergy[ReferenceId] += 1;

            tries += 1;
        }

        if (World.MaxN != -1 && tries >= World.MaxN)
        {
            World.TotalLostSends += 1;
            return;
        }

        World.TotalSends += 1;
        World.AreaEnergy[ReferenceId] += 1;

        float minDistance = float.PositiveInfinity;
        Sensor? nextSensor = null;

        var referencePosition =
            World.ReferenceSensors[ReferenceId].Position;

        var directions = new Vector2[]
        {
            new(0, 1), new(1, 0), new(-1, 0), new(0, -1)
        };

        foreach (var direction in directions)
        {
            var targetPosition = Position + direction;

            var hitSensor = World.GetSensor(targetPosition);

            if (hitSensor != null)
            {
                var difference = referencePosition - targetPosition;
                float distance = Math.Abs(difference.X) + Math.Abs(difference.Y);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nextSensor = hitSensor;
                }
            }
        }

        if (nextSensor == null)
        {
            throw new Exception("Что-то пошло не так");
        }

        nextSensor.SendSignal(sender);
    }
}