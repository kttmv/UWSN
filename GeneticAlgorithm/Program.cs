using System.Linq;
using System.Numerics;

int n = 10;
double v = n / 2.0;
List<double> overallXpoints = new List<double>();
List<List<double>> overallypoints = new List<List<double>>() { new List<double>(), new List<double>(), new List<double>() };
List<List<double>> overallyenergy = new List<List<double>>() { new List<double>(), new List<double>(), new List<double>() };
List<List<double>> overallylosts = new List<List<double>>() { new List<double>(), new List<double>(), new List<double>() };

List<List<int>> directions = new List<List<int>>() { new List<int>() { 1, 0 }, new List<int>() { -1, 0 }, new List<int>() { 0, 1 }, new List<int>() { 0, -1 } };

// Предполагается, что функция GetFitnessValue уже определена где-то в другом месте вашего кода
double GetFitnessValue(ImitationResults imitResult)
{
    return imitResult.Sends;  // *res.areaEnergyDispersion
}

// Предполагается, что функция GetRefPointsByRule уже определена где-то в другом месте вашего кода
var singleRefs = WorldImitation.GetRefPointsByRule(n, WorldImitation.SingleRule);

int N = 2;

for (int perim = 4; perim <= 60; perim++)
{
    overallXpoints.Add(perim * 100.0 / (4 * n));

    var contourRefs = new List<int[]>();

    int currentMu = 1;
    double bestMuSends = double.PositiveInfinity;
    for (int mu = 1; mu <= n / 2; mu++)
    {
        if ((1 + (mu - 1) * 2) * 4 > perim)
        {
            currentMu = mu - 1;
            break;
        }

        // Предполагается, что функции ContourRule и GetRefPointsByRule уже определены где-то в другом месте вашего кода
        Func<int, int, int, bool> CountourRuleLambda = (i, j, n) => WorldImitation.ContourRule(i, j, n, mu, v);
        contourRefs = WorldImitation.GetRefPointsByRule(n, CountourRuleLambda);

        // Предполагается, что класс WorldImitation и его метод Run уже определены где-то в другом месте вашего кода
        var wi_perim = new WorldImitation(n, 1, N, contourRefs);
        var res = wi_perim.Run(10);

        if (res.Sends > bestMuSends)
        {
            currentMu = mu - 1;
            break;
        }

        bestMuSends = res.Sends;
    }

    // Предполагается, что функция GetRefPointsFromMu уже определена где-то в другом месте вашего кода
    contourRefs = WorldImitation.GetRefPointsFromMu(n, currentMu);

    // Предполагается, что класс Polygon и его методы AddPoint, TryMovePoint, IsValid, GetLength, TryExpandByPoint уже определены где-то в другом месте вашего кода
    var pol = new Polygon(n);

    foreach (var pt in contourRefs)
    {
        pol.AddPoint(pt[0], pt[1]);
    }

    bool whileFlag = true;
    int curIter = 0;
    int maxIter = 10000;
    while (whileFlag && curIter < maxIter)
    {
        double minSends = double.PositiveInfinity;
        List<int> optPoint = null;
        List<int> optDir = null;

        foreach (var existPt in pol.pointsArr)
        {
            foreach (var dir in directions)
            {
                var newPol = new Polygon(pol);
                bool suc = newPol.TryMovePoint(existPt.x, existPt.y, dir[0], dir[1]);

                if (!suc || !newPol.IsValid() || newPol.GetLength() > perim)
                {
                    continue;
                }

                var curRefs = newPol.GetPoints();

                var curRefsList = new List<int[]>();

                foreach (var r in curRefs)
                {
                    var vecArr = new int[] { r.x, r.y };

                    curRefsList.Add(vecArr);
                }

                var wi_perim = new WorldImitation(n, 1, N, curRefsList);
                var res = wi_perim.Run(10);

                if (GetFitnessValue(res) < minSends)
                {
                    optPoint = new List<int>() { existPt.x, existPt.y };
                    minSends = res.Sends;
                    optDir = dir;
                }
            }
        }

        double moveMinSends = minSends;
        var movePoint = optPoint;

        minSends = double.PositiveInfinity;
        optPoint = null;

        foreach (var exPt in pol.pointsToExpand)
        {
            var newPol = new Polygon(pol);
            bool suc = newPol.ExpandByPoint(exPt.x, exPt.x);

            if (!suc || !newPol.IsValid() || newPol.GetLength() > perim)
            {
                continue;
            }

            var curRefs = newPol.GetPoints();

            var curRefsList = new List<int[]>();

            foreach (var r in curRefs)
            {
                var vecArr = new int[] { r.x, r.y };

                curRefsList.Add(vecArr);
            }

            var wi_perim = new WorldImitation(n, 1, N, curRefsList);
            var res = wi_perim.Run(10);

            if (GetFitnessValue(res) < minSends)
            {
                optPoint = new List<int>() { exPt.x, exPt.y }; ;
                minSends = res.Sends;
            }
        }

        whileFlag = false;

        if (double.IsInfinity(minSends) && double.IsInfinity(moveMinSends))
        {
            break;
        }

        if (minSends < moveMinSends)
        {
            pol.ExpandByPoint(optPoint[0], optPoint[1]);
            whileFlag = true;
        }
        else if (moveMinSends < minSends)
        {
            pol.TryMovePoint(movePoint[0], movePoint[1], optDir[0], optDir[1]);
            whileFlag = true;
        }

        curIter++;
    }

    Console.WriteLine("Is final valid? " + pol.IsValid());

    var output = pol.GetPoints();

    var contourRefsList = new List<int[]>();

    foreach (var r in output)
    {
        var vecArr = new int[] { r.x, r.y };

        contourRefsList.Add(vecArr);
    }

    contourRefs = contourRefsList;
    int idx = 0;
    foreach (double p in new List<double>() { 0.95, 0.7, 0.5 })
    {
        var wi_perim = new WorldImitation(n, p, N, contourRefs);
        var wi_perimEndless = new WorldImitation(n, p, N, singleRefs);
        var res = wi_perim.Run(10);
        var resEndless = wi_perimEndless.Run(10);

        overallypoints[idx].Add(res.Sends);
        if (res.Sends <= resEndless.Sends)
        {
            overallyenergy[idx].Add(resEndless.Sends * 100 / res.Sends);
        }
        else
        {
            overallyenergy[idx].Add(100);
        }
        overallylosts[idx].Add(res.Losts * 100 / (n * n - contourRefs.Count));
        idx++;
    }
}


public struct Vector2Int
{
    public int x;
    public int y;

    public Vector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Vector2Int))
            return false;

        Vector2Int vec = (Vector2Int)obj;

        if (x == vec.x && y == vec.y)
        {
            return true;
        }
        
        return false;
    }
}

public class World
{
    public List<Sensor> Refs { get; set; }
    public int N { get; set; }
    public double P { get; set; }
    public int MaxN { get; set; }
    public List<int> AreaCounter { get; set; }
    public List<int> AreaEnergy { get; set; }
    public Sensor[,] SensorMap { get; set; }
    public int SendsTotal { get; set; }
    public int LostTotal { get; set; }
    public int SucMessages { get; set; }
    public int TryNum { get; set; }

    public World(int n, double p, int maxN)
    {
        Refs = new List<Sensor>();
        N = n;
        P = p;
        MaxN = maxN;
        AreaCounter = new List<int>();
        AreaEnergy = new List<int>();
        SensorMap = new Sensor[n, n];
        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                SensorMap[x, y] = new Sensor(x, y, this);
            }
        }
        SendsTotal = 0;
        LostTotal = 0;
        SucMessages = 0;
        TryNum = 0;
    }

    public void SetRef(int x, int y)
    {
        if (x < 0 || x >= N || y < 0 || y >= N)
        {
            throw new Exception("x or y dont fit to world sensor map");
        }
        if (SensorMap[x, y].IsRef)
        {
            return;
        }
        SensorMap[x, y].SetRefFlag(true, Refs.Count);
        Refs.Add(SensorMap[x, y]);
        AreaCounter.Add(1);
        AreaEnergy.Add(0);
    }

    public Sensor GetSensor(int x, int y)
    {
        if (x < 0 || x >= N || y < 0 || y >= N)
        {
            return null;
        }
        return SensorMap[x, y];
    }
}

public class Sensor
{
    public int CoordX { get; set; }
    public int CoordY { get; set; }
    public World World { get; set; }
    public bool IsRef { get; set; }
    public int RefId { get; set; }

    public Sensor(int x, int y, World world)
    {
        CoordX = x;
        CoordY = y;
        World = world;
        IsRef = false;
        RefId = -1;
    }

    public void SetRefFlag(bool isRef, int refId)
    {
        IsRef = isRef;
        RefId = refId;
    }

    public void StartSendingSignal()
    {
        if (!IsRef)
        {
            World.TryNum++;
        }
        SendSignal(this);
    }

    public void FindPivot()
    {
        if (IsRef)
        {
            return;
        }

        double minLength = double.MaxValue;
        double minAreaSize = double.MaxValue;

        int[][] directions = new int[][] { new int[] { 0, 1 }, new int[] { 1, 0 }, new int[] { -1, 0 }, new int[] { 0, -1 } };
        foreach (var dir in directions)
        {
            int targetX = CoordX + dir[0];
            int targetY = CoordY + dir[1];

            foreach (var reference in World.Refs)
            {
                double dist = Math.Abs(reference.CoordX - targetX) + Math.Abs(reference.CoordY - targetY);

                if (dist < minLength)
                {
                    minLength = dist;
                    RefId = reference.RefId;
                    minAreaSize = World.AreaCounter[reference.RefId];
                }
                else if (Math.Abs(dist - minLength) < 0.001 && minAreaSize > World.AreaCounter[reference.RefId])
                {
                    RefId = reference.RefId;
                    minAreaSize = World.AreaCounter[reference.RefId];
                }
            }

            World.AreaCounter[RefId]++;
        }
    }

    public void SendSignal(Sensor sender)
    {
        if (IsRef)
        {
            if (this != sender)
            {
                World.SucMessages++;
            }
            return;
        }

        int tryNum = 0;
        while (World.MaxN == -1 || tryNum <= World.MaxN)
        {
            int rand = new Random().Next(1, 101);
            if (rand <= (int)(World.P * 100))
            {
                break;
            }

            World.SendsTotal++;
            World.AreaEnergy[RefId]++;

            tryNum++;
        }

        if (World.MaxN != -1 && tryNum >= World.MaxN)
        {
            World.LostTotal++;
            return;
        }

        World.SendsTotal++;
        World.AreaEnergy[RefId]++;

        double minLength = double.MaxValue;
        Sensor nextSensor = null;

        int refCoordX = World.Refs[RefId].CoordX;
        int refCoordY = World.Refs[RefId].CoordY;

        int[][] directions = new int[][] { new int[] { 0, 1 }, new int[] { 1, 0 }, new int[] { -1, 0 }, new int[] { 0, -1 } };
        foreach (var dir in directions)
        {
            int targetX = CoordX + dir[0];
            int targetY = CoordY + dir[1];

            Sensor hitSensor = World.GetSensor(targetX, targetY);

            if (hitSensor != null)
            {
                double dist = Math.Abs(refCoordX - targetX) + Math.Abs(refCoordY - targetY);

                if (dist < minLength)
                {
                    minLength = dist;
                    nextSensor = hitSensor;
                }
            }
        }

        nextSensor.SendSignal(sender);
    }
}

public class ImitationResults
{
    public Sensor[,] SensorMap { get; set; }
    public List<Sensor> Refs { get; set; }
    public int Sends { get; set; }
    public int SucReceives { get; set; }
    public int Losts { get; set; }
    public int TriesSendToRef { get; set; }
    public double AreaEnergyDispersion { get; set; }

    public ImitationResults()
    {
        SensorMap = new Sensor[0, 0];
        Refs = new List<Sensor>();
        Sends = 0;
        SucReceives = 0;
        Losts = 0;
        TriesSendToRef = 0;
        AreaEnergyDispersion = 0;
    }

    public static ImitationResults Imitation(int n, double p, int maxN, List<int[]> refPoints)
    {
        World world = new World(n, p, maxN);

        foreach (var point in refPoints)
        {
            world.SetRef(point[0], point[1]);
        }

        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                world.SensorMap[x, y].FindPivot();
            }
        }

        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                world.SensorMap[x, y].StartSendingSignal();
            }
        }

        ImitationResults res = new ImitationResults();
        res.SucReceives = world.SucMessages;
        res.Sends = world.SendsTotal;
        res.Losts = world.LostTotal;
        res.TriesSendToRef = world.TryNum;
        res.SensorMap = (Sensor[,])world.SensorMap.Clone();
        res.Refs = new List<Sensor>(world.Refs);

        double avgDistrib = n * n / world.Refs.Count;
        foreach (var distr in world.AreaCounter)
        {
            res.AreaEnergyDispersion += Math.Pow(distr - avgDistrib, 2);
        }

        res.AreaEnergyDispersion /= world.Refs.Count;

        return res;
    }
}

public class WorldImitation
{
    public int N { get; set; }
    public double P { get; set; }
    public int MaxN { get; set; }
    public List<int[]> RefPoints { get; set; }

    public WorldImitation(int n, double p, int maxN, List<int[]> refPoints)
    {
        N = n;
        P = p;
        MaxN = maxN;
        RefPoints = refPoints;
    }

    public ImitationResults Run(int imitationsNumber)
    {
        ImitationResults res = new ImitationResults();
        for (int im = 0; im < imitationsNumber; im++)
        {
            ImitationResults imResults = ImitationResults.Imitation(N, P, MaxN, RefPoints);
            res.Losts += imResults.Losts;
            res.Sends += imResults.Sends;
            res.SucReceives += imResults.SucReceives;
            res.TriesSendToRef += imResults.TriesSendToRef;
            res.AreaEnergyDispersion += imResults.AreaEnergyDispersion;
            res.SensorMap = imResults.SensorMap;
            res.Refs = imResults.Refs;
        }

        res.Losts /= imitationsNumber;
        res.Sends /= imitationsNumber;
        res.SucReceives /= imitationsNumber;
        res.TriesSendToRef /= imitationsNumber;
        res.AreaEnergyDispersion /= imitationsNumber;

        return res;
    }

    public static bool ContourRule(int i, int j, int n, int mu, double v)
    {
        return (
            (j >= v - mu && j < (v - mu) + (mu * 2) && i == v - mu)
            || (i == (v - mu) + (mu * 2) - 1 && j >= v - mu && j < (v - mu) + (mu * 2))
            || (i >= v - mu && i < (v - mu) + (mu * 2) && j == v - mu)
            || (i >= v - mu && i < (v - mu) + (mu * 2) && j == (v - mu) + (mu * 2) - 1)
        );
    }

    public static bool SingleRule(int i, int j, int n)
    {
        return i == n - 1 && j == n - 1;
    }

    public static List<int[]> GetRefPointsByRule(int n, Func<int, int, int, bool> rule)
    {
        List<int[]> points = new List<int[]>();

        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                if (rule(x, y, n))
                {
                    points.Add(new int[] { x, y });
                }
            }
        }

        return points;
    }

    public static List<int[]> GetRefPointsFromMu(int n, int mu)
    {
        List<int[]> points = new List<int[]>();

        int v = n / 2;

        for (int x = v - mu; x < v + mu; x++)
        {
            points.Add(new int[] { x, v - mu });
        }

        for (int y = v - mu; y < v + mu; y++)
        {
            points.Add(new int[] { v + mu - 1, y });
        }

        for (int x = v + mu - 1; x > v - mu - 1; x--)
        {
            points.Add(new int[] { x, v + mu - 1 });
        }

        for (int y = v + mu - 1; y > v - mu - 1; y--)
        {
            points.Add(new int[] { v - mu, y });
        }

        return points;
    }

    public static double Area(int[] pt1, int[] pt2, int[] pt3)
    {
        return (pt2[0] - pt1[0]) * (pt3[1] - pt1[1]) - (pt2[1] - pt1[1]) * (pt3[0] - pt1[0]);
    }

    public static bool Intersect_1(int a, int b, int c, int d)
    {
        if (a > b)
        {
            int temp = a;
            a = b;
            b = temp;
        }
        if (c > d)
        {
            int temp = c;
            c = d;
            d = temp;
        }
        return Math.Max(a, c) <= Math.Min(b, d);
    }

    public static bool Intersect(int[] a, int[] b, int[] c, int[] d)
    {
        return (
            Intersect_1(a[0], b[0], c[0], d[0])
            && Intersect_1(a[1], b[1], c[1], d[1])
            && Area(a, b, c) * Area(a, b, d) <= 0
            && Area(c, d, a) * Area(c, d, b) <= 0
        );
    }
}

public class Polygon
{
    public int n;
    public HashSet<Vector2Int> points;
    public HashSet<Vector2Int> pointsToExpand;
    public List<Vector2Int> pointsArr;
    public List<Vector2Int[]> edges;
    public double polyLen;

    public Polygon(int n)
    {
        this.n = n;
        this.points = new HashSet<Vector2Int>();
        this.pointsToExpand = new HashSet<Vector2Int>();
        this.pointsArr = new List<Vector2Int>();
        this.edges = new List<Vector2Int[]>();
        this.polyLen = 0;
    }
    
    public Polygon(Polygon pol)
    {
        n = pol.n;
        points = pol.points;
        pointsToExpand = pol.pointsToExpand;
        pointsArr = pol.pointsArr;
        edges = pol.edges;
        polyLen = pol.polyLen;
    }

    public List<Vector2Int> GetPoints()
    {
        return this.edges.Select(e => e[0]).ToList();
    }

    private double EdgeLength(Vector2Int p1, Vector2Int p2)
    {
        var dx = p1.x - p2.x;
        var dy = p1.y - p2.y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public double GetLength()
    {
        return this.polyLen;
    }

    public bool CheckCrossing()
    {
        foreach (var e in this.edges)
        {
            foreach (var e2 in this.edges)
            {
                if (e != e2)
                {
                    if (!(e[0].Equals(e2[1]) || e[0].Equals(e2[0]) || e[1].Equals(e2[1]) || e[1].Equals(e2[0])) 
                        && WorldImitation.Intersect(new int[2] { e[0].x, e[0].y }, new int[2] { e[1].x, e[1].y }, 
                                                    new int[2] { e2[0].x, e2[0].x }, new int[2] { e2[1].x, e2[1].x }))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public bool IsValid()
    {
        return !this.CheckCrossing();
    }

    private void UpdateEdgesByAddPoints(Vector2Int pt)
    {
        var currentPointsNum = this.pointsArr.Count;

        if (currentPointsNum < 1)
        {
            return;
        }

        if (currentPointsNum == 1)
        {
            var edge = new[] { this.pointsArr[currentPointsNum - 1], pt };
            this.polyLen += this.EdgeLength(edge[0], edge[1]);
            this.edges.Add(edge);

            edge = new[] { pt, this.pointsArr[currentPointsNum - 1] };
            this.polyLen += this.EdgeLength(edge[0], edge[1]);
            this.edges.Add(edge);
            return;
        }

        this.polyLen -= this.EdgeLength(this.edges.Last()[0], this.edges.Last()[1]);
        var edge2 = new[] { this.pointsArr.Last(), pt };
        this.polyLen += this.EdgeLength(edge2[0], edge2[1]);
        this.edges[edges.Count - 1] = edge2;
        this.edges.Add(new[] { pt, this.pointsArr[0] });
        this.polyLen += this.EdgeLength(this.edges.Last()[0], this.edges.Last()[1]);
    }

    public void AddPoint(int x, int y)
    {
        if (x < 0 || x >= this.n || y < 0 || y >= this.n)
        {
            return;
        }

        var pt = new Vector2Int(x, y);

        if (this.pointsArr.Contains(pt))
        {
            return;
        }

        this.UpdateEdgesByAddPoints(pt);

        this.points.Add(pt);
        this.pointsArr.Add(pt);

        if (this.pointsToExpand.Contains(pt))
        {
            this.pointsToExpand.Remove(pt);
        }

        this.AddPointToExpand(x + 1, y);
        this.AddPointToExpand(x - 1, y);
        this.AddPointToExpand(x, y + 1);
        this.AddPointToExpand(x, y - 1);
    }

    public bool ExpandByPoint(int x, int y)
    {
        if (x < 0 || x >= this.n || y < 0 || y >= this.n)
        {
            return false;
        }

        var pt = new Vector2Int(x, y);
        var curIdx = 0;
        var minEdgeIdx = -1;
        var minSumNewLength = double.PositiveInfinity;
        foreach (var e in this.edges)
        {
            var curExpandLength = (
                this.EdgeLength(e[0], pt)
                + this.EdgeLength(e[1], pt)
                - this.EdgeLength(e[0], e[1])
            );
            if (curExpandLength < minSumNewLength)
            {
                minSumNewLength = curExpandLength;
                minEdgeIdx = curIdx;
            }

            curIdx++;
        }

        var edge = this.edges[minEdgeIdx];
        this.polyLen += (
            this.EdgeLength(edge[0], pt)
            + this.EdgeLength(edge[1], pt)
            - this.EdgeLength(edge[0], edge[1])
        );
        var temp = this.edges[minEdgeIdx][1];
        this.edges[minEdgeIdx][1] = pt;
        this.edges.Insert(minEdgeIdx + 1, new[] { pt, temp });

        if (this.pointsToExpand.Contains(pt))
        {
            this.pointsToExpand.Remove(pt);
        }

        this.points.Add(pt);
        this.pointsArr.Add(pt);
        this.AddPointToExpand(x + 1, y);
        this.AddPointToExpand(x - 1, y);
        this.AddPointToExpand(x, y + 1);
        this.AddPointToExpand(x, y - 1);

        return true;
    }

    public bool TryMovePoint(int x, int y, int dirX, int dirY)
    {
        if (x < 0 || x >= this.n || y < 0 || y >= this.n)
        {
            return false;
        }

        var old = new Vector2Int(x, y);
        var newPt = new Vector2Int(x + dirX, y + dirY);
        if (!this.pointsToExpand.Contains(newPt))
        {
            return false;
        }

        this.pointsToExpand.Remove(newPt);

        foreach (var e in this.edges)
        {
            if (e[0].Equals(old))
            {
                this.polyLen -= this.EdgeLength(e[0], e[1]);
                e[0] = newPt;
                this.polyLen += this.EdgeLength(e[0], e[1]);
            }
            if (e[1].Equals(old))
            {
                this.polyLen -= this.EdgeLength(e[0], e[1]);
                e[1] = newPt;
                this.polyLen += this.EdgeLength(e[0], e[1]);
            }
        }

        this.points.Remove(old);
        this.pointsArr.Remove(old);
        this.points.Add(newPt);
        this.pointsArr.Add(newPt);

        this.AddPointToExpand(newPt.x + 1, newPt.y);
        this.AddPointToExpand(newPt.x - 1, newPt.y);
        this.AddPointToExpand(newPt.x, newPt.y + 1);
        this.AddPointToExpand(newPt.x, newPt.y - 1);

        return true;
    }

    private void AddPointToExpand(int x, int y)
    {
        if (x < 0 || x >= this.n || y < 0 || y >= this.n)
        {
            return;
        }
        var pt = new Vector2Int(x, y);
        if (!this.points.Contains(pt))
        {
            this.pointsToExpand.Add(pt);
        }
    }
}







