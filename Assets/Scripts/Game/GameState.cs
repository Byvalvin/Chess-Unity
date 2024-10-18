public class GameState
{
    public PlayerState[] PlayerStates { get; private set; } // Array of player states
    public ulong OccupancyBoard { get; private set; } // Combined occupancy board

    public GameState(string player1Type, string player2Type)
    {
        PlayerStates = new PlayerState[2];
        PlayerStates[0] = new PlayerState(player1Type.Trim(), true);  // First player is white
        PlayerStates[1] = new PlayerState(player2Type.Trim(), false); // Second player is black
        OccupancyBoard = 0; // Initialize occupancy board
    }

    public void Initialize()
    {
        foreach (var playerState in PlayerStates)
        {
            playerState.InitializePieces(); // Call to initialize pieces
            UpdateOccupancyBoard(playerState);
        }
    }

    private void UpdateOccupancyBoard(PlayerState playerState)
    {
        // Combine the player's piece boards into the occupancy board
        foreach (var pieceBoard in playerState.PieceBoards.Values) // Access the values of the dictionary
        {
            OccupancyBoard |= pieceBoard.Bitboard; // OR operation to combine bitboards
        }
    }
}
