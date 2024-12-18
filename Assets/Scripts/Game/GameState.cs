using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text; // string builder hash

using System.Security.Cryptography; // SHA256
using System.IO;  // +MemStream

public class GameState
{
    
    public event Action<Vector2Int, bool, Vector2Int, bool, Vector2Int, bool> OnPieceMoved; // update the Board UI if there is one
    public PlayerState[] PlayerStates { get; private set; } // Array of player states
    public ulong OccupancyBoard { get; private set; } // Combined occupancy board
    public int currentIndex = 0; // white to start

    public char PromoteTo{get; set;}
    public bool Gameover{get; private set;}
    public int Winner{get; private set;}

    public int MoveCount{get; private set;}
    public Queue<string> lastThreeStates{get; private set;}
    public int NoCaptureNoPawnMoveCount{get; private set;} // Track moves without captures or pawn moves

    public GameState(
        string player1Type, string player1Name,
        string player2Type, string player2Name
    )
    {
        PlayerStates = new PlayerState[2];
   
        PlayerStates[0] = Objects.CreatePlayerState(player1Type.Trim(), player1Name.Trim(), true); // First player is white
        PlayerStates[1] = Objects.CreatePlayerState(player2Type.Trim(), player2Name.Trim(), false); // Second player is black
        if (PlayerStates[0] is BotState botState1)
            botState1.CurrentGame = this;
        if (PlayerStates[1] is BotState botState2)
            botState2.CurrentGame = this;

        OccupancyBoard = 0; // Initialize occupancy board
        Winner = -1; // no winner to start

        MoveCount=0;
        lastThreeStates = new Queue<string>(); lastThreeStates.Enqueue(HashA());
        NoCaptureNoPawnMoveCount=0;

    }
    public GameState(GameState original){
        PlayerStates = new PlayerState[2];
        PlayerStates[0] = original.PlayerStates[0].Clone();
        PlayerStates[1] = original.PlayerStates[1].Clone();

        currentIndex = original.currentIndex;
        PromoteTo = original.PromoteTo;

        Winner = original.Winner;

        MoveCount = original.MoveCount;
        lastThreeStates = new Queue<string>(original.lastThreeStates);
        NoCaptureNoPawnMoveCount = original.NoCaptureNoPawnMoveCount;

        Initialize();
    }
    public GameState Clone() => new GameState(this);

    public string HashA(){
        // Simple example: concatenate relevant properties
        StringBuilder hashBuilder = new StringBuilder();

        foreach (PlayerState playerState in PlayerStates){
            foreach (PieceBoard pieceBoard in playerState.PieceBoards.Values){
                hashBuilder.Append(pieceBoard.Type);
                List<int> bitPositions = BitOps.GetAllSetBitIndicesLinear(pieceBoard.Bitboard);
                foreach (int bitPosition in bitPositions){
                    //Debug.Log(bitPosition + " " + pieceBoard.Type + " " + playerState.TurnIndex);
                    hashBuilder.Append(bitPosition);
                }
            }
        }
        hashBuilder.Append(currentIndex); // Include whose turn it is
        //hashBuilder.Append(MoveCount);
        return hashBuilder.ToString();
    }



public string HashD()
{
    using (SHA256 sha256 = SHA256.Create())
    {
        // Create a MemoryStream for efficient writing
        using (MemoryStream stream = new MemoryStream())
        {
            foreach (PlayerState playerState in PlayerStates)
            {
                foreach (PieceBoard pieceBoard in playerState.PieceBoards.Values)
                {
                    // Hash piece type
                    byte[] typeBytes = Encoding.UTF8.GetBytes(pieceBoard.Type.ToString());
                    stream.Write(typeBytes, 0, typeBytes.Length);

                    // Hash piece positions (bitboard)
                    List<int> bitPositions = BitOps.GetAllSetBitIndicesLinear(pieceBoard.Bitboard);
                    foreach (int bitPosition in bitPositions)
                    {
                        byte[] positionBytes = BitConverter.GetBytes(bitPosition);
                        stream.Write(positionBytes, 0, positionBytes.Length);
                    }
                }
            }

            // Include whose turn it is
            byte[] turnBytes = BitConverter.GetBytes(currentIndex);
            stream.Write(turnBytes, 0, turnBytes.Length);

            // Compute SHA-256 hash directly from the memory stream
            byte[] hashBytes = sha256.ComputeHash(stream.ToArray());
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}

    public string HashC(int depth){
        // Simple example: concatenate relevant properties
        StringBuilder hashBuilder = new StringBuilder();

        foreach (PlayerState playerState in PlayerStates){
            foreach (PieceBoard pieceBoard in playerState.PieceBoards.Values){
                hashBuilder.Append(pieceBoard.Type);
                List<int> bitPositions = BitOps.GetAllSetBitIndicesLinear(pieceBoard.Bitboard);
                foreach (int bitPosition in bitPositions){
                    //Debug.Log(bitPosition + " " + pieceBoard.Type + " " + playerState.TurnIndex);
                    hashBuilder.Append(bitPosition);
                }
            }
        }
        hashBuilder.Append(currentIndex); // Include whose turn it is
        hashBuilder.Append(depth); // Include depth
        //hashBuilder.Append(MoveCount);
        return hashBuilder.ToString();
    }


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
        foreach (PieceBoard pieceBoard in player.PieceBoards.Values){
            List<int> pieceIndexes = BitOps.GetAllSetBitIndicesLinear(pieceBoard.Bitboard);
            foreach (int pieceIndex in pieceIndexes)
                if(GetMovesAllowed(pieceBoard, pieceIndex)!=0UL)
                    return true;
        }
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
            PieceBoard opponentPieceBoard = kvpPiece.Value;
            if(kvpPiece.Key=='K' && opponentPieceBoard is KingBoard oppKingBoard){
                opponentMoves |= oppKingBoard.GetAttackMoves();
            }else if(kvpPiece.Key=='P' && opponentPieceBoard is PawnBoard oppPawnBoard){
                opponentMoves |= oppPawnBoard.GetAttackMoves();
            }else{
                ulong enemyBoardExceptKingPos = otherPlayer.OccupancyBoard & ~(otherPlayer.PieceBoards['K'].Bitboard);
                List<int> pieceIndexes = BitOps.GetAllSetBitIndicesLinear(opponentPieceBoard.Bitboard);
                foreach (int pieceIndex in pieceIndexes)
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

        //Debug.Log("move invoked updated");
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

        bool isPawnMove = pieceBoard is PawnBoard && originalIndex != index; // A pawn move (doesn't involve a capture)
       

        int removedPieceIndex = -1; // no removed piece
        if (isCapture) // already valifdated move by now
        {
            // Remove the piece from the opponent's board
            PieceBoard opponentPieceBoard = GetPieceBoard(index, PlayerStates[1 - currentIndex]);
            PlayerStates[1-currentIndex].RemovePiece(opponentPieceBoard, index);
            //Debug.Log($"Captured opponent's piece at index {index}.");
            removedPieceIndex = index;
        }
        else if(isEnPassantCapture)
        {
            PlayerStates[1-currentIndex].EnPassantChosen();
            //Debug.Log($"Captured opponent's piece enpassant {index}.");
            removedPieceIndex = (PlayerStates[1-currentIndex].PieceBoards['P'] as PawnBoard).enPassantablePawn;
        }


        if(IsPromotion(pieceBoard, index)){
            if(PromoteTo!='\0'){
                //Debug.Log("Promotion!"+PromoteTo);
                // remove piece from this pawn pieceBoard
                pieceBoard.RemovePiece(originalIndex);
                // update pieceBoard 

                // add to selected pieceboard
                PlayerStates[currentIndex].PieceBoards[PromoteTo].AddPiece(index);
                // update pieceBpard
                // create piece ui

                MoveUpdate(index, isCapture:isCapture, capturedPosition:removedPieceIndex, isPromotion:true);
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

        // TRACKINGS
        //Track move count
        MoveCount++;

        // Track the last three game state hashes
        string currentHash = HashA();
        if (lastThreeStates.Count == 3)
            lastThreeStates.Dequeue(); // Remove the oldest hash
        lastThreeStates.Enqueue(currentHash); // Add the current hash


        //Debug.Log(isPawnMove + " " + isCapture + " "+ isEnPassantCapture + " " +(isPawnMove || isCapture || isEnPassantCapture) + " " + NoCaptureNoPawnMoveCount);
        // Track cap or awnmove
        NoCaptureNoPawnMoveCount = (isPawnMove || isCapture || isEnPassantCapture)?
                0 : NoCaptureNoPawnMoveCount+1;


        SwitchPlayer();
        UpdateBoard();
        UpdateGameState();
        Gameover = IsGameEnd();
        
    }

    
    public ulong ValidateMoves(PieceBoard pieceboard, int index, ulong moves)
    {
        ulong filteredMoves = moves;
        bool isKing = pieceboard.Type == 'K' && pieceboard is KingBoard;
        PlayerState currPlayer = PlayerStates[currentIndex];

        // Condition 1: If in double-check, only allow king's moves
        if (currPlayer.DoubleCheck)
        {
            return  isKing ? moves : 0UL;
        }

        // Condition 3: (Handle pinned pieces if needed)
        int kingIndex = currPlayer.GetKingIndex();
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
            
            //Make sure rook exists
            //(currPlayer.PieceBoards['R'].Bitboard & BitOps.a1<<0) != 0;
            //make sure it is rooks first move (//Make sure rook exists)
            //currPlayer.PieceBoards['R'].FirstMovers.Contains(0);
            //make sure no opp piece atatcks spaces between king abd rook

            ulong allAttackedSquares = GetAllAttackMoves(PlayerStates[1-currentIndex]);
            ulong kingCastleMove = (pieceboard as KingBoard).GetKingsideCastlingMoves(OccupancyBoard, currPlayer.OccupancyBoard),
                queenCastleMove = (pieceboard as KingBoard).GetQueensideCastlingMoves(OccupancyBoard, currPlayer.OccupancyBoard);
            int kcastleIndex = BitOps.GetFirstSetBitIndexBSR(kingCastleMove),
                qcastleIndex = BitOps.GetFirstSetBitIndexBSR(queenCastleMove);
            // check KingSide
            if(Math.Abs(kingIndex-kcastleIndex)==2 
            &&  (!(currPlayer.PieceBoards['R'].FirstMovers.Contains(currPlayer.IsWhite? 7:63))
                ||((KingSideSquares & allAttackedSquares) != 0)
                ||PlayerStates[currentIndex].InCheck
                )
            ){
                filteredMoves &= ~(kingCastleMove); // remove the castle move
            }

            // check QueenSide
            if(Math.Abs(kingIndex-qcastleIndex)==2
            &&  (!(currPlayer.PieceBoards['R'].FirstMovers.Contains(currPlayer.IsWhite? 0:56))
                ||((QueenSideSquares & allAttackedSquares) != 0)
                ||PlayerStates[currentIndex].InCheck
                )
            ){
                filteredMoves &= ~(queenCastleMove); // remove the castle move
            }
        }


        // Condition 2: Check for king attacker
        if(PlayerStates[currentIndex].InCheck){
            int kingAttackerIndex = currPlayer.KingAttacker;
            ulong attackerPosition = BitOps.a1 << kingAttackerIndex;
            ulong path = GetAttackPath(kingAttackerIndex, kingIndex, PlayerStates[1 - currentIndex]);

            if (!isKing){
                // Only allow moves that block the attack or capture the attacker
                filteredMoves &= (path | attackerPosition);
            }else if(isKing){ //Handle castling

                // remove all castling moves since king in check
                //filteredMoves &= ~((pieceboard as KingBoard).GetCastlingMoves());
                ulong kingCastleMove = (pieceboard as KingBoard).GetKingsideCastlingMoves(OccupancyBoard, currPlayer.OccupancyBoard),
                    queenCastleMove = (pieceboard as KingBoard).GetQueensideCastlingMoves(OccupancyBoard, currPlayer.OccupancyBoard);
                int kcastleIndex = BitOps.GetFirstSetBitIndexBSR(kingCastleMove),
                    qcastleIndex = BitOps.GetFirstSetBitIndexBSR(queenCastleMove);

                if(Math.Abs(kingIndex-kcastleIndex)==2)
                    filteredMoves &= ~(kingCastleMove);
                if(Math.Abs(kingIndex-qcastleIndex)==2)
                    filteredMoves &= ~(queenCastleMove);

                // king must leav check or cap attacker
                filteredMoves &= (~(path) | attackerPosition);
                return filteredMoves;
            }
        }

        // remove opposing king postiion to prevent king captures
        ulong otherPlayerKingPosition = PlayerStates[1-currentIndex].PieceBoards['K'].Bitboard;
        filteredMoves &= (~otherPlayerKingPosition);
        
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
                    pieceType == 'Q' ? BitOps.MovementType.Queen :
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
                List<int> oppIndexes = BitOps.GetAllSetBitIndicesLinear(kvpPiece.Value.Bitboard);
                foreach (int oppIndex in oppIndexes){
                    if(IsLineValid(oppIndex, kingIndex, kvpPiece.Key)){ // if there is a path
                        pinnedMovement=GetAttackPath(oppIndex, kingIndex, otherPlayer); //get the path
                        ulong piecePosition=BitOps.a1 << pieceIndex,
                            attackerPosition=BitOps.a1 << oppIndex;
                        // Debug.Log(pieceIndex + " " + kvpMoves.Key + " "+ pinnedMovement);
                        bool otherPieceOnPath = (OccupancyBoard & (pinnedMovement & ~(attackerPosition) & ~(piecePosition)))!=0;
                        if(otherPieceOnPath){
                            pinnedMovement=0UL; // reset, continue the search for a pinn
                        }else{
                            pinnedMovement |= attackerPosition;
                            bool pinned = (pinnedMovement&piecePosition)!=0;
                            if(pinned){
                                return true; // if piece is indeed pinned, no need to continue search
                            }else{
                                pinnedMovement = 0UL; // continue searching
                            }
                            
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

        otherPlayer.KingAttacker=attacker;
        
        //Debug.Log("Player Check Update: "+otherPlayer.PlayerType +" is in check?: "+otherPlayer.InCheck + " "+ otherPlayer.DoubleCheck + " " +otherPlayer.IsInCheck + "by attackker at " + attacker);
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
            //Debug.Log($"GAME OVER:{player.PlayerType} IS CHECKMATED");
            Winner = 1-player.TurnIndex;
            return true;
        }
        return false;
    }
    public bool CheckCheckmate()=> PlayerCheckmated(PlayerStates[0]) || PlayerCheckmated(PlayerStates[1]);

    public bool PlayerStalemated(PlayerState player){
        if(!hasMoves(player) && currentIndex==player.TurnIndex && !player.IsInCheck){
            //Debug.Log($"GAME OVER: DRAW-> {player.PlayerType} STALEMATED");
            return true;
        }
        return false;
    }
    public bool CheckStalemate()=> PlayerStalemated(PlayerStates[0]) || PlayerStalemated(PlayerStates[1]);

    public bool IsInsufficientMaterial()
    {
        bool player1HasInsufficientMaterial = HasInsufficientMaterialForPlayer(PlayerStates[0]);
        bool player2HasInsufficientMaterial = HasInsufficientMaterialForPlayer(PlayerStates[1]);

        // The game is a draw only if both players have insufficient material
        return player1HasInsufficientMaterial && player2HasInsufficientMaterial;
    }
    private bool HasInsufficientMaterialForPlayer(PlayerState player)
    {
        int pieceCount = 0;
        bool hasKnight = false;
        bool hasBishop = false;
        bool hasQueen = false;
        bool hasRook = false;
        bool hasPawn = false; // Track if the player has pawns

        // Loop through the player's PieceBoards and check the size of ValidMovesMap to count pieces
        foreach (var pieceBoard in player.PieceBoards.Values)
        {
            //int pieceCountForType = pieceBoard.ValidMovesMap.Count; // The size of the ValidMovesMap gives the number of pieces
            int pieceCountForType = BitOps.CountSetBits(pieceBoard.Bitboard);
            // Determine what piece this is based on the piece type
            switch (pieceBoard.Type)
            {
                case 'K': // King (should always be 1, but check for completeness)
                    break;
                case 'N': // Knight
                    hasKnight = pieceCountForType>0;
                    break;
                case 'B': // Bishop
                    hasBishop = pieceCountForType>0;
                    break;
                case 'R': // Rook
                    hasRook = pieceCountForType>0;
                    break;
                case 'Q': // Queen
                    hasQueen = pieceCountForType>0;
                    break;
                case 'P': // Pawn
                    hasPawn = pieceCountForType>0; // Found a pawn, the player has sufficient material
                    break;
            }
            pieceCount += pieceCountForType;

        }

        //Debug.Log(player.PlayerName + " "+hasPawn + " " + hasQueen + " " + hasRook + pieceCount);

        // If the player has any pawns, they have sufficient material
        if (hasPawn) return false; // Pawn means the player can promote, so they have sufficient material

        // Check for immediate cases of sufficient material
        if (hasQueen || hasRook) return false; // If the player has a Queen or Rook, they can potentially checkmate
        if (pieceCount > 2 && (hasKnight || hasBishop)) return false; // Two minor pieces (Knight/Bishop) are enough for checkmate

        // Now check for insufficient material:
        // If only Kings, or Kings + Knight/Bishop, we have an insufficient material scenario
        if (pieceCount == 1) return true; // King vs King (insufficient material)
        if (pieceCount == 2 && !(hasPawn || hasRook || hasQueen) && (hasKnight || hasBishop)) return true; // King vs King + Knight or King vs King + Bishop
 
        // If we haven't returned yet, the player likely has enough material for checkmate
        return false;
    }



    public bool CheckInsufficientMaterial()
    {
        // Check for insufficient material draw (both players need insufficient material)
        if (IsInsufficientMaterial())
        {
            return true;
        }
        return false;
    }

    public bool Check50MoveRule()
    {

        // Check for the 50-move rule: no capture or pawn move for 50 moves per player (100 total moves)
        if (NoCaptureNoPawnMoveCount >= 50 || MoveCount >=500)
        {
            return true;
        }
        return false;
    }

    public bool CheckThreefoldRepetition() 
    {
        // Check for threefold repetition: same position occurs 3 times
        string currentHash = HashA();
        int count = 0;
        foreach (string hash in lastThreeStates)
        {
            if (hash == currentHash)
            {
                count++;
            }
        }

        if (count >= 2) // Repetition has occurred 3 times (including the current state)
        {
            return true;
        }
        return false;
    }

    public bool Draw()=>CheckInsufficientMaterial() || Check50MoveRule() || CheckThreefoldRepetition();


    public bool IsGameEnd()=>CheckCheckmate() || CheckStalemate() || Draw();

    // For Bots
    public void MakeBotMove(int fromIndex, int toIndex){

        // find the piece at from
        PieceBoard pieceToMove = GetPieceBoard(fromIndex, PlayerStates[currentIndex]);
        // set selectedPiece for any listener
        
        if(IsPromotion(pieceToMove, toIndex)){
            // set promotedPawn for any listener
            PromoteTo = PlayerStates[currentIndex].PromoteTo;
        }

        ExecuteMove(pieceToMove, fromIndex, toIndex);
    }
}
