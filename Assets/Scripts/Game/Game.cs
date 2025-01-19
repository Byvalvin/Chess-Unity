using UnityEngine;
using System;

public class Game : MonoBehaviour
{
    private GameState gameState;
    private Board board;
    private Player[] players;

    public void StartGame(string player1Type, string player1Name, string player2Type, string player2Name, string filePath="")
    {
        gameState = new GameState(player1Type, player1Name, player2Type, player2Name);
        gameState.Initialize();

        players = new Player[2];
        InitializePlayers(player1Type, player2Type, player1Name, player2Name);

        board = gameObject.AddComponent<Board>(); // Add the Board component
        board.Initialize(gameState);

        // Additional setup as needed
    }

    void InitializePlayers(string whitePlayerTypeName, string blackPlayerTypeName, string whitePlayerName, string blackPlayerName){
        // Dynamically add the components using the Type objects
        // Convert the selected type names to Type objects
        Type whitePlayerType = Type.GetType(whitePlayerTypeName);
        Type blackPlayerType = Type.GetType(blackPlayerTypeName);
        if (whitePlayerType == null || blackPlayerType == null){
            Debug.LogError("Could not find player types!");
            return;
        }
        Player P1 = gameObject.AddComponent(whitePlayerType) as Player,
            P2 = gameObject.AddComponent(blackPlayerType) as Player;
            /*
        Debug.Log("PlayerPlayer stement" + P1+" "+(P1 is Player) + (P1 is Avenger));
        Debug.Log("PlayerPlayer stement" + P2+" "+(P2 is Player) + (P2 is Avenger));
        */

        // Ensure that P1 and P2 are not null after adding components
        if (P1 == null || P2 == null)
        {
            Debug.LogError("Failed to add player components!");
            return;
        }

        P1.State = gameState.PlayerStates[0];
        P2.State = gameState.PlayerStates[1];

        players[0] = P1;
        players[1] = P2;
    }




    void Start(){
        StartGame("Levi", "P1", "Player","P2");
    }

    void Update(){

    }
}
