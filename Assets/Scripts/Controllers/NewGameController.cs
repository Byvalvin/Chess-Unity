using UnityEngine;

public class NewGameController : MonoBehaviour
{
    void Awake()
    {
        // Create GameObject for the Game script
        GameObject gameObject = new GameObject("Game");
        gameObject.AddComponent<Game>(); // Attach Game script

        // Create GameObject for the PlayerSelectionUI
        GameObject uiObject = new GameObject("PlayerSelectionUI");
        PlayerSelectionUI playerSelectionUI = uiObject.AddComponent<PlayerSelectionUI>();
        
        // Optionally, set the PlayerSelectionUI's canvas reference to be part of the Game
        playerSelectionUI.Game = gameObject.GetComponent<Game>();
    }
}
