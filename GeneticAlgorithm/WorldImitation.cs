using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static GeneticAlgorithm.Imitation;

namespace GeneticAlgorithm;

public class WorldImitation
{
    public List<Vector2> ReferencePoints { get; set; }

    public int N { get; set; }
    public int MaxN { get; set; }
    public float Probability { get; set; }

    public WorldImitation
    (
        int n, int maxN, float probability, List<Vector2> referencePoints
    )
    {
        N = n;
        MaxN = maxN;
        Probability = probability;
        ReferencePoints = referencePoints;
    }

    public ImitationResult Run(int imitationsCount)
    {
        ImitationResult? result = null;

        for (int i = 0; i < imitationsCount; i++)
        {
            var imitationResult = Imitate(N, MaxN, ReferencePoints, Probability);

            if (result == null)
            {
                result = imitationResult;
            }
            else
            {
                result.TotalLostSends += imitationResult.TotalLostSends;
                result.TotalSends += imitationResult.TotalSends;
                result.SuccessfullMessages += imitationResult.SuccessfullMessages;
                result.SendToReferenceTries += imitationResult.SendToReferenceTries;
                result.AreaEnergyDispersion += imitationResult.AreaEnergyDispersion;
                result.Sensors = imitationResult.Sensors;
                result.ReferenceSensors = imitationResult.ReferenceSensors;
            }
        }

        if (result == null)
            throw new Exception("Что-то пошло не так");

        result.TotalLostSends /= imitationsCount;
        result.TotalSends /= imitationsCount;
        result.SuccessfullMessages /= imitationsCount;
        result.SendToReferenceTries /= imitationsCount;
        result.AreaEnergyDispersion /= imitationsCount;

        return result;
    }
}