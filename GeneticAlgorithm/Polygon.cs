using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm;

public class Polygon
{
    public int N { get; set; }
    public HashSet<Vector2> Points { get; set; }
    public HashSet<Vector2> PointsToExpand { get; set; }
    public List<Vector2> PointsList { get; set; }
    public List<(Vector2, Vector2)> Edges { get; set; }
    public float Perimeter { get; set; }

    public bool IsValid { get { return !CheckCrossing(); } }

    public Polygon(int n)
    {
        N = n;
        Points = new();
        PointsToExpand = new();
        PointsList = new();
        Edges = new();
    }

    public Polygon(Polygon old)
    {
        N = old.N;
        Points = new(old.Points);
        PointsToExpand = new(old.PointsToExpand);
        PointsList = new(old.PointsList);
        Edges = new(old.Edges);
        Perimeter = old.Perimeter;
    }

    public List<Vector2> GetPoints()
    {
        var points = new List<Vector2>();
        foreach (var edge in Edges)
        {
            points.Add(new Vector2(edge.Item1.X, edge.Item1.Y));
        }

        return points;
    }

    private static float GetEdgeLength(Vector2 point1, Vector2 point2)
    {
        var difference = point1 - point2;
        return difference.Length();
    }

    private bool CheckCrossing()
    {
        foreach (var edge1 in Edges)
        {
            foreach (var edge2 in Edges)
            {
                if (edge1 == edge2)
                    continue;

                if
                (
                    !(
                        edge1.Item1 == edge2.Item2 || edge1.Item1 == edge2.Item1 ||
                        edge1.Item2 == edge2.Item2 || edge1.Item2 == edge1.Item1
                    )
                    &&
                    Utilities.Intersect(
                        edge1.Item1, edge2.Item2, edge1.Item1, edge2.Item2)
                )
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void UpdateEdgesByAddingPoint(Vector2 point)
    {
        int currentPointNum = PointsList.Count;

        if (currentPointNum < 1)
        {
            return;
        }

        if (currentPointNum == 1)
        {
            var edgeStart = PointsList[currentPointNum - 1];
            var edgeEnd = point;

            Perimeter += GetEdgeLength(edgeStart, edgeEnd);
            Edges.Add((edgeStart, edgeEnd));

            edgeStart = point;
            edgeEnd = PointsList[currentPointNum - 1];

            Perimeter += GetEdgeLength(edgeStart, edgeEnd);
            Edges.Add((edgeStart, edgeEnd));

            return;
        }

        Perimeter -= GetEdgeLength(Edges[^1].Item1, Edges[^1].Item2);

        var newEdgeStart = PointsList[^1];
        var newEdgeEnd = point;

        Perimeter += GetEdgeLength(newEdgeStart, newEdgeEnd);

        Edges[^1] = (newEdgeStart, newEdgeEnd);
        Edges.Add((point, PointsList[^1]));

        Perimeter += GetEdgeLength(Edges[^1].Item1, Edges[^1].Item2);
    }

    private void AddPointToExpand(Vector2 point)
    {
        if (point.X < 0 || point.Y < 0 || point.X >= N || point.Y >= N)
        {
            return;
        }

        if (!Points.Contains(point))
        {
            PointsToExpand.Add(point);
        }
    }

    public void AddPoint(Vector2 point)
    {
        if (point.X < 0 || point.Y < 0 || point.X >= N || point.Y >= N)
        {
            return;
        }

        if (PointsList.Contains(point))
        {
            return;
        }

        UpdateEdgesByAddingPoint(point);

        Points.Add(point);
        PointsList.Add(point);

        if (PointsToExpand.Contains(point))
        {
            PointsToExpand.Remove(point);
        }

        AddPointToExpand(new Vector2(point.X + 1, point.Y));
        AddPointToExpand(new Vector2(point.X - 1, point.Y));
        AddPointToExpand(new Vector2(point.X, point.Y + 1));
        AddPointToExpand(new Vector2(point.X, point.Y - 1));
    }

    public bool ExpandByPoint(Vector2 point)
    {
        if (point.X < 0 || point.Y < 0 || point.X >= N || point.Y >= N)
        {
            return false;
        }

        int currentIndex = 0;
        int minEdgeIndex = -1;
        float minNewLengthSum = float.PositiveInfinity;

        foreach (var e in Edges)
        {
            float currentExpandLength =
                GetEdgeLength(e.Item1, point)
                + GetEdgeLength(e.Item2, point)
                - GetEdgeLength(e.Item1, e.Item2);

            if (currentExpandLength < minNewLengthSum)
            {
                minNewLengthSum = currentExpandLength;
                minEdgeIndex = currentIndex;
            }

            currentIndex++;
        }

        var edge = Edges[minEdgeIndex];

        Perimeter +=
            GetEdgeLength(edge.Item1, point)
            + GetEdgeLength(edge.Item2, point)
            - GetEdgeLength(edge.Item1, edge.Item2);

        var temp = Edges[minEdgeIndex].Item2;

        Edges[minEdgeIndex] = (edge.Item1, point);
        Edges.Insert(minEdgeIndex + 1, (point, temp));

        if (PointsToExpand.Contains(point))
        {
            PointsToExpand.Remove(point);
        }

        Points.Add(point);
        PointsList.Add(point);

        AddPointToExpand(new Vector2(point.X + 1, point.Y));
        AddPointToExpand(new Vector2(point.X - 1, point.Y));
        AddPointToExpand(new Vector2(point.X, point.Y + 1));
        AddPointToExpand(new Vector2(point.X, point.Y - 1));

        return true;
    }

    public bool TryMovePoint(Vector2 point, Vector2 direction)
    {
        if (point.X < 0 || point.Y < 0 || point.X >= N || point.Y >= N)
        {
            return false;
        }

        var newPoint = point + direction;

        if (!PointsToExpand.Contains(newPoint))
        {
            return false;
        }

        PointsToExpand.Remove(newPoint);

        for (int i = 0; i < Edges.Count; i++)
        {
            if (Edges[i].Item1 == point)
            {
                Perimeter -= GetEdgeLength(Edges[i].Item1, Edges[i].Item2);
                Edges[i] = (newPoint, Edges[i].Item2);
                Perimeter += GetEdgeLength(Edges[i].Item1, Edges[i].Item2);
            }
            if (Edges[i].Item2 == point)
            {
                Perimeter -= GetEdgeLength(Edges[i].Item1, Edges[i].Item2);
                Edges[i] = (Edges[i].Item1, newPoint);
                Perimeter += GetEdgeLength(Edges[i].Item1, Edges[i].Item2);
            }
        }

        Points.Remove(point);
        PointsList.Remove(point);
        Points.Add(newPoint);
        PointsList.Add(newPoint);

        AddPointToExpand(new Vector2(newPoint.X + 1, newPoint.Y));
        AddPointToExpand(new Vector2(newPoint.X - 1, newPoint.Y));
        AddPointToExpand(new Vector2(newPoint.X, newPoint.Y + 1));
        AddPointToExpand(new Vector2(newPoint.X, newPoint.Y - 1));

        return true;
    }
}