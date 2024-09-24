// Scripts/Utils/Utility.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility class providing global static methods for various calculations.
/// </summary>
public static class Utility
{
    public enum MovementType
    {
        Diagonal,
        NonDiagonal,
        All
    }

    // Player UI variables
    public static Vector2 GetMouseWorldPosition()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(mousePos.x, mousePos.y);
    }
    
    public static bool MouseUp() => Input.GetMouseButtonUp(0);
    public static bool MouseDown() => Input.GetMouseButtonDown(0);



    // Vector math
    public static Vector2Int RoundVector2(Vector2 position) => new Vector2Int((int)Mathf.Round(position.x), (int)Mathf.Round(position.y));

    public static bool InBounds(Vector2 minPt, Vector2 maxPt, Vector2 givenPt)
    {
        Vector2 effectiveMin = new Vector2(Mathf.Min(minPt.x, maxPt.x), Mathf.Min(minPt.y, maxPt.y));
        Vector2 effectiveMax = new Vector2(Mathf.Max(minPt.x, maxPt.x), Mathf.Max(minPt.y, maxPt.y));

        return effectiveMin.x <= givenPt.x && givenPt.x <= effectiveMax.x &&
               effectiveMin.y <= givenPt.y && givenPt.y <= effectiveMax.y;
    }

    // Bresenham's Line Algorithm
    public static HashSet<Vector2Int> GetLinePoints(Vector2Int start, Vector2Int end)
    {
        HashSet<Vector2Int> points = new HashSet<Vector2Int>();
        int dx = Mathf.Abs(end.x - start.x);
        int dy = Mathf.Abs(end.y - start.y);
        int sx = (start.x < end.x) ? 1 : -1;
        int sy = (start.y < end.y) ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            points.Add(start);
            if (start.x == end.x && start.y == end.y) break;

            int err2 = err * 2;
            if (err2 > -dy)
            {
                err -= dy;
                start.x += sx;
            }

            if (err2 < dx)
            {
                err += dx;
                start.y += sy;
            }
        }

        return points;
    }

    public static HashSet<Vector2Int> GetIntermediatePoints(Vector2Int start, Vector2Int end, MovementType type)
    {
        switch (type)
        {
            case MovementType.Diagonal:
                return GetDiagonalIntermediatePoints(start, end);
            case MovementType.NonDiagonal:
                return GetNonDiagonalIntermediatePoints(start, end);
            case MovementType.All:
                return GetAllIntermediatePoints(start, end);
            default:
                return new HashSet<Vector2Int>(); // Return an empty set for unsupported types
        }
    }

    private static HashSet<Vector2Int> GetDiagonalIntermediatePoints(Vector2Int start, Vector2Int end)
    {
        HashSet<Vector2Int> points = new HashSet<Vector2Int>();
        int dx = end.x - start.x;
        int dy = end.y - start.y;

        if (Mathf.Abs(dx) != Mathf.Abs(dy)) return points; // Only valid for diagonal moves

        int stepX = (dx > 0) ? 1 : -1;
        int stepY = (dy > 0) ? 1 : -1;
        int steps = Mathf.Abs(dx);

        for (int i = 1; i < steps; i++)
        {
            points.Add(new Vector2Int(start.x + i * stepX, start.y + i * stepY));
        }

        return points;
    }

    private static HashSet<Vector2Int> GetNonDiagonalIntermediatePoints(Vector2Int start, Vector2Int end)
    {
        HashSet<Vector2Int> points = new HashSet<Vector2Int>();
        int dx = end.x - start.x;
        int dy = end.y - start.y;

        if (dx == 0) // Vertical move
        {
            int stepY = (dy > 0) ? 1 : -1;
            for (int i = 1; i < Mathf.Abs(dy); i++)
            {
                points.Add(new Vector2Int(start.x, start.y + i * stepY));
            }
        }
        else if (dy == 0) // Horizontal move
        {
            int stepX = (dx > 0) ? 1 : -1;
            for (int i = 1; i < Mathf.Abs(dx); i++)
            {
                points.Add(new Vector2Int(start.x + i * stepX, start.y));
            }
        }

        return points;
    }
    public static HashSet<Vector2Int> GetAllIntermediatePoints(Vector2Int start, Vector2Int end)
    {
        HashSet<Vector2Int> points = new HashSet<Vector2Int>();

        int dx = end.x - start.x;
        int dy = end.y - start.y;

        int steps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));

        for (int i = 1; i < steps; i++)
        {
            float t = (float)i / steps;
            int x = (int)Mathf.Lerp(start.x, end.x, t);
            int y = (int)Mathf.Lerp(start.y, end.y, t);
            points.Add(new Vector2Int(x, y));
        }

        return points;
    }

    public static HashSet<Vector2Int> GetSurroundingPoints(Vector2Int center)
    {
        HashSet<Vector2Int> surroundingPoints = new HashSet<Vector2Int>();
        Vector2Int[] offsets = {
            new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
            new Vector2Int(-1, 0),                           new Vector2Int(1, 0),
            new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1)
        };

        foreach (var offset in offsets)
        {
            surroundingPoints.Add(center + offset);
        }

        return surroundingPoints;
    }

    public static HashSet<Vector2Int> GetAllPointsInArea(Vector2Int start, Vector2Int end)
    {
        HashSet<Vector2Int> points = new HashSet<Vector2Int>();
        int minX = Mathf.Min(start.x, end.x);
        int maxX = Mathf.Max(start.x, end.x);
        int minY = Mathf.Min(start.y, end.y);
        int maxY = Mathf.Max(start.y, end.y);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                points.Add(new Vector2Int(x, y));
            }
        }

        return points;
    }

    // // Containers    
    // public static HashSet<T> FindAll<T>(HashSet<T> set, Func<T, bool> predicate)
    // {
    //     HashSet<T> result = new HashSet<T>();
    //     foreach (var item in set)
    //     {
    //         if (predicate(item))
    //         {
    //             result.Add(item);
    //         }
    //     }
    //     return result;
    // }

    // public static HashSet<T> FindAllAnd<T>(HashSet<T> set, Func<T, bool> predicate, bool additionalCondition)
    // {
    //     HashSet<T> result = new HashSet<T>();
    //     foreach (var item in set)
    //     {
    //         if (predicate(item) && additionalCondition)
    //         {
    //             result.Add(item);
    //         }
    //     }
    //     return result;
    // }


}
