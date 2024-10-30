using System;
using System.Linq; // Add this line for LINQ

public static class Objects{
    public static PlayerState CreatePlayerState(string playerTypeName, string playerName, bool isWhite){
        // Use reflection to instantiate the appropriate player state
        var type = System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
            .FirstOrDefault(t => t.Name == $"{playerTypeName}State");
        
        if (type != null){
            PlayerState playerState = (PlayerState)Activator.CreateInstance(type, playerName, isWhite);
            //Debug.Log("Player stement" + playerState+" "+(playerState is AvengerState));
            return playerState;
        }
        return null; // Handle case where type is not found
    }
}