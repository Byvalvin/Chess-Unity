using UnityEngine;

public class Game : MonoBehaviour
{
    private GameState gameState;
    private Board board;
    private Player[] players;

    public void StartGame(string player1Type, string player2Type)
    {
        gameState = new GameState(player1Type, player2Type);
        gameState.Initialize();

        board = gameObject.AddComponent<Board>(); // Add the Board component
        board.Initialize(gameState);

        players = new Player[2];
        players[0] = new Player(gameState.PlayerStates[0]);
        players[1] = new Player(gameState.PlayerStates[1]);

        // Additional setup as needed
    }
}
