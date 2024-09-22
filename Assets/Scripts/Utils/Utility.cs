// Scripts/Utils/Utility.cs
using UnityEngine;

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
    public static Vector2 RoundVector2(Vector2 position)=>new Vector2(Mathf.Round(position.x), Mathf.Round(position.y));

    public static bool InBounds(Vector2 minPt, Vector2 maxPt, Vector2 givenPt)
    {
        Vector2 effectiveMin = new Vector2(Mathf.Min(minPt.x, maxPt.x), Mathf.Min(minPt.y, maxPt.y));
        Vector2 effectiveMax = new Vector2(Mathf.Max(minPt.x, maxPt.x), Mathf.Max(minPt.y, maxPt.y));

        return effectiveMin.x <= givenPt.x && givenPt.x <= effectiveMax.x &&
               effectiveMin.y <= givenPt.y && givenPt.y <= effectiveMax.y;
    }


}
