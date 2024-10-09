using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class PlayerTypeUtility
{
    public static List<string> GetPlayerOptions()
    {
        List<string> playerOptions = new List<string>();

        // Get all types in the current assembly
        Assembly assembly = Assembly.GetExecutingAssembly();

        // Get all types that are subclasses of PlayerState (exclude BotState)
        var playerTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(PlayerState)) && t != typeof(BotState));

        // Add PlayerState explicitly
        playerOptions.Add(nameof(PlayerState).Replace("State", "")); // Adding "PlayerState" for the human player

        foreach (var playerType in playerTypes)
        {
            // Remove "State" from the type name and add to options list
            string optionName = playerType.Name.Replace("State", "");
            playerOptions.Add(optionName);
        }

        return playerOptions;
    }
}
