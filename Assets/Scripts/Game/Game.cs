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

        players = new Player[2];
        players[0] = new Player(gameState.PlayerStates[0]);
        players[1] = new Player(gameState.PlayerStates[1]);

        board = gameObject.AddComponent<Board>(); // Add the Board component
        board.Initialize(gameState);

        // Additional setup as needed
    }




    void Start(){
        StartGame("P1", "P2");
    }

    void Update(){

    }
}
