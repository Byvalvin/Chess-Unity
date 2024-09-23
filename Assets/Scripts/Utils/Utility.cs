// Scripts/Utils/Utility.cs
using UnityEngine;
using System.Collections.Generic;

// Utility class dont maintain a state so can call static functions globally and freely across project + static prob quicker

public static class Utility
{
    // Player UI variables
    public static Vector2 GetMouseWorldPosition()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(mousePos.x, mousePos.y);
    }
    public static bool MouseUp() => Input.GetMouseButtonUp(0);
    public static bool MouseDown() => Input.GetMouseButtonDown(0);


    // Vector math
    public static Vector2Int RoundVector2(Vector2 position)=>new Vector2Int((int)Mathf.Round(position.x), (int)Mathf.Round(position.y));

    public static bool InBounds(Vector2 minPt, Vector2 maxPt, Vector2 givenPt)
    {
        Vector2 effectiveMin = new Vector2(Mathf.Min(minPt.x, maxPt.x), Mathf.Min(minPt.y, maxPt.y));
        Vector2 effectiveMax = new Vector2(Mathf.Max(minPt.x, maxPt.x), Mathf.Max(minPt.y, maxPt.y));

        return effectiveMin.x <= givenPt.x && givenPt.x <= effectiveMax.x &&
               effectiveMin.y <= givenPt.y && givenPt.y <= effectiveMax.y;
    }

    //Bresenham's Line Algorithm. This algorithm efficiently finds all the points on a straight line between two points in a grid.
    public static List<Vector2Int> GetLinePoints(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> points = new List<Vector2Int>();

        int dx = Mathf.Abs(end.x - start.x);
        int dy = Mathf.Abs(end.y - start.y);
        int sx = (start.x < end.x) ? 1 : -1;
        int sy = (start.y < end.y) ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            Vector2Int point = new Vector2Int(start.x, start.y);
            if(!points.Contains(point)) points.Add(point); // Add current point

            if (start.x == end.x && start.y == end.y)
                break;

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
    public static List<Vector2Int> GetIntermediatePoints(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> points = new List<Vector2Int>();

        // Calculate the differences
        int dx = end.x - start.x;
        int dy = end.y - start.y;

        // Determine the number of steps needed
        int steps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));

        for (int i = 1; i < steps; i++) // Start from 1 to steps - 1
        {
            float t = (float)i / steps;
            int x = (int)Mathf.Lerp(start.x, end.x, t);
            int y = (int)Mathf.Lerp(start.y, end.y, t);
            Vector2Int point = new Vector2Int(x, y);
            if(!points.Contains(point))points.Add(point);
        }

        return points;
    }



}
