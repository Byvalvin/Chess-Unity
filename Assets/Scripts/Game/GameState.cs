using UnityEngine;
using System;
using System.Collections.Generic;

public class GameState
{
    
    public static event Action<Vector2Int, bool, Vector2Int, bool, Vector2Int, bool> OnPieceMoved; // update the Board UI if there is one
    public PlayerState[] PlayerStates { get; private set; } // Array of player states
    public ulong OccupancyBoard { get; private set; } // Combined occupancy board
    public int currentIndex = 0; // white to start

    public char PromoteTo{get; set;}
    public bool Gameover{get; private set;}

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

        Initialize();
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

    public bool hasMoves(PlayerState player){
        foreach (PieceBoard pieceBoard in player.PieceBoards.Values)
            foreach (int pieceIndex in pieceBoard.ValidMovesMap.Keys)
                if(GetMovesAllowed(pieceBoard, pieceIndex)!=0UL)
                    return true;
        return false;   
    }
    public ulong GetMovesAllowed(PieceBoard pieceBoard, int index){
        // include enpassant for validation
        ulong moves = pieceBoard.ValidMovesMap[index];
        return ValidateMoves(pieceBoard, index, moves);
    }

    public ulong GetAllAttackMoves(PlayerState player){
        ulong opponentMoves = 0UL;
        PlayerState otherPlayer = PlayerStates[1-player.TurnIndex]; //the other player
        foreach (var kvpPiece in player.PieceBoards){
            if(kvpPiece.Key=='K')continue;
            PieceBoard opponentPieceBoard = kvpPiece.Value;
            if(kvpPiece.Key=='P' && opponentPieceBoard is PawnBoard oppPawnBoard){
                opponentMoves |= oppPawnBoard.GetAttackMoves();
            }else{
                ulong enemyBoardExceptKingPos = otherPlayer.OccupancyBoard & ~(otherPlayer.PieceBoards['K'].Bitboard);
                foreach (int pieceIndex in opponentPieceBoard.ValidMovesMap.Keys)
                    opponentMoves |= opponentPieceBoard.GetValidMoves(player.OccupancyBoard, pieceIndex, enemyBoardExceptKingPos, true);
            }
        }
        return opponentMoves;
    }


    private void MoveUpdate(int finalIndex, bool isCapture=false, int capturedPosition=-1, bool isCastle=false, int castledRookPosition=-1, bool isPromotion=false){
        
        //ALERT UI for Listeners(Board) to update
        OnPieceMoved?.Invoke(BitOps.GetPosition(finalIndex),
            isCapture, BitOps.GetPosition(capturedPosition),
            isCastle, BitOps.GetPosition(castledRookPosition),
            isPromotion
        );

        Debug.Log("move invoked updated");
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

    public void ExecuteMove(PieceBoard pieceBoard, int originalIndex, int index){
        // Check if the target index is occupied
        Vector2Int originalPosition = BitOps.GetPosition(originalIndex);
        bool isCapture = (OccupancyBoard & (BitOps.a1 << index)) != 0,
            isEnPassantCapture = pieceBoard is PawnBoard pawnBoard 
                            && PlayerStates[1 - currentIndex].PieceBoards['P'] is PawnBoard oppPawnBoard
                            && Math.Abs(index - oppPawnBoard.enPassantablePawn)==Board.N
                            && Math.Abs(originalPosition.x - BitOps.GetPosition(oppPawnBoard.enPassantablePawn).x)==1
                            && Math.Abs(originalPosition.y - BitOps.GetPosition(oppPawnBoard.enPassantablePawn).y)==0
                            && oppPawnBoard.canBeCapturedEnPassant,
            
            isCastle = pieceBoard is KingBoard kingBoard
                        && Math.Abs(originalIndex-index)==2;
       

        int removedPieceIndex = -1; // no removed piece
        if (isCapture) // already valifdated move by now
        {
            // Remove the piece from the opponent's board
            PieceBoard opponentPieceBoard = GetPieceBoard(index, PlayerStates[1 - currentIndex]);
            PlayerStates[1-currentIndex].RemovePiece(opponentPieceBoard, index);
            Debug.Log($"Captured opponent's piece at index {index}.");
            removedPieceIndex = index;
        }
        else if(isEnPassantCapture)
        {
            PlayerStates[1-currentIndex].EnPassantChosen();
            Debug.Log($"Captured opponent's piece enpassant {index}.");
            removedPieceIndex = (PlayerStates[1-currentIndex].PieceBoards['P'] as PawnBoard).enPassantablePawn;
        }

        if(IsPromotion(pieceBoard, index)){
            if(PromoteTo!='\0'){
                Debug.Log("Promotion!"+PromoteTo);
                // remove piece from this pawn pieceBoard
                pieceBoard.RemovePiece(index);
                // update pieceBoard 

                // add to selected pieceboard
                PlayerStates[currentIndex].PieceBoards[PromoteTo].AddPiece(index);
                // update pieceBpard
                // create piece ui

                MoveUpdate(index,isPromotion:true);
            }

        }else{
            pieceBoard.Move(originalIndex, index);
            MoveUpdate(index, isCapture || isEnPassantCapture, removedPieceIndex);

            if(isCastle){// also find and move the correct rook
                int rookOriginalPosition, rookFinalPosition;
                if(PlayerStates[currentIndex].IsWhite){
                    rookOriginalPosition = index==2 ? 0:7;
                    rookFinalPosition = index==2 ? 3:5;
                }else{
                    rookOriginalPosition = index==58 ? 56:63;
                    rookFinalPosition = index==58 ? 59:61;
                }
                PlayerStates[currentIndex].PieceBoards['R'].Move(rookOriginalPosition, rookFinalPosition);
                MoveUpdate(rookFinalPosition,
                    isCapture:false, removedPieceIndex,
                    isCastle:true, castledRookPosition:rookOriginalPosition
                );

            }
        }

        // re-setup
        (PlayerStates[1 - currentIndex].PieceBoards['P'] as PawnBoard).EnPassantReset(); // reset enpassant after a move is made a poor fix but works for now
        PromoteTo='\0';
        PlayerStates[currentIndex].PromoteTo='\0';

        SwitchPlayer();
        UpdateBoard();
        UpdateGameState();
        Gameover = IsGameEnd();
    }

    
    public ulong ValidateMoves(PieceBoard pieceboard, int index, ulong moves)
    {
        ulong filteredMoves = moves;
        bool isKing = pieceboard.Type == 'K';
        PlayerState currPlayer = PlayerStates[currentIndex];

        // Condition 1: If in double-check, only allow king's moves
        if (currPlayer.DoubleCheck)
        {
            return  isKing ? moves : 0UL;
        }

        
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
            }else if(isKing){ //Handle castling
                // remove all castling moves since king in check
                filteredMoves &= ~((pieceboard as KingBoard).GetCastlingMoves(OccupancyBoard, currPlayer.OccupancyBoard));
                return filteredMoves;
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
        }else{ //is King,  hnadle castling.
            ulong KingSideSquares = currPlayer.IsWhite? KingBoard.WhiteKingsideMask : KingBoard.BlackKingsideMask,
                QueenSideSquares = currPlayer.IsWhite? KingBoard.WhiteQueensideMask : KingBoard.BlackQueensideMask;
            ulong KingSideMove = currPlayer.IsWhite? KingBoard.WhiteKingSideMove : KingBoard.BlackKingSideMove,
                QueenSideMove = currPlayer.IsWhite? KingBoard.WhiteQueenSideMove : KingBoard.BlackQueenSideMove;
            
            //Make sure rook exists
            //(currPlayer.PieceBoards['R'].Bitboard & BitOps.a1<<0) != 0;
            //make sure it is rooks first move (//Make sure rook exists)
            //currPlayer.PieceBoards['R'].FirstMovers.Contains(0);
            //make sure no opp piece atatcks spaces between king abd rook

            ulong allAttackedSquares = GetAllAttackMoves(PlayerStates[1-currentIndex]);
            // check KingSide
            if(!(currPlayer.PieceBoards['R'].FirstMovers.Contains(currPlayer.IsWhite? 7:63))
            ||((KingSideSquares & allAttackedSquares) != 0)
            ){
                filteredMoves &= ~(KingSideMove); // remove the castle move
            }

            // check QueenSide
            if(!(currPlayer.PieceBoards['R'].FirstMovers.Contains(currPlayer.IsWhite? 0:56))
            ||((QueenSideSquares & allAttackedSquares) != 0)
            ){
                filteredMoves &= ~(QueenSideMove); // remove the castle move
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
                        ulong piecePosition=BitOps.a1 << pieceIndex,
                            attackerPosition=BitOps.a1 << kvpMoves.Key;

                        bool otherPieceOnPath = (OccupancyBoard & (pinnedMovement & ~(attackerPosition) & ~(piecePosition)))!=0;
                        if(otherPieceOnPath){
                            pinnedMovement=0UL; // reset, continue the search for a pinn
                        }else{
                            pinnedMovement |= attackerPosition;
                            return (pinnedMovement&piecePosition)!=0; // if piece is indeed pinned
                        }
                        
                    }
                }
            }
            
        }
        
        return false; // No attacker found
    }



    private void ResetEnpassant(PlayerState playerState) => (playerState.PieceBoards['P'] as PawnBoard).ResetEnPassant();
    
    private void Opposition(){
        int p1KingIndex = PlayerStates[0].GetKingIndex(),
            p2KingIndex = PlayerStates[1].GetKingIndex();
        // Debug.Log(p1KingIndex+"is king index1");
        // Debug.Log(p2KingIndex+"is king index2");
        ulong opposition = PlayerStates[0].PieceBoards['K'].ValidMovesMap[p1KingIndex]
                        & PlayerStates[1].PieceBoards['K'].ValidMovesMap[p2KingIndex];

        PlayerStates[0].PieceBoards['K'].ValidMovesMap[p1KingIndex] &= ~opposition;
        PlayerStates[1].PieceBoards['K'].ValidMovesMap[p2KingIndex] &= ~opposition;
    }

    private void UpdateKingAttack(PlayerState playerState){  
        PlayerState otherPlayer=PlayerStates[1-playerState.TurnIndex],
                    currPlayer=playerState;

        int kingIndex = currPlayer.GetKingIndex();
        // Debug.Log(kingIndex+"is king index");
        if(kingIndex==-1){
            // Debug.LogError(currPlayer+"has no King!");
            return;
        }

        ulong kingMoves = currPlayer.PieceBoards['K'].ValidMovesMap[kingIndex];
 
        kingMoves &= ~(GetAllAttackMoves(otherPlayer));  
        
        currPlayer.PieceBoards['K'].ValidMovesMap[kingIndex] = kingMoves;
    }

    private void UpdateCheckStatus(PlayerState playerState){ // only need to do this fi rth eother player. The player who is playing after the current player
        PlayerState otherPlayer=PlayerStates[1-playerState.TurnIndex],
                    currPlayer=playerState;

        int attacker = -1;
        int attackerCount = 0;

        // do pawns first
        PawnBoard pawnBoard = currPlayer.PieceBoards['P'] as PawnBoard;
        foreach (var kvpPiece in pawnBoard.ValidMovesMap){ // Pawns attack differently
            int potentialAttackerIndex = kvpPiece.Key;
            ulong moves = pawnBoard.GetAttackMove(potentialAttackerIndex);
            if((moves & otherPlayer.PieceBoards['K'].Bitboard)!=0){ // attack on other player king
                attacker = potentialAttackerIndex;
                attackerCount++;
            }
        }

        foreach (var kvpPlayer in currPlayer.PieceBoards) // search for attacker of otherPlayer's King
        {
            if(attackerCount>=2) break;

            if(kvpPlayer.Key=='K' || kvpPlayer.Key=='P') continue; //Kings cant attack Kings, already did pawns

            PieceBoard pieceBoard = kvpPlayer.Value;
            foreach (var kvpPiece in pieceBoard.ValidMovesMap)
            {
                int potentialAttackerIndex = kvpPiece.Key;
                ulong moves = kvpPiece.Value;
                if((moves & otherPlayer.PieceBoards['K'].Bitboard)!=0){ // attack on other player king
                    attacker = potentialAttackerIndex;
                    attackerCount++;
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
            
            int playerIndex = (currBitPos & PlayerStates[0].OccupancyBoard)!=0 ? 0 
                            : (currBitPos & PlayerStates[1].OccupancyBoard)!=0 ? 1
                            : -1;
            
            if(playerIndex==-1) continue;

            PlayerState currPlayerState = PlayerStates[playerIndex],
                        otherPlayerState = PlayerStates[1-playerIndex];
         
            PieceBoard currPieceBoard = null; // find correct PieceBoard
            foreach (PieceBoard pieceBoard in currPlayerState.PieceBoards.Values)
                if((pieceBoard.Bitboard & currBitPos)!=0){
                    currPieceBoard = pieceBoard;
                    break;
                }
            if(currPieceBoard!=null){
                ulong enemyBoardExceptKingPos = otherPlayerState.OccupancyBoard & ~(otherPlayerState.PieceBoards['K'].Bitboard);
                currPieceBoard.ResetValidMoves(currPlayerState.OccupancyBoard, i, currPieceBoard.Type=='P'?  otherPlayerState.OccupancyBoard:enemyBoardExceptKingPos);
            }
        }
        if(PlayerStates[1-currentIndex].PieceBoards['P'] is PawnBoard oppPawnBoard && oppPawnBoard.canBeCapturedEnPassant)// add only when opp presents the chance once
            (PlayerStates[currentIndex].PieceBoards['P'] as PawnBoard).AddEnPassant(oppPawnBoard);
        // Debug.Log((PlayerStates[1].PieceBoards['P'] is PawnBoard oppPawnBoard2 && oppPawnBoard2.canBeCapturedEnPassant)+ " " +(PlayerStates[1].PieceBoards['P'] as PawnBoard).enPassantablePawn+" "+(PlayerStates[1].PieceBoards['P'] as PawnBoard).enPassantCounter);
        Opposition();
        foreach (PlayerState playerState in PlayerStates)
        {
            ResetEnpassant(playerState);
            UpdateCheckStatus(playerState);
            UpdateKingAttack(playerState);
        }

    }

    // prootion
    public static bool IsPromotion(PieceBoard pieceBoard, int targetIndex)
        => pieceBoard is PawnBoard 
            && (pieceBoard.IsWhite?(56<=targetIndex&&targetIndex<=63):(0<=targetIndex&&targetIndex<=7));

    // Game end
    public bool PlayerCheckmated(PlayerState player){ // ends when a player is in double check and cant move the king OR a player is in check and cant evade, capture attacker or block check path
        if(!hasMoves(player)&& currentIndex==player.TurnIndex && player.IsInCheck){
            Debug.Log($"GAME OVER:{player.PlayerType} IS CHECKMATED");
            return true;
        }
        return false;
    }
    public bool CheckCheckmate()=> PlayerCheckmated(PlayerStates[0]) || PlayerCheckmated(PlayerStates[1]);

    public bool PlayerStalemated(PlayerState player){
        if(!hasMoves(player) && currentIndex==player.TurnIndex && !player.IsInCheck){
            Debug.Log($"GAME OVER: DRAW-> {player.PlayerType} STALEMATED");
            return true;
        }
        return false;
    }
    public bool CheckStalemate()=> PlayerStalemated(PlayerStates[0]) || PlayerStalemated(PlayerStates[1]);
    public bool IsGameEnd()=>CheckCheckmate() || CheckStalemate();


}
