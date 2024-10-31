using System.Collections.Generic;
using UnityEngine;
using System;

public class Bot : Player
{

    // Add methods for player actions, like making a move, if needed
}


public abstract class BotState : PlayerState
{
    // Material Values (simplified)
    protected Dictionary<char, int> pieceValues = new Dictionary<char, int>
    {
        { 'P', 1 },  // Pawn
        { 'N', 3 },  // Knight
        { 'B', 3 },  // Bishop
        { 'R', 5 },  // Rook
        { 'Q', 9 },  // Queen
        { 'K', 0 }     // King (no value since it can't be captured)
    };
            // Define center squares
    protected const ulong centerSquares = 0x0000001818000000; // e4, e5, d4, d5
    private const int KingTileValue = 3;



    public GameState CurrentGame{get; set;}
    // Transposition table
    protected Dictionary<string, int> TT = new Dictionary<string, int>();

    public BotState(string playerName, bool isWhite) : base(playerName, isWhite){}

    public BotState(BotState original) : base(original){
        this.CurrentGame = original.CurrentGame;
    }
    public abstract override PlayerState Clone();
    

    public virtual Vector2Int GetMove(){
        
        Dictionary<int, ulong> moveMap = new Dictionary<int, ulong>();
        foreach (PieceBoard pieceBoard in PieceBoards.Values){
            foreach (var kvp in pieceBoard.ValidMovesMap)
            {
                ulong validMoves = CurrentGame.GetMovesAllowed(pieceBoard, kvp.Key);
                if(validMoves!=0) moveMap[kvp.Key] = validMoves;
            }
            
        }

        // call the thing that determines the mvoe to play given all the valid mvoes of all pieces
        Vector2Int completeMove = Evaluate(moveMap);
    
        return completeMove;
    }
    protected virtual int EvaluateGameState(GameState gameState)=>1;
    protected virtual int EvaluateMove(int fromIndex, int toIndex, GameState clone){
        clone.MakeBotMove(fromIndex, toIndex);
        return EvaluateGameState(clone);// placeholder assumes all moves are equal but diff bots will have diff scoring
    }
    
    protected virtual Vector2Int Evaluate(Dictionary<int, ulong> moveMap){
        //return new Vector2Int(8,16);

        int bestFromIndex = -1;
        int bestToIndex = -1;
        int bestScore = int.MinValue; // Initialize with the lowest possible score

        // Loop through each piece's valid moves
        foreach (var kvp in moveMap)
        {
            int from = kvp.Key;
            ulong allTo = kvp.Value;

            // Iterate over the bits in the ulong to find all possible destination indices
            while (allTo != 0)
            {
                ulong bit = allTo & (~(allTo - 1)); // Isolate the rightmost set bit
                int toIndex = BitOps.BitScan(bit); // Get the index of the isolated bit
                
                // Clone the current game state for evaluation
                GameState clonedGame = CurrentGame.Clone();

                // Evaluate the move and get the score
                int score = EvaluateMove(from, toIndex, clonedGame);

                // Update the best score and corresponding indices if this move is better
                if (score > bestScore)
                {
                    bestScore = score;
                    bestFromIndex = from;
                    bestToIndex = toIndex;
                }

                // Clear the rightmost set bit to continue
                allTo ^= bit;
            }
        }
        return new Vector2Int(bestFromIndex, bestToIndex);
    }


    // all eval functions

    protected int EvaluateMaterial(GameState gameState, int playerIndex){
        PlayerState currPlayer = gameState.PlayerStates[playerIndex];
        // Evaluate material balance
        int score = 0;
        foreach (var pieceBoard in currPlayer.PieceBoards)
        {
            char pieceType = pieceBoard.Key;
            int pieceValue = pieceValues[pieceType];
            score += pieceBoard.Value.ValidMovesMap.Count * pieceValue;  // White pieces add to score
        }
        return score;
    }

    // Example: Function to evaluate king safety
    protected int EvaluateKingSafety(GameState gameState)
    {
        PlayerState currPlayer = gameState.PlayerStates[TurnIndex];
        int score = 0;

        // Check positions of both kings and potential threats
        // This can be expanded based on specific criteria
        if (currPlayer.IsInCheck)
        {
            score -= 5; // Penalize for being in check
        }

        return score;
    }

    protected int KingTiles(GameState gameState){
        PlayerState oppPlayer = gameState.PlayerStates[1 - TurnIndex];
        KingBoard opposingKing = (oppPlayer.PieceBoards['K'] as KingBoard);
        int tileCount = BitOps.CountSetBits(opposingKing.ValidMovesMap[oppPlayer.GetKingIndex()]);
        return KingTileValue * (8 - tileCount);
    }

    // Example: Function to evaluate center control
    protected int EvaluateCenterControl(GameState gameState)
    {
        PlayerState currPlayer = gameState.PlayerStates[TurnIndex];
        int score = 0;

        // Count pieces in center squares
        foreach (PieceBoard pieceBoard in currPlayer.PieceBoards.Values)
        {
            ulong pieceBitboard = pieceBoard.Bitboard;

            if ((pieceBitboard & centerSquares) != 0)
            {
                score += 5; // Reward for occupying center squares
            }
            foreach (ulong moves in pieceBoard.ValidMovesMap.Values)
            {
                score += BitOps.CountSetBits((moves & centerSquares));
            }
        }

        return score;
    }

    // helpwer
    protected int CountAttackers(GameState gameState, ulong targetBitboard)
    {
        int attackerCount = 0;
        foreach (PieceBoard pieceBoard in gameState.PlayerStates[TurnIndex].PieceBoards.Values)
        {
            foreach (var kvp in pieceBoard.ValidMovesMap)
            {
                int from = kvp.Key;

                // get correct attack moves
                ulong attackMoves = kvp.Value;
                if(pieceBoard is KingBoard kingBoard)
                    attackMoves = kingBoard.GetAttackMoves();
                else if(pieceBoard is PawnBoard pawnBoard)
                    attackMoves = pawnBoard.GetAttackMove(from);
                
                // check if attacker
                if((attackMoves & targetBitboard) != 0) // Check if the piece can attack the target
                    attackerCount++;
            }
        }
        return attackerCount;
    }
    protected int CountDefenders(GameState gameState, ulong targetBitboard)
    {
        PlayerState currPlayer = gameState.PlayerStates[TurnIndex],
                otherPlayer = gameState.PlayerStates[1-TurnIndex];
        int attackerCount = 0;
        foreach (PieceBoard pieceBoard in gameState.PlayerStates[TurnIndex].PieceBoards.Values)
        {
            foreach (var kvp in pieceBoard.ValidMovesMap)
            {
                int from = kvp.Key;

                // get correct attack moves
                ulong attackMoves = 0UL;
                if(pieceBoard is KingBoard kingBoard)
                    attackMoves = kingBoard.GetAttackMoves();
                else if(pieceBoard is PawnBoard pawnBoard)
                    attackMoves = pawnBoard.GetAttackMove(from);
                else
                    attackMoves = pieceBoard.GetValidMoves(currPlayer.OccupancyBoard, from, otherPlayer.OccupancyBoard, includeFriends:true);
                
                // check if attacker
                if((attackMoves & targetBitboard) != 0) // Check if the piece can attack the target
                    attackerCount++;
            }
        }
        return attackerCount;
    }

    protected int EvaluateMobility(GameState clone)
    {
        int mobilityScore = 0;
        foreach (PieceBoard pieceBoard in clone.PlayerStates[TurnIndex].PieceBoards.Values)
        {
            foreach (ulong moves in pieceBoard.ValidMovesMap.Values)
            {
                mobilityScore += BitOps.CountSetBits(moves);
            }
        }
        return mobilityScore;
    }

    protected int EvaluatePieceSafety(char type, GameState toGameState, ulong fromtarget, ulong totarget){
        int toNotSafe = CountAttackers(toGameState, totarget);
        int fromNotSafe = -CountAttackers(CurrentGame, fromtarget);
        return (toNotSafe + fromNotSafe) * pieceValues[type];
    }




}
