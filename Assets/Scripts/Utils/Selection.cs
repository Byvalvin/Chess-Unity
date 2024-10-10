using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class PlayerTypeUtility
{
    private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();

    public static List<string> GetPlayerOptions()
    {
        var playerOptions = new HashSet<string>(); // Using HashSet to avoid duplicates

        // Add PlayerState explicitly
        playerOptions.Add(nameof(PlayerState).Replace("State", "")); // Adding "PlayerState" for the human player

        // Get all types that are subclasses of PlayerState (exclude BotState)
        var playerTypes = _assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(PlayerState)) && t != typeof(BotState));

        foreach (var playerType in playerTypes)
        {
            // Remove "State" from the type name and add to options list
            string optionName = playerType.Name.Replace("State", "");
            playerOptions.Add(optionName);
        }

        return playerOptions.ToList(); // Convert HashSet back to List before returning
    }
}
