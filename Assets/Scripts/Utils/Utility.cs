// Scripts/Utils/Utility.cs
using UnityEngine;
using System.Collections.Generic;
using System;


/// <summary>
/// Utility class providing global static methods for various calculations.
/// </summary>
public static class Utility
{
    public enum MovementType
    {
        Diagonal,
        NonDiagonal,
        Any
    }

    // Player UI variables
    /*
    get where the user's mouse is currently
    */
    public static Vector2 GetMouseWorldPosition()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(mousePos.x, mousePos.y);
    }
    
    /*
    get if the user released the left-click button down
    */
    public static bool MouseUp() => Input.GetMouseButtonUp(0);
    /*
    get if the user pressed the left-click button down
    */
    public static bool MouseDown() => Input.GetMouseButtonDown(0);



    // Vector math

    /*
    get if the closest integer location given float location
    */
    public static Vector2Int RoundVector2(Vector2 position) => new Vector2Int((int)Mathf.Round(position.x), (int)Mathf.Round(position.y));

    /*
    get if a given point is in the bounds given by the min and max points
    */
    public static bool InBounds(Vector2 minPt, Vector2 maxPt, Vector2 givenPt)
    {
        Vector2 effectiveMin = new Vector2(Mathf.Min(minPt.x, maxPt.x), Mathf.Min(minPt.y, maxPt.y));
        Vector2 effectiveMax = new Vector2(Mathf.Max(minPt.x, maxPt.x), Mathf.Max(minPt.y, maxPt.y));

        return effectiveMin.x <= givenPt.x && givenPt.x <= effectiveMax.x &&
               effectiveMin.y <= givenPt.y && givenPt.y <= effectiveMax.y;
    }

    // Bresenham's Line Algorithm: diag, horz and vert and every angle between
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

    /*
    get the positions between the start and end positions, excluding the start and end positions. Must specify the MovementType
    */
    public static HashSet<Vector2Int> GetIntermediatePoints(Vector2Int start, Vector2Int end, MovementType type)
    {
        return type switch{
            MovementType.Diagonal=> GetIntermediateDiagonalLinePoints(start, end),
            MovementType.NonDiagonal=> GetIntermediateNonDiagonalLinePoints(start, end),
            MovementType.Any=> GetIntermediateLinePoints(start, end),
            _=> new HashSet<Vector2Int>() // Return an empty set for unsupported types
        };
    }

    /*
    get the positions in the diagonal between the start and end positions if there are any. Optionally specify to include the start and end positions
    Can be used to check if a point is on a diagonal path
    */
    public static HashSet<Vector2Int> GetIntermediateDiagonalLinePoints(Vector2Int start, Vector2Int end, bool includeEnds=false)
    {
        HashSet<Vector2Int> points = new HashSet<Vector2Int>();
        int dx = end.x - start.x;
        int dy = end.y - start.y;

        if (Mathf.Abs(dx) != Mathf.Abs(dy)) return points; // Only valid for diagonal moves

        int stepX = (dx > 0) ? 1 : -1;
        int stepY = (dy > 0) ? 1 : -1;
        int steps = Mathf.Abs(dx);

        for (int i = 1; i < steps; i++)
            points.Add(new Vector2Int(start.x + i * stepX, start.y + i * stepY));
        
        if(includeEnds){
            points.Add(start);
            points.Add(end);
        }

        return points;
    }

    /*
    get the positions in the non-diagonal(horizontal or vertical) between the start and end positions if there are any. Optionally specify to include the start and end positions
    Can be used to check if a point is on a non-diagonal path
    */
    public static HashSet<Vector2Int> GetIntermediateNonDiagonalLinePoints(Vector2Int start, Vector2Int end, bool includeEnds=false)
    {
        HashSet<Vector2Int> points = new HashSet<Vector2Int>();
        int dx = end.x - start.x;
        int dy = end.y - start.y;

        if (dx == 0) // Vertical move
        {
            int stepY = (dy > 0) ? 1 : -1;
            for (int i = 1; i < Mathf.Abs(dy); i++)
                points.Add(new Vector2Int(start.x, start.y + i * stepY));
            
            if(includeEnds){
                points.Add(start);
                points.Add(end);
            }

        }
        else if (dy == 0) // Horizontal move
        {
            int stepX = (dx > 0) ? 1 : -1;
            for (int i = 1; i < Mathf.Abs(dx); i++)
                points.Add(new Vector2Int(start.x + i * stepX, start.y));
            
            if(includeEnds){
                points.Add(start);
                points.Add(end);
            }
        }

        return points;
    }

    /*
    get the positions in the diagonal or non-diagonal between the start and end positions if there are any. Optionally specify to include the start and end positions
    Can be used to check if a point is on a diagonal or non diagonal path
    */
    public static HashSet<Vector2Int> GetIntermediateLinePoints(Vector2Int start, Vector2Int end, bool includeEnds=false)
    {
        HashSet<Vector2Int> points = new HashSet<Vector2Int>();
        
        int dx = end.x - start.x;
        int dy = end.y - start.y;

        if (Mathf.Abs(dx) == Mathf.Abs(dy)) // Diagonal move
            points = GetIntermediateDiagonalLinePoints(start, end, includeEnds);
        else if (dx == 0 || dy == 0) // Vertical or horizontal move
            points = GetIntermediateNonDiagonalLinePoints(start, end, includeEnds);

        return points;
    }


    /*
    get the positions touching a given position
    */
    public static HashSet<Vector2Int> GetSurroundingPoints(Vector2Int center)
    {
        HashSet<Vector2Int> surroundingPoints = new HashSet<Vector2Int>();
        Vector2Int[] offsets = {
            new Vector2Int(-1, 1), new Vector2Int(0, -1), new Vector2Int(1, 1),
            new Vector2Int(-1, 0),                        new Vector2Int(1, 0),
            new Vector2Int(-1, -1), new Vector2Int(0, 1), new Vector2Int(1, -1)
        };

        foreach (var offset in offsets)
            surroundingPoints.Add(center + offset);

        return surroundingPoints;
    }

    /*
    Get the positions a knight can move to from the given position.
    */
    public static HashSet<Vector2Int> GetKnightMoves(Vector2Int position){
        HashSet<Vector2Int> knightMoves = new HashSet<Vector2Int>();

        // All possible knight moves (2 in one direction and 1 in the other)
        Vector2Int[] knightOffsets = {
                    new Vector2Int(-1, 2),        new Vector2Int(1, 2),

            new Vector2Int(-2, 1),                          new Vector2Int(2, 1),

            new Vector2Int(-2, -1),                         new Vector2Int(2, -1),

                    new Vector2Int(-1, -2),        new Vector2Int(1, -2)
        };

        foreach (var offset in knightOffsets)
            knightMoves.Add(position + offset);

        return knightMoves;
    }



    /*
    get the positions in the area formed by the start and end points, including the start and end points
    */
    public static HashSet<Vector2Int> GetAllPointsInArea(Vector2Int start, Vector2Int end)
    {
        HashSet<Vector2Int> points = new HashSet<Vector2Int>();
        int minX = Mathf.Min(start.x, end.x);
        int maxX = Mathf.Max(start.x, end.x);
        int minY = Mathf.Min(start.y, end.y);
        int maxY = Mathf.Max(start.y, end.y);

        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                points.Add(new Vector2Int(x, y));

        return points;
    }

    // Containers    

    /*
    similar to List<T>.FindAll, given a set of value of type T, returns the values that satisfy the predicate in a set
    */
    public static HashSet<T> FindAll<T>(HashSet<T> set, Func<T, bool> predicate)
    {
        HashSet<T> result = new HashSet<T>();
        foreach (var item in set)
            if (predicate(item))
                result.Add(item);
        return result;
    }

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
