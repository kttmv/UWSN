using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm;

public static class Utilities
{
    public static bool ContourRule(float i, float j, float mu, float v)
    {
        return
            (j >= v - mu && j < v - mu + mu * 2 && i == v - mu) ||
            (j >= v - mu && j < v - mu + mu * 2 && i == v - mu + mu * 2 - 1) ||
            (i >= v - mu && i < v - mu + mu * 2 && j == v - mu) ||
            (i >= v - mu && i < v - mu + mu * 2 && j == v - mu + mu * 2 - 1);
    }

    public static bool SingleRule(float i, float j, float n)
    {
        return i == n - 1 && j == n - 1;
    }

    public static List<Vector2> GetRefPointsByRule
    (
        int n, Func<float, float, float, bool> rule
    )
    {
        var points = new List<Vector2>();

        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                if (rule(x, y, n))
                    points.Add(new Vector2(x, y));
            }
        }

        return points;
    }

    public static List<Vector2> GetRefPointsFromMu(int n, int mu)
    {
        var points = new List<Vector2>();

        int v = n / 2;

        for (int x = v - mu; x < v + mu; x++)
        {
            points.Add(new Vector2(x, v - mu));
        }

        for (int y = v - mu; y < v + mu; y++)
        {
            points.Add(new Vector2(v + mu - 1, y));
        }

        for (int x = v + mu - 1; x > v - mu - 1; x--)
        {
            points.Add(new Vector2(x, v + mu - 1));
        }

        for (int y = v + mu - 1; y > v - mu - 1; y--)
        {
            points.Add(new Vector2(v - mu, y));
        }

        return points;
    }

    public static float GetArea(Vector2 point1, Vector2 point2, Vector2 point3)
    {
        return
            (point2.X - point1.X) * (point3.Y - point1.Y)
            - (point2.Y - point1.Y) * (point3.X - point1.X);
    }

    private static bool Intersect(float a, float b, float c, float d)
    {
        if (a > b)
        {
            (b, a) = (a, b);
        }

        if (c > d)
        {
            (d, c) = (c, d);
        }

        return Math.Max(a, c) <= Math.Min(b, d);
    }

    public static bool Intersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return
            Intersect(a.X, b.X, c.X, d.X) &&
            Intersect(a.Y, b.Y, c.Y, d.Y) &&
            GetArea(a, b, c) * GetArea(a, b, d) <= 0 &&
            GetArea(c, d, a) * GetArea(c, d, b) <= 0;
    }
}