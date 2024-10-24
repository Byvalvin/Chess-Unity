using UnityEngine;
using System;
using System.Collections.Generic;

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
        bool isKing = pieceboard.Type == 'K';
        // Condition 1: If in double-check, only allow king's moves
        if (PlayerStates[currentIndex].DoubleCheck)
        {
            return  isKing ? moves : 0UL;
        }

        PlayerState currPlayer = PlayerStates[currentIndex];
        int kingIndex = currPlayer.GetKingIndex();
        int kingAttackerIndex = currPlayer.KingAttacker;

        // Condition 2: Check for king attacker
        if(PlayerStates[currentIndex].InCheck){
            if (kingAttackerIndex != -1 && !isKing)
            {
                ulong attackerPosition = BitOps.a1 << kingAttackerIndex;
                ulong path = GetAttackPath(kingAttackerIndex, kingIndex, PlayerStates[1 - currentIndex]);

                // Only allow moves that block the attack or capture the attacker
                filteredMoves &= (path | attackerPosition);
            }
        }
        

        // Condition 3: (Handle pinned pieces if needed)
        if(!isKing){
            ulong pinPathAndAttackerPosition;
            bool isPinned = CheckForPinnedPiece(pieceboard, index, kingIndex, PlayerStates[1 - currentIndex], out pinPathAndAttackerPosition);
            
            if (isPinned)
            {
                // Restrict moves to either capturing the attacker or moving back to the king
                filteredMoves &= (pinPathAndAttackerPosition);
            }
        }

        return filteredMoves; // Return the filtered moves
    }

    private ulong GetAttackPath(int kingAttackerIndex, int kingIndex, PlayerState otherPlayer)
    {
        ulong attackerPosition = BitOps.a1 << kingAttackerIndex;
        
        // Check for each type of piece that can attack
        foreach (char pieceType in new[] { 'Q', 'R', 'B' })
        {
            if (otherPlayer.PieceBoards.TryGetValue(pieceType, out var board) && 
                (board.Bitboard & attackerPosition) != 0)
            {
                return BitOps.GetPathMask(kingAttackerIndex, kingIndex, 
                    pieceType == 'Q' ? BitOps.MovementType.Any :
                    pieceType == 'R' ? BitOps.MovementType.Rook :
                    BitOps.MovementType.Bishop);
            }
        }

        return 0UL; // No valid attacker found
    }

    private bool IsLineValid(int pieceIndex, int kingIndex, char pieceType)
    {
        Vector2Int pieceColRow=BitOps.GetPosition(pieceIndex), kingColRow=BitOps.GetPosition(kingIndex);
        int pieceRow=pieceColRow.y, pieceCol=pieceColRow.x,
            kingRow=kingColRow.y, kingCol=kingColRow.x;

        if (pieceType == 'Q')
        {
            // Queen can attack in straight lines (horizontal, vertical) or diagonally
            return BitOps.isValidHorizontalMove(pieceRow, pieceCol, kingRow, kingCol) 
                || BitOps.isValidVerticalMove(pieceRow, pieceCol, kingRow, kingCol) 
                || BitOps.isValidDiagonalMove(pieceRow, pieceCol, kingRow, kingCol);
        }
        else if (pieceType == 'R')
        {
            // Rook can only attack in straight lines (horizontal or vertical)
            return BitOps.isValidHorizontalMove(pieceRow, pieceCol, kingRow, kingCol)
                || BitOps.isValidVerticalMove(pieceRow, pieceCol, kingRow, kingCol);
        }
        else if (pieceType == 'B')
        {
            // Bishop can only attack diagonally
            return BitOps.isValidDiagonalMove(pieceRow, pieceCol, kingRow, kingCol);
        }

        return false; // Invalid piece type
    }

    private bool CheckForPinnedPiece(PieceBoard pieceboard, int pieceIndex, int kingIndex, PlayerState otherPlayer, out ulong pinnedMovement){
        pinnedMovement = 0UL;

        foreach (var kvpPiece in otherPlayer.PieceBoards){
            if(new HashSet<char>{ 'Q', 'R', 'B' }.Contains(kvpPiece.Key)){ // only sliders
                foreach (var kvpMoves in kvpPiece.Value.ValidMovesMap){
                    if(IsLineValid(kvpMoves.Key, kingIndex, kvpPiece.Key)){ // if there is a path
                        pinnedMovement=GetAttackPath(kvpMoves.Key, kingIndex, otherPlayer); //get the path
                        ulong piecePosition=BitOps.a1 << pieceIndex;
                        bool otherPieceOnPath = (OccupancyBoard & (pinnedMovement & ~(BitOps.a1<<kvpMoves.Key) & ~(piecePosition)))!=0;
                        if(otherPieceOnPath){
                            pinnedMovement=0UL; // reset, continue the search for a pinn
                        }else{
                            return (pinnedMovement&piecePosition)!=0; // if piece is indeed pinned
                        }
                        
                    }
                }
            }
            
        }
        
        return false; // No attacker found
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

    private void UpdateKingAttack(PlayerState playerState){
        
        PlayerState otherPlayer=PlayerStates[1-playerState.TurnIndex],
                    currPlayer=playerState;

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

    private void UpdateCheckStatus(PlayerState playerState){ // only need to do this fi rth eother player. The player who is playing after the current player
        PlayerState otherPlayer=PlayerStates[1-playerState.TurnIndex],
                    currPlayer=playerState;

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

        Debug.Log(otherPlayer.PlayerType +" is in check?: "+otherPlayer.InCheck + " "+ otherPlayer.DoubleCheck + " " +otherPlayer.IsInCheck + "by attackker at " + attacker);
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
                    /*
                    if(currPieceBoard.Type=='N'){
                        Debug.Log("This is a Knight"+currPlayerState.IsWhite);
                        foreach (var item in currPieceBoard.ValidMovesMap){
                            Debug.Log(item.Key + " " + item.Value);
                        }
                    }
                    */
                }
            }
        }
        Opposition();
        foreach (PlayerState playerState in PlayerStates)
        {
            UpdateCheckStatus(playerState);
            UpdateKingAttack(playerState);
        }

    }


}
