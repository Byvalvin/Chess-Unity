using UnityEngine;
using System;
public class GameState
{
    
    public static event Action<Vector2Int, bool> OnPieceMoved; // update the Board UI if there is one
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
        UpdateGameState();
    }

    public void SwitchPlayer()=>currentIndex = 1-currentIndex;

    private void UpdateOccupancyBoard(PlayerState playerState)
    {
        OccupancyBoard |= playerState.OccupancyBoard; // OR operation to combine bitboards
    }

    public void UpdateBoard(){
        // update player board
        foreach (var playerState in PlayerStates)
            playerState.UpdateOccupancyBoard();
        
        OccupancyBoard = 0; // reset
        OccupancyBoard |= (PlayerStates[0].OccupancyBoard | PlayerStates[1].OccupancyBoard);
    }

    private void MoveUpdate(int finalIndex, bool isCapture){
        
        //ALERT UI for Listeners(Board) to update
        OnPieceMoved?.Invoke(BitOps.GetPosition(finalIndex), isCapture);

        Debug.Log("move invoked updated");

        // re-setup
        SwitchPlayer();
        UpdateBoard();
        UpdateGameState();
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

        MoveUpdate(index, isCapture);
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

    public ulong ValidateMoves(PieceBoard pieceboard, int index, ulong moves)
    {
        ulong filteredMoves = moves;
        // Condition 1: If in double-check, only allow king's moves
        if (PlayerStates[currentIndex].DoubleCheck){
            return pieceboard.Type == 'K' ? moves : 0UL; // Only allow moves if the piece is the king
        }
        PlayerState currPlayer = PlayerStates[currentIndex];
        PlayerState otherPlayer = PlayerStates[1 - currentIndex];
        int kingIndex = PlayerStates[currentIndex].GetKingIndex();

        // condition 2: checking, by KingAttacker
        int kingAttackerIndex = currPlayer.KingAttacker;

        if (kingAttackerIndex != -1)
        {
            ulong attackerPosition = BitOps.a1 << kingAttackerIndex;

            // Check if the attacker is a Queen, Rook, or Bishop
            if (otherPlayer.PieceBoards.TryGetValue('Q', out var queenBoard) && 
                (queenBoard.Bitboard & attackerPosition) != 0 ||
                otherPlayer.PieceBoards.TryGetValue('R', out var rookBoard) && 
                (rookBoard.Bitboard & attackerPosition) != 0 ||
                otherPlayer.PieceBoards.TryGetValue('B', out var bishopBoard) && 
                (bishopBoard.Bitboard & attackerPosition) != 0)
            {
                // Get the direction of the attack
                ulong path = BitOps.GetDirection(kingAttackerIndex, kingIndex);

                // Only allow moves that block the attack or capture the attacker
                filteredMoves &= (path | attackerPosition); // Keep only moves along the path or capturing the attacker
            }
            else if (otherPlayer.PieceBoards.TryGetValue('N', out var knightBoard) &&
                    (knightBoard.Bitboard & attackerPosition) != 0)
            {
                // For knights, they can only capture
                filteredMoves &= attackerPosition; // Only allow capturing the knight
            }
        }
        

        // condition 3: pinned pieces


        return moves; // Return the filtered moves
    }


    public ulong GetMovesAllowed(PieceBoard pieceBoard, int index){
        // include enpassant for validation
        ulong moves = pieceBoard.ValidMovesMap[index];
        return ValidateMoves(pieceBoard, index, moves);
    }

    private void Opposition(){
        int p1KingIndex = PlayerStates[0].GetKingIndex(),
            p2KingIndex = PlayerStates[1].GetKingIndex();
        Debug.Log(p1KingIndex+"is king index1");
        Debug.Log(p2KingIndex+"is king index2");
        ulong opposition = PlayerStates[0].PieceBoards['K'].ValidMovesMap[p1KingIndex]
                        & PlayerStates[1].PieceBoards['K'].ValidMovesMap[p2KingIndex];

        PlayerStates[0].PieceBoards['K'].ValidMovesMap[p1KingIndex] &= ~opposition;
        PlayerStates[1].PieceBoards['K'].ValidMovesMap[p2KingIndex] &= ~opposition;
    }

    private void UpdateKingAttack(){
        for(int pi=0; pi<2; pi++){
            PlayerState otherPlayer=PlayerStates[1-pi],
                        currPlayer=PlayerStates[pi];

            int kingIndex = currPlayer.GetKingIndex();
            Debug.Log(kingIndex+"is king index");
            if(kingIndex==-1){
                Debug.LogError(currPlayer+"has no King!");
                return;
            }

            ulong kingMoves = currPlayer.PieceBoards['K'].ValidMovesMap[kingIndex];
            foreach (var kvpPiece in otherPlayer.PieceBoards){
                if(kvpPiece.Key=='K')continue;
                PieceBoard opponentPieceBoard = kvpPiece.Value;
                foreach (int pieceIndex in opponentPieceBoard.ValidMovesMap.Keys){
                    ulong opponentMoves = 0UL;
                    if(kvpPiece.Key=='P' && opponentPieceBoard is PawnBoard oppPawnBoard){
                        opponentMoves = oppPawnBoard.GetAttackMoves();
                    }else{
                        ulong enemyBoardExceptKingPos = currPlayer.OccupancyBoard & ~(currPlayer.PieceBoards['K'].Bitboard);
                        opponentMoves = opponentPieceBoard.GetValidMoves(otherPlayer.OccupancyBoard, pieceIndex, enemyBoardExceptKingPos, true);
                    }
                    kingMoves &= ~(opponentMoves);
                }
            }
            currPlayer.PieceBoards['K'].ValidMovesMap[kingIndex] = kingMoves;
        }

    }

    private void UpdateCheckStatus(){ // only need to do this fi rth eother player. The player who is playing after the current player
        PlayerState otherPlayer=PlayerStates[1-currentIndex],
                    currPlayer=PlayerStates[currentIndex];

        int attacker = -1;
        int attackerCount = 0;
        foreach (var kvpPlayer in currPlayer.PieceBoards) // search for attacker of otherPlayer's King
        {
            if(kvpPlayer.Key=='K')continue; //Kings cant attack Kings

            PieceBoard pieceBoard = kvpPlayer.Value;
            if(attackerCount>=2)break;

            foreach (var kvpPiece in pieceBoard.ValidMovesMap)
            {
                int potentialAttackerIndex = kvpPiece.Key;
                ulong moves = kvpPiece.Value;
                if((moves & otherPlayer.PieceBoards['K'].Bitboard)!=0){ // attack on other player king
                    attacker = potentialAttackerIndex;
                    attackerCount++;
                }
                if(attackerCount>=2){
                    break;
                }
            }
        }

        otherPlayer.InCheck = attackerCount==1;
        otherPlayer.DoubleCheck = attackerCount>1;
        if(otherPlayer.IsInCheck) otherPlayer.KingAttacker=attacker;
    }

    private void UpdateGameState(){
        for(int i=0; i<64; i++){
            ulong currBitPos = (BitOps.a1<<i);
            
            int playerIndex; //find correct playerstate
            playerIndex = (currBitPos & PlayerStates[0].OccupancyBoard)!=0 ? 0 
                        : (currBitPos & PlayerStates[1].OccupancyBoard)!=0 ? 1
                        : -1;
            PlayerState currPlayerState = playerIndex!=-1 ? PlayerStates[playerIndex] : null;
            if(currPlayerState!=null){ // find correct PieceBoard
                PieceBoard currPieceBoard = null;
                foreach (PieceBoard pieceBoard in currPlayerState.PieceBoards.Values){
                    if((pieceBoard.Bitboard & currBitPos)!=0){
                        currPieceBoard = pieceBoard;
                        break;
                    }
                }
                if(currPieceBoard!=null){
                    //Debug.Log(currPlayerState + " " + i + " " +currPieceBoard);
                    ulong enemyBoardExceptKingPos = PlayerStates[1-playerIndex].OccupancyBoard & ~(PlayerStates[1-playerIndex].PieceBoards['K'].Bitboard);
                    currPieceBoard.ResetValidMoves(currPlayerState.OccupancyBoard, i, enemyBoardExceptKingPos);
                }
            }
        }
        Opposition();
        UpdateCheckStatus();
        UpdateKingAttack();
    }


}
