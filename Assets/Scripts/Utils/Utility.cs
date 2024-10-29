// Scripts/Utils/Utility.cs
using UnityEngine;
using System.Collections.Generic;
using System;


/// <summary>
/// Utility class providing global static methods for various calculations.
/// </summary>
public static class Utility
{

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

    public static bool InBounds(Vector2Int position, int min, int max) => min<=position.x&&position.x<max && min<=position.y&&position.y<max;

}