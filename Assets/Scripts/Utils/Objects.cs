using System;
using System.Collections.Generic;
using UnityEngine;

public static class Objects
{
    private static readonly Dictionary<string, Type> PlayerStateTypes = new Dictionary<string, Type>();

    static Objects()
    {
        // Cache player state types on initialization
        foreach (var type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
        {
            // Exclude abstract types and specifically exclude BotState
            if (type.IsSubclassOf(typeof(PlayerState)) && !type.IsAbstract && type != typeof(BotState))
            {
                PlayerStateTypes[type.Name] = type;
            }
        }

        // Add PlayerState class itself
        PlayerStateTypes[typeof(PlayerState).Name] = typeof(PlayerState);
    }

    public static PlayerState CreatePlayerState(string playerTypeName, string playerName, bool isWhite)
    {
        if (PlayerStateTypes.TryGetValue($"{playerTypeName}State", out var type))
        {
            return (PlayerState)Activator.CreateInstance(type, playerName, isWhite);
        }

        // Optional: log an error or throw an exception if the type is not found
        //Debug.LogError($"Player type '{playerTypeName}State' not found.");
        return null;
    }
}
