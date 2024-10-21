using UnityEngine;
using System;
public class GameState
{
    
    public static event Action<Vector2Int> OnPieceMoved; // update the Board UI if there is one
    public PlayerState[] PlayerStates { get; private set; } // Array of player states
    public ulong OccupancyBoard { get; private set; } // Combined occupancy board
    public int currentIndex = 0; // white to start

    public GameState(string player1Type, string player2Type)
    {
        PlayerStates = new PlayerState[2];
        PlayerStates[0] = new PlayerState(player1Type.Trim(), true);  // First player is white
        PlayerStates[1] = new PlayerState(player2Type.Trim(), false); // Second player is black
        OccupancyBoard = 0; // Initialize occupancy board

    }
    public GameState(GameState original){
        PlayerStates[0] = original.PlayerStates[0].Clone();
        PlayerStates[1] = original.PlayerStates[1].Clone();
        currentIndex = original.currentIndex;
    }
    public GameState Clone() => new GameState(this);

    public void Initialize()
    {
        UpdateBoard();
    }

    public void SwitchPlayer()=>currentIndex = 1-currentIndex;

    private void UpdateOccupancyBoard(PlayerState playerState)
    {
        foreach (var pieceBoard in playerState.PieceBoards.Values) // Access the values of the dictionary
            OccupancyBoard |= pieceBoard.Bitboard; // OR operation to combine bitboards
    }

    public void UpdateBoard(){
        OccupancyBoard = 0; // reset
        foreach(PlayerState playerState in PlayerStates){
            foreach(PieceBoard pieceBoard in playerState.PieceBoards.Values)
                OccupancyBoard |= pieceBoard.Bitboard;
        }
    }

    private void MoveUpdate(int finalIndex){
        
        //ALERT UI for Listeners(Board) to update
        OnPieceMoved?.Invoke(BitOps.GetPosition(finalIndex));

        Debug.Log("move invoked updated");
        SwitchPlayer();
        UpdateBoard();
    }

    public void ExecuteMove(PieceBoard pieceBoard, int originalIndex, int index){

        // Check if the target index is occupied
        bool isCapture = (OccupancyBoard & (BitOps.a1 << index)) != 0;

        if (isCapture) // already valifdated move by now
        {
            // Remove the piece from the opponent's board
            PieceBoard opponentPieceBoard = GetPieceBoard(index, PlayerStates[1 - currentIndex]);
            PlayerStates[1-currentIndex].RemovePiece(opponentPieceBoard, index);
            Debug.Log($"Captured opponent's piece at index {index}.");
        }

        pieceBoard.Move(originalIndex, index);
        MoveUpdate(index);
    }

    public PieceBoard GetPieceBoard(int index, PlayerState givenPlayerState =null){
        // Determine the color of the piece at the target index
        PieceBoard opponentPieceBoard = null;

        if(givenPlayerState!=null){
            foreach (var board in givenPlayerState.PieceBoards.Values){
                if ((board.Bitboard & (BitOps.a1 << index)) != 0){
                    opponentPieceBoard = board;
                    break;
                }
            }
            
        }else{ // Check each player's pieces 
            foreach (var playerState in PlayerStates){
                foreach (var board in playerState.PieceBoards.Values){
                    if ((board.Bitboard & (BitOps.a1 << index)) != 0) {
                        opponentPieceBoard = board;
                        break;
                    }
                }
                if (opponentPieceBoard != null) break;
            }
        }
        return opponentPieceBoard;

    }


}
