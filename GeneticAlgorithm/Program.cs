using System.Drawing;
using System.Numerics;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace GeneticAlgorithm;

public class Program
{
    public static void Main()
    {
        int n = 10;
        float v = n / 2;

        var overallPointsX = new List<float>();

        var overallPointsY = new List<List<float>>
        {
            new(),
            new(),
            new()
        };

        var overallEnergyY = new List<List<float>>
        {
            new(),
            new(),
            new()
        };

        var overallLostsY = new List<List<float>>
        {
            new(),
            new(),
            new()
        };

        var directions = new Vector2[]
        {
            new(1, 0),
            new(-1, 0),
            new(0, 1),
            new(0, -1),
        };

        int maxN = 2;

        for (int perimeter = 4; perimeter < 61; perimeter++)
        {
            overallPointsX.Add(perimeter * 100 / (4 * n));

            int currentMu = 1;
            float bestMuSends = float.PositiveInfinity;

            for (int mu = 1; mu < n / 2 + 1; mu++)
            {
                if ((1 + (mu - 1) * 2) * 4 > perimeter)
                {
                    currentMu = mu - 1;
                    break;
                }

                bool contourRule(float i, float j, float n) => Utilities.ContourRule(i, j, mu, v);
                var contourReferencesMu = Utilities.GetRefPointsByRule(n, contourRule);

                var worldImitation = new WorldImitation(n, maxN, 1, contourReferencesMu);
                var result = worldImitation.Run(10);

                if (result.TotalSends > bestMuSends)
                {
                    currentMu = mu - 1;

                    // почему-то в коде быстрова эта строчка находится вне
                    // условия. скорее всего ошибка
                    bestMuSends = result.TotalSends;

                    break;
                }
            }

            var contourReferences = Utilities.GetRefPointsFromMu(n, currentMu);

            var polygon = new Polygon(n);

            foreach (var point in contourReferences)
            {
                polygon.AddPoint(point);
            }

            bool flag = true;
            int currentIteration = 0;
            int maxIterations = 10000;

            while (flag && currentIteration < maxIterations)
            {
                int minSends = int.MaxValue;
                Vector2? optimalPoint = null;
                Vector2? optimalDirection = null;

                foreach (var existPoint in polygon.PointsList)
                {
                    foreach (var direction in directions)
                    {
                        var newPolygon = new Polygon(polygon);
                        bool success = newPolygon.TryMovePoint(existPoint, direction);

                        if (!success || !newPolygon.IsValid
                            || newPolygon.Perimeter > perimeter)
                        {
                            continue;
                        }

                        var currentReferences = newPolygon.GetPoints();

                        var worldImitation = new WorldImitation(n, maxN, 1, currentReferences);
                        var result = worldImitation.Run(10);

                        if (result.FitnessValue < minSends)
                        {
                            optimalPoint = existPoint;
                            minSends = result.FitnessValue;
                            optimalDirection = direction;
                        }
                    }
                }

                int moveMinSends = minSends;
                Vector2? movePoint = optimalPoint;

                minSends = int.MaxValue;
                optimalPoint = null;

                foreach (var expandPoint in polygon.PointsToExpand)
                {
                    var newPolygon = new Polygon(polygon);
                    var success = newPolygon.ExpandByPoint(expandPoint);

                    if (!success
                        || !newPolygon.IsValid
                        || newPolygon.Perimeter > perimeter)
                    {
                        continue;
                    }

                    var currentReferences = newPolygon.GetPoints();

                    var worldImitation = new WorldImitation(n, maxN, 1, currentReferences);
                    var result = worldImitation.Run(10);

                    if (result.FitnessValue < minSends)
                    {
                        optimalPoint = expandPoint;
                        minSends = result.FitnessValue;
                    }
                }

                flag = false;

                if (minSends == int.MaxValue && moveMinSends == int.MaxValue)
                {
                    break;
                }

                if (minSends < moveMinSends)
                {
                    polygon.ExpandByPoint(optimalPoint.Value);
                    flag = true;
                }
                else if (moveMinSends < minSends)
                {
                    polygon.TryMovePoint(movePoint.Value, optimalDirection.Value);
                    flag = true;
                }
                currentIteration += 1;
            }

            Console.WriteLine("Is final valid? " + polygon.IsValid);

            contourReferences = polygon.GetPoints();

            var probabilities = new float[] { 0.95f, 0.7f, 0.5f };

            for (int i = 0; i < probabilities.Length; i++)
            {
                float probability = probabilities[i];

                var worldImitation =
                    new WorldImitation(n, maxN, probability, contourReferences);
                var worldImitationEndless =
                    new WorldImitation(n, maxN, probability, contourReferences);

                var result = worldImitation.Run(10);
                var resultEndless = worldImitationEndless.Run(10);

                // ShowSensorMap("sensor_mao_for"+str(perim), res.sensorMap, res.refs, contourRefs)

                overallPointsY[i].Add(result.TotalSends);

                if (result.TotalSends <= resultEndless.TotalSends)
                {
                    overallEnergyY[i]
                        .Add(resultEndless.TotalSends * 100 / result.TotalSends);
                }
                else
                {
                    overallEnergyY[i].Add(100);
                }

                overallLostsY[i].Add(
                    result.TotalLostSends * 100 / (n * n - contourReferences.Count));
            }

            // ShowRefPoints("opt_for_"+str(perim), n, contourRefs)
        }

        //plt.cla()
        //plt.plot(overallXpoints, overallypoints[0], label = 'for p=' + str(0.95) + ' ga')
        //plt.plot(overallXpoints, overallypoints[1], label = 'for p=' + str(0.7) + ' ga')
        //plt.plot(overallXpoints, overallypoints[2], label = 'for p=' + str(0.5) + ' ga')
        //ax = plt.gca()
        //ax.xaxis.set_major_locator(MaxNLocator(integer = True))
        //ax.set_xlabel('Perimetr')
        //ax.set_ylabel('Lp')
        //ax.grid(True)
        //ax.legend()
        //ax.set_title('Среднее число пересылок')
        //plt.savefig('imgs\\test_overall_sends.png')

        //plt.cla()
        //plt.plot(overallXpoints, overallyenergy[0], label = 'for p=' + str(0.95) + ' ga')
        //plt.plot(overallXpoints, overallyenergy[1], label = 'for p=' + str(0.7) + ' ga')
        //plt.plot(overallXpoints, overallyenergy[2], label = 'for p=' + str(0.5) + ' ga')
        //ax = plt.gca()
        //ax.xaxis.set_major_locator(MaxNLocator(integer = True))
        //ax.set_xlabel('Периметр контура к периметру сети(%)')
        //ax.set_ylabel('EspLp(%)')
        //ax.grid(True)
        //ax.legend()
        //ax.set_title('Отношение количества перепосылок')
        //plt.savefig('imgs\\test_overall_energy.png')

        //plt.cla()
        //plt.plot(overallXpoints, overallylosts[0], label = 'for p=' + str(0.95) + ' ga')
        //plt.plot(overallXpoints, overallylosts[1], label = 'for p=' + str(0.7) + ' ga')
        //plt.plot(overallXpoints, overallylosts[2], label = 'for p=' + str(0.5) + ' ga')
        //ax = plt.gca()
        //ax.xaxis.set_major_locator(MaxNLocator(integer = True))
        //ax.set_xlabel('Отношение периметра контура к периметру сети(%)')
        //ax.set_ylabel('EpsP')
        //ax.grid(True)
        //ax.legend()
        //ax.set_title('Средний процент потерянных сообщений')
        //plt.savefig('imgs\\test_overall_losts.png')
    }
}

//def ShowSensorMap(name, sensorMap, refs, refPoints):
//    idImg = len(refPoints)
//    markers = ["o", "v", "^", ">", "<", "1", "2", "3", "4", "s", "p", "+", "x", "D", "d"]
//    markerIdx = -1

//    plt.figure(10)
//    plt.cla()
//    plt.gca().yaxis.set_major_locator(MaxNLocator(integer=True))
//    plt.gca().xaxis.set_major_locator(MaxNLocator(integer=True))
//    contourX = np.array([])
//    contourY = np.array([])
//    for coord in refPoints:
//        contourX = np.append(contourX, coord[0])
//        contourY = np.append(contourY, coord[1])

//    contourX = np.append(contourX, refPoints[0][0])
//    contourY = np.append(contourY, refPoints[0][1])

//    plt.plot(contourX, contourY)

//    for ref in refs:
//        markerIdx += 1
//        markerIdx %= len(markers)

//        xSens = np.array([])
//        ySens = np.array([])

//        figX = np.array([])
//        figY = np.array([])
//        for sensors in sensorMap:
//            for sensor in sensors:
//                if ref.refId == sensor.refId:
//                    if sensor.isRef:
//                        figX = np.append(figX, sensor.coordX)
//                        figY = np.append(figY, sensor.coordY)
//                    xSens = np.append(xSens, sensor.coordX)
//                    ySens = np.append(ySens, sensor.coordY)

//        plt.scatter(xSens, ySens, s=30, marker=markers[markerIdx])

//    plt.savefig('imgs\\'+name+str(idImg)+'refs_test_img.png')

//def ShowRefPoints(name, n, refPoints):
//    xSens = np.array([])
//    ySens = np.array([])
//    for x in range(n) :
//        for y in range(n) :
//            xSens = np.append(xSens, x)
//            ySens = np.append(ySens, y)

//    figX = np.array([])
//    figY = np.array([])
//    for coord in refPoints:
//        figX = np.append(figX, coord[0])
//        figY = np.append(figY, coord[1])

//    figX = np.append(figX, refPoints[0][0])
//    figY = np.append(figY, refPoints[0][1])

//    plt.cla()
//    plt.scatter(xSens, ySens, 1)
//    plt.scatter(figX, figY, 10)
//    plt.plot(figX, figY)
//    plt.savefig('imgs\\'+name+'_test_img.png')

// ПЕРВАЯ ПОПЫТКА ПЕРЕПИСАТЬ КОД

//using System.Linq;
//using System.Numerics;

//int n = 10;
//double v = n / 2.0;
//List<double> overallXpoints = new List<double>();
//List<List<double>> overallypoints = new List<List<double>>() { new List<double>(), new List<double>(), new List<double>() };
//List<List<double>> overallyenergy = new List<List<double>>() { new List<double>(), new List<double>(), new List<double>() };
//List<List<double>> overallylosts = new List<List<double>>() { new List<double>(), new List<double>(), new List<double>() };

//List<List<int>> directions = new List<List<int>>() { new List<int>() { 1, 0 }, new List<int>() { -1, 0 }, new List<int>() { 0, 1 }, new List<int>() { 0, -1 } };

//// Предполагается, что функция GetFitnessValue уже определена где-то в другом месте вашего кода
//double GetFitnessValue(ImitationResults imitResult)
//{
//    return imitResult.Sends;  // *res.areaEnergyDispersion
//}

//// Предполагается, что функция GetRefPointsByRule уже определена где-то в другом месте вашего кода
//var singleRefs = WorldImitation.GetRefPointsByRule(n, WorldImitation.SingleRule);

//int N = 2;

//for (int perim = 4; perim <= 60; perim++)
//{
//    overallXpoints.Add(perim * 100.0 / (4 * n));

//    var contourRefs = new List<int[]>();

//    int currentMu = 1;
//    double bestMuSends = double.PositiveInfinity;
//    for (int mu = 1; mu <= n / 2; mu++)
//    {
//        if ((1 + (mu - 1) * 2) * 4 > perim)
//        {
//            currentMu = mu - 1;
//            break;
//        }

//        // Предполагается, что функции ContourRule и GetRefPointsByRule уже определены где-то в другом месте вашего кода
//        Func<int, int, int, bool> CountourRuleLambda = (i, j, n) => WorldImitation.ContourRule(i, j, n, mu, v);
//        contourRefs = WorldImitation.GetRefPointsByRule(n, CountourRuleLambda);

//        // Предполагается, что класс WorldImitation и его метод Run уже определены где-то в другом месте вашего кода
//        var wi_perim = new WorldImitation(n, 1, N, contourRefs);
//        var res = wi_perim.Run(10);

//        if (res.Sends > bestMuSends)
//        {
//            currentMu = mu - 1;
//            break;
//        }

//        bestMuSends = res.Sends;
//    }

//    // Предполагается, что функция GetRefPointsFromMu уже определена где-то в другом месте вашего кода
//    contourRefs = WorldImitation.GetRefPointsFromMu(n, currentMu);

//    // Предполагается, что класс Polygon и его методы AddPoint, TryMovePoint, IsValid, GetLength, TryExpandByPoint уже определены где-то в другом месте вашего кода
//    var pol = new Polygon(n);

//    foreach (var pt in contourRefs)
//    {
//        pol.AddPoint(pt[0], pt[1]);
//    }

//    bool whileFlag = true;
//    int curIter = 0;
//    int maxIter = 10000;
//    while (whileFlag && curIter < maxIter)
//    {
//        double minSends = double.PositiveInfinity;
//        List<int> optPoint = null;
//        List<int> optDir = null;

//        for (int i = 0; i < pol.pointsArr.Count; i++)
//        {
//            var existPt = pol.pointsArr[i];
//            foreach (var dir in directions)
//            {
//                var newPol = new Polygon(pol);
//                bool suc = newPol.TryMovePoint(existPt.x, existPt.y, dir[0], dir[1]);

//                if (!suc || !newPol.IsValid() || newPol.GetLength() > perim)
//                {
//                    continue;
//                }

//                var curRefs = newPol.GetPoints();

//                var curRefsList = new List<int[]>();

//                foreach (var r in curRefs)
//                {
//                    var vecArr = new int[] { r.x, r.y };

//                    curRefsList.Add(vecArr);
//                }

//                var wi_perim = new WorldImitation(n, 1, N, curRefsList);
//                var res = wi_perim.Run(10);

//                if (GetFitnessValue(res) < minSends)
//                {
//                    optPoint = new List<int>() { existPt.x, existPt.y };
//                    minSends = res.Sends;
//                    optDir = dir;
//                }
//            }
//        }

//        double moveMinSends = minSends;
//        var movePoint = optPoint;

//        minSends = double.PositiveInfinity;
//        optPoint = null;

//        for (int i = 0; i < pol.pointsToExpand.Count; i++)
//        {
//            var exPt = pol.pointsToExpand.ToList()[i];
//            var newPol = new Polygon(pol);
//            bool suc = newPol.ExpandByPoint(exPt.x, exPt.x);

//            if (!suc || !newPol.IsValid() || newPol.GetLength() > perim)
//            {
//                continue;
//            }

//            var curRefs = newPol.GetPoints();

//            var curRefsList = new List<int[]>();

//            foreach (var r in curRefs)
//            {
//                var vecArr = new int[] { r.x, r.y };

//                curRefsList.Add(vecArr);
//            }

//            var wi_perim = new WorldImitation(n, 1, N, curRefsList);
//            var res = wi_perim.Run(10);

//            if (GetFitnessValue(res) < minSends)
//            {
//                optPoint = new List<int>() { exPt.x, exPt.y };
//                minSends = res.Sends;
//            }
//        }

//        whileFlag = false;

//        if (double.IsInfinity(minSends) && double.IsInfinity(moveMinSends))
//        {
//            break;
//        }

//        if (minSends < moveMinSends)
//        {
//            pol.ExpandByPoint(optPoint[0], optPoint[1]);
//            whileFlag = true;
//        }
//        else if (moveMinSends < minSends)
//        {
//            pol.TryMovePoint(movePoint[0], movePoint[1], optDir[0], optDir[1]);
//            whileFlag = true;
//        }

//        curIter++;
//    }

//    Console.WriteLine("Is final valid? " + pol.IsValid());

//    var output = pol.GetPoints();

//    var contourRefsList = new List<int[]>();

//    foreach (var r in output)
//    {
//        var vecArr = new int[] { r.x, r.y };

//        contourRefsList.Add(vecArr);
//    }

//    contourRefs = contourRefsList;
//    int idx = 0;
//    foreach (double p in new List<double>() { 0.95, 0.7, 0.5 })
//    {
//        var wi_perim = new WorldImitation(n, p, N, contourRefs);
//        var wi_perimEndless = new WorldImitation(n, p, N, singleRefs);
//        var res = wi_perim.Run(10);
//        var resEndless = wi_perimEndless.Run(10);

//        overallypoints[idx].Add(res.Sends);
//        if (res.Sends <= resEndless.Sends)
//        {
//            overallyenergy[idx].Add(resEndless.Sends * 100 / res.Sends);
//        }
//        else
//        {
//            overallyenergy[idx].Add(100);
//        }
//        overallylosts[idx].Add(res.Losts * 100 / (n * n - contourRefs.Count));
//        idx++;
//    }
//}

//public struct Vector2Int
//{
//    public int x;
//    public int y;

//    public Vector2Int(int x, int y)
//    {
//        this.x = x;
//        this.y = y;
//    }

//    public override bool Equals(object obj)
//    {
//        if (!(obj is Vector2Int))
//            return false;

//        Vector2Int vec = (Vector2Int)obj;

//        if (x == vec.x && y == vec.y)
//        {
//            return true;
//        }

//        return false;
//    }
//}

//public class World
//{
//    public List<Sensor> Refs { get; set; }
//    public int N { get; set; }
//    public double P { get; set; }
//    public int MaxN { get; set; }
//    public List<int> AreaCounter { get; set; }
//    public List<int> AreaEnergy { get; set; }
//    public Sensor[,] SensorMap { get; set; }
//    public int SendsTotal { get; set; }
//    public int LostTotal { get; set; }
//    public int SucMessages { get; set; }
//    public int TryNum { get; set; }

//    public World(int n, double p, int maxN)
//    {
//        Refs = new List<Sensor>();
//        N = n;
//        P = p;
//        MaxN = maxN;
//        AreaCounter = new List<int>();
//        AreaEnergy = new List<int>();
//        SensorMap = new Sensor[n, n];
//        for (int x = 0; x < n; x++)
//        {
//            for (int y = 0; y < n; y++)
//            {
//                SensorMap[x, y] = new Sensor(x, y, this);
//            }
//        }
//        SendsTotal = 0;
//        LostTotal = 0;
//        SucMessages = 0;
//        TryNum = 0;
//    }

//    public void SetRef(int x, int y)
//    {
//        if (x < 0 || x >= N || y < 0 || y >= N)
//        {
//            throw new Exception("x or y dont fit to world sensor map");
//        }
//        if (SensorMap[x, y].IsRef)
//        {
//            return;
//        }
//        SensorMap[x, y].SetRefFlag(true, Refs.Count);
//        Refs.Add(SensorMap[x, y]);
//        AreaCounter.Add(1);
//        AreaEnergy.Add(0);
//    }

//    public Sensor GetSensor(int x, int y)
//    {
//        if (x < 0 || x >= N || y < 0 || y >= N)
//        {
//            return null;
//        }
//        return SensorMap[x, y];
//    }
//}

//public class Sensor
//{
//    public int CoordX { get; set; }
//    public int CoordY { get; set; }
//    public World World { get; set; }
//    public bool IsRef { get; set; }
//    public int RefId { get; set; }

//    public Sensor(int x, int y, World world)
//    {
//        CoordX = x;
//        CoordY = y;
//        World = world;
//        IsRef = false;
//        RefId = -1;
//    }

//    public void SetRefFlag(bool isRef, int refId)
//    {
//        IsRef = isRef;
//        RefId = refId;
//    }

//    public void StartSendingSignal()
//    {
//        if (!IsRef)
//        {
//            World.TryNum++;
//        }
//        SendSignal(this);
//    }

//    public void FindPivot()
//    {
//        if (IsRef)
//        {
//            return;
//        }

//        double minLength = double.MaxValue;
//        double minAreaSize = double.MaxValue;

//        int[][] directions = new int[][] { new int[] { 0, 1 }, new int[] { 1, 0 }, new int[] { -1, 0 }, new int[] { 0, -1 } };
//        foreach (var dir in directions)
//        {
//            int targetX = CoordX + dir[0];
//            int targetY = CoordY + dir[1];

//            foreach (var reference in World.Refs)
//            {
//                double dist = Math.Abs(reference.CoordX - targetX) + Math.Abs(reference.CoordY - targetY);

//                if (dist < minLength)
//                {
//                    minLength = dist;
//                    RefId = reference.RefId;
//                    minAreaSize = World.AreaCounter[reference.RefId];
//                }
//                else if (Math.Abs(dist - minLength) < 0.001 && minAreaSize > World.AreaCounter[reference.RefId])
//                {
//                    RefId = reference.RefId;
//                    minAreaSize = World.AreaCounter[reference.RefId];
//                }
//            }
//        }

//        World.AreaCounter[RefId]++;
//    }

//    public void SendSignal(Sensor sender)
//    {
//        if (IsRef)
//        {
//            if (this != sender)
//            {
//                World.SucMessages++;
//            }
//            return;
//        }

//        int tryNum = 0;
//        while (World.MaxN == -1 || tryNum <= World.MaxN)
//        {
//            int rand = new Random().Next(1, 101);
//            if (rand <= (int)(World.P * 100))
//            {
//                break;
//            }

//            World.SendsTotal++;
//            World.AreaEnergy[RefId]++;

//            tryNum++;
//        }

//        if (World.MaxN != -1 && tryNum >= World.MaxN)
//        {
//            World.LostTotal++;
//            return;
//        }

//        World.SendsTotal++;
//        World.AreaEnergy[RefId]++;

//        double minLength = double.MaxValue;
//        Sensor nextSensor = null;

//        int refCoordX = World.Refs[RefId].CoordX;
//        int refCoordY = World.Refs[RefId].CoordY;

//        int[][] directions = new int[][] { new int[] { 0, 1 }, new int[] { 1, 0 }, new int[] { -1, 0 }, new int[] { 0, -1 } };
//        foreach (var dir in directions)
//        {
//            int targetX = CoordX + dir[0];
//            int targetY = CoordY + dir[1];

//            Sensor hitSensor = World.GetSensor(targetX, targetY);

//            if (hitSensor != null)
//            {
//                double dist = Math.Abs(refCoordX - targetX) + Math.Abs(refCoordY - targetY);

//                if (dist < minLength)
//                {
//                    minLength = dist;
//                    nextSensor = hitSensor;
//                }
//            }
//        }

//        nextSensor.SendSignal(sender);
//    }
//}

//public class ImitationResults
//{
//    public Sensor[,] SensorMap { get; set; }
//    public List<Sensor> Refs { get; set; }
//    public int Sends { get; set; }
//    public int SucReceives { get; set; }
//    public int Losts { get; set; }
//    public int TriesSendToRef { get; set; }
//    public double AreaEnergyDispersion { get; set; }

//    public ImitationResults()
//    {
//        SensorMap = new Sensor[0, 0];
//        Refs = new List<Sensor>();
//        Sends = 0;
//        SucReceives = 0;
//        Losts = 0;
//        TriesSendToRef = 0;
//        AreaEnergyDispersion = 0;
//    }

//    public static ImitationResults Imitation(int n, double p, int maxN, List<int[]> refPoints)
//    {
//        World world = new World(n, p, maxN);

//        foreach (var point in refPoints)
//        {
//            world.SetRef(point[0], point[1]);
//        }

//        for (int x = 0; x < n; x++)
//        {
//            for (int y = 0; y < n; y++)
//            {
//                world.SensorMap[x, y].FindPivot();
//            }
//        }

//        for (int x = 0; x < n; x++)
//        {
//            for (int y = 0; y < n; y++)
//            {
//                world.SensorMap[x, y].StartSendingSignal();
//            }
//        }

//        ImitationResults res = new ImitationResults();
//        res.SucReceives = world.SucMessages;
//        res.Sends = world.SendsTotal;
//        res.Losts = world.LostTotal;
//        res.TriesSendToRef = world.TryNum;
//        res.SensorMap = (Sensor[,])world.SensorMap.Clone();
//        res.Refs = new List<Sensor>(world.Refs);

//        double avgDistrib = n * n / world.Refs.Count;
//        foreach (var distr in world.AreaCounter)
//        {
//            res.AreaEnergyDispersion += Math.Pow(distr - avgDistrib, 2);
//        }

//        res.AreaEnergyDispersion /= world.Refs.Count;

//        return res;
//    }
//}

//public class WorldImitation
//{
//    public int N { get; set; }
//    public double P { get; set; }
//    public int MaxN { get; set; }
//    public List<int[]> RefPoints { get; set; }

//    public WorldImitation(int n, double p, int maxN, List<int[]> refPoints)
//    {
//        N = n;
//        P = p;
//        MaxN = maxN;
//        RefPoints = refPoints;
//    }

//    public ImitationResults Run(int imitationsNumber)
//    {
//        ImitationResults res = new ImitationResults();
//        for (int im = 0; im < imitationsNumber; im++)
//        {
//            ImitationResults imResults = ImitationResults.Imitation(N, P, MaxN, RefPoints);
//            res.Losts += imResults.Losts;
//            res.Sends += imResults.Sends;
//            res.SucReceives += imResults.SucReceives;
//            res.TriesSendToRef += imResults.TriesSendToRef;
//            res.AreaEnergyDispersion += imResults.AreaEnergyDispersion;
//            res.SensorMap = imResults.SensorMap;
//            res.Refs = imResults.Refs;
//        }

//        res.Losts /= imitationsNumber;
//        res.Sends /= imitationsNumber;
//        res.SucReceives /= imitationsNumber;
//        res.TriesSendToRef /= imitationsNumber;
//        res.AreaEnergyDispersion /= imitationsNumber;

//        return res;
//    }

//    public static bool ContourRule(int i, int j, int n, int mu, double v)
//    {
//        return (
//            (j >= v - mu && j < (v - mu) + (mu * 2) && i == v - mu)
//            || (i == (v - mu) + (mu * 2) - 1 && j >= v - mu && j < (v - mu) + (mu * 2))
//            || (i >= v - mu && i < (v - mu) + (mu * 2) && j == v - mu)
//            || (i >= v - mu && i < (v - mu) + (mu * 2) && j == (v - mu) + (mu * 2) - 1)
//               );
//    }

//    public static bool SingleRule(int i, int j, int n)
//    {
//        return i == n - 1 && j == n - 1;
//    }

//    public static List<int[]> GetRefPointsByRule(int n, Func<int, int, int, bool> rule)
//    {
//        List<int[]> points = new List<int[]>();

//        for (int x = 0; x < n; x++)
//        {
//            for (int y = 0; y < n; y++)
//            {
//                if (rule(x, y, n))
//                {
//                    points.Add(new int[] { x, y });
//                }
//            }
//        }

//        return points;
//    }

//    public static List<int[]> GetRefPointsFromMu(int n, int mu)
//    {
//        List<int[]> points = new List<int[]>();

//        int v = n / 2;

//        for (int x = v - mu; x < v + mu; x++)
//        {
//            points.Add(new int[] { x, v - mu });
//        }

//        for (int y = v - mu; y < v + mu; y++)
//        {
//            points.Add(new int[] { v + mu - 1, y });
//        }

//        for (int x = v + mu - 1; x > v - mu - 1; x--)
//        {
//            points.Add(new int[] { x, v + mu - 1 });
//        }

//        for (int y = v + mu - 1; y > v - mu - 1; y--)
//        {
//            points.Add(new int[] { v - mu, y });
//        }

//        return points;
//    }

//    public static double Area(int[] pt1, int[] pt2, int[] pt3)
//    {
//        return (pt2[0] - pt1[0]) * (pt3[1] - pt1[1]) - (pt2[1] - pt1[1]) * (pt3[0] - pt1[0]);
//    }

//    public static bool Intersect_1(int a, int b, int c, int d)
//    {
//        if (a > b)
//        {
//            int temp = a;
//            a = b;
//            b = temp;
//        }
//        if (c > d)
//        {
//            int temp = c;
//            c = d;
//            d = temp;
//        }
//        return Math.Max(a, c) <= Math.Min(b, d);
//    }

//    public static bool Intersect(int[] a, int[] b, int[] c, int[] d)
//    {
//        return (
//            Intersect_1(a[0], b[0], c[0], d[0])
//            && Intersect_1(a[1], b[1], c[1], d[1])
//            && Area(a, b, c) * Area(a, b, d) <= 0
//            && Area(c, d, a) * Area(c, d, b) <= 0
//               );
//    }
//}

//public class Polygon
//{
//    public int n;
//    public HashSet<Vector2Int> points;
//    public HashSet<Vector2Int> pointsToExpand;
//    public List<Vector2Int> pointsArr;
//    public List<Vector2Int[]> edges;
//    public double polyLen;

//    public Polygon(int n)
//    {
//        this.n = n;
//        this.points = new HashSet<Vector2Int>();
//        this.pointsToExpand = new HashSet<Vector2Int>();
//        this.pointsArr = new List<Vector2Int>();
//        this.edges = new List<Vector2Int[]>();
//        this.polyLen = 0;
//    }

//    public Polygon(Polygon pol)
//    {
//        n = pol.n;
//        points = new(pol.points);
//        pointsToExpand = new(pol.pointsToExpand);
//        pointsArr = new(pol.pointsArr);
//        edges = new(pol.edges);
//        polyLen = pol.polyLen;
//    }

//    public List<Vector2Int> GetPoints()
//    {
//        return this.edges.Select(e => e[0]).ToList();
//    }

//    private double EdgeLength(Vector2Int p1, Vector2Int p2)
//    {
//        var dx = p1.x - p2.x;
//        var dy = p1.y - p2.y;
//        return Math.Sqrt(dx * dx + dy * dy);
//    }

//    public double GetLength()
//    {
//        return this.polyLen;
//    }

//    public bool CheckCrossing()
//    {
//        foreach (var e in this.edges)
//        {
//            foreach (var e2 in this.edges)
//            {
//                if (e != e2)
//                {
//                    if (!(e[0].Equals(e2[1]) || e[0].Equals(e2[0]) || e[1].Equals(e2[1]) || e[1].Equals(e2[0]))
//                        && WorldImitation.Intersect(new int[2] { e[0].x, e[0].y }, new int[2] { e[1].x, e[1].y },
//                                                    new int[2] { e2[0].x, e2[0].x }, new int[2] { e2[1].x, e2[1].x }))
//                    {
//                        return true;
//                    }
//                }
//            }
//        }

//        return false;
//    }

//    public bool IsValid()
//    {
//        return !this.CheckCrossing();
//    }

//    private void UpdateEdgesByAddPoints(Vector2Int pt)
//    {
//        var currentPointsNum = this.pointsArr.Count;

//        if (currentPointsNum < 1)
//        {
//            return;
//        }

//        if (currentPointsNum == 1)
//        {
//            var edge = new[] { this.pointsArr[currentPointsNum - 1], pt };
//            this.polyLen += this.EdgeLength(edge[0], edge[1]);
//            this.edges.Add(edge);

//            edge = new[] { pt, this.pointsArr[currentPointsNum - 1] };
//            this.polyLen += this.EdgeLength(edge[0], edge[1]);
//            this.edges.Add(edge);
//            return;
//        }

//        this.polyLen -= this.EdgeLength(this.edges.Last()[0], this.edges.Last()[1]);
//        var edge2 = new[] { this.pointsArr.Last(), pt };
//        this.polyLen += this.EdgeLength(edge2[0], edge2[1]);
//        this.edges[edges.Count - 1] = edge2;
//        this.edges.Add(new[] { pt, this.pointsArr[0] });
//        this.polyLen += this.EdgeLength(this.edges.Last()[0], this.edges.Last()[1]);
//    }

//    public void AddPoint(int x, int y)
//    {
//        if (x < 0 || x >= this.n || y < 0 || y >= this.n)
//        {
//            return;
//        }

//        var pt = new Vector2Int(x, y);

//        if (this.pointsArr.Contains(pt))
//        {
//            return;
//        }

//        this.UpdateEdgesByAddPoints(pt);

//        this.points.Add(pt);
//        this.pointsArr.Add(pt);

//        if (this.pointsToExpand.Contains(pt))
//        {
//            this.pointsToExpand.Remove(pt);
//        }

//        this.AddPointToExpand(x + 1, y);
//        this.AddPointToExpand(x - 1, y);
//        this.AddPointToExpand(x, y + 1);
//        this.AddPointToExpand(x, y - 1);
//    }

//    public bool ExpandByPoint(int x, int y)
//    {
//        if (x < 0 || x >= this.n || y < 0 || y >= this.n)
//        {
//            return false;
//        }

//        var pt = new Vector2Int(x, y);
//        var curIdx = 0;
//        var minEdgeIdx = -1;
//        var minSumNewLength = double.PositiveInfinity;
//        foreach (var e in this.edges)
//        {
//            var curExpandLength = (
//                this.EdgeLength(e[0], pt)
//                + this.EdgeLength(e[1], pt)
//                - this.EdgeLength(e[0], e[1])
//                                  );
//            if (curExpandLength < minSumNewLength)
//            {
//                minSumNewLength = curExpandLength;
//                minEdgeIdx = curIdx;
//            }

//            curIdx++;
//        }

//        var edge = this.edges[minEdgeIdx];
//        this.polyLen += (
//            this.EdgeLength(edge[0], pt)
//            + this.EdgeLength(edge[1], pt)
//            - this.EdgeLength(edge[0], edge[1])
//                        );
//        var temp = this.edges[minEdgeIdx][1];
//        this.edges[minEdgeIdx][1] = pt;
//        this.edges.Insert(minEdgeIdx + 1, new[] { pt, temp });

//        if (this.pointsToExpand.Contains(pt))
//        {
//            this.pointsToExpand.Remove(pt);
//        }

//        this.points.Add(pt);
//        this.pointsArr.Add(pt);
//        this.AddPointToExpand(x + 1, y);
//        this.AddPointToExpand(x - 1, y);
//        this.AddPointToExpand(x, y + 1);
//        this.AddPointToExpand(x, y - 1);

//        return true;
//    }

//    public bool TryMovePoint(int x, int y, int dirX, int dirY)
//    {
//        if (x < 0 || x >= this.n || y < 0 || y >= this.n)
//        {
//            return false;
//        }

//        var old = new Vector2Int(x, y);
//        var newPt = new Vector2Int(x + dirX, y + dirY);
//        if (!this.pointsToExpand.Contains(newPt))
//        {
//            return false;
//        }

//        this.pointsToExpand.Remove(newPt);

//        foreach (var e in this.edges)
//        {
//            if (e[0].Equals(old))
//            {
//                this.polyLen -= this.EdgeLength(e[0], e[1]);
//                e[0] = newPt;
//                this.polyLen += this.EdgeLength(e[0], e[1]);
//            }
//            if (e[1].Equals(old))
//            {
//                this.polyLen -= this.EdgeLength(e[0], e[1]);
//                e[1] = newPt;
//                this.polyLen += this.EdgeLength(e[0], e[1]);
//            }
//        }

//        this.points.Remove(old);
//        this.pointsArr.Remove(old);
//        this.points.Add(newPt);
//        this.pointsArr.Add(newPt);

//        this.AddPointToExpand(newPt.x + 1, newPt.y);
//        this.AddPointToExpand(newPt.x - 1, newPt.y);
//        this.AddPointToExpand(newPt.x, newPt.y + 1);
//        this.AddPointToExpand(newPt.x, newPt.y - 1);

//        return true;
//    }

//    private void AddPointToExpand(int x, int y)
//    {
//        if (x < 0 || x >= this.n || y < 0 || y >= this.n)
//        {
//            return;
//        }
//        var pt = new Vector2Int(x, y);
//        if (!this.points.Contains(pt))
//        {
//            this.pointsToExpand.Add(pt);
//        }
//    }
//}