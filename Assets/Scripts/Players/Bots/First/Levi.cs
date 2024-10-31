using System;
using System.Collections.Generic;
using UnityEngine;

public class Levi : Bot
{

    // Add methods for player actions, like making a move, if needed
}


public class LeviState : BotState
{
    private const int MaxDepth = 4,
                    KingThreatPenalty = 10,
                    CaptureScoreMultiplier = 2,
                    PieceProtectionReward = 5;

    public LeviState(string playerName, bool isWhite) : base(playerName, isWhite){}

    public LeviState(LeviState original) : base(original){}
    public override PlayerState Clone() => new LeviState(this);

    protected override int EvaluateMove(int fromIndex, int toIndex, GameState clone){
        clone.MakeBotMove(fromIndex, toIndex);
        int movescore = Minimax(clone, MaxDepth, int.MinValue, int.MaxValue, !IsWhite);
        Debug.Log("Scoring| "+ fromIndex + " to" + toIndex + ": " + movescore);
        return movescore;
    }
    private bool IsGameOver(GameState gameState){
        // Implement logic to determine if the game is over (checkmate, stalemate, etc.)
        return gameState.IsGameEnd(); // Placeholder
    }
    private int Minimax(GameState gameState, int depth, int alpha, int beta, bool maximizingPlayer){
        //string hashKey = gameState.HashA(); // Generate the hash for the current game state
        //ulong hashKey = gameState.HashB(); // Generate the hash for the current game state
        /*
        Debug.Log(maximizingPlayer+" "+depth + " "+ alpha + " " + beta);
        Debug.Log(TT + "for tt" + hashKey);
        */
        /*
        foreach (var item in TT){
            Debug.Log("Hasing: "+ item.Key + " " + item.Value);
        }
        
        // Check if we have already evaluated this game state
        if (TT.TryGetValue(hashKey, out int cachedValue))
            return cachedValue; // Return the cached evaluation
        */
        if (depth == 0 || IsGameOver(gameState))
            return EvaluateGameState(gameState);

        int eval;
        if (maximizingPlayer == IsWhite){
            int maxEval = int.MinValue;
            foreach (var move in GenerateAllMoves(gameState, TurnIndex)){
                //Debug.Log(move);
                GameState clonedGame = gameState.Clone();
                clonedGame.MakeBotMove(move.x, move.y);
                eval = Minimax(clonedGame, depth - 1, alpha, beta, !IsWhite);
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);

                if (beta <= alpha)
                    break; // Alpha-beta pruning
            }
            eval = maxEval;
        }
        else{
            int minEval = int.MaxValue;
            foreach (var move in GenerateAllMoves(gameState, 1 - TurnIndex)){
                GameState clonedGame = gameState.Clone();
                clonedGame.MakeBotMove(move.x, move.y);
                eval = Minimax(clonedGame, depth - 1, alpha, beta, IsWhite);
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);

                if (beta <= alpha)
                    break; // Alpha-beta pruning
            }
            eval = minEval;
        }

        // Store the evaluation in the transposition table
        //TT[hashKey] = eval;

        return eval;
    }

    protected override int EvaluateGameState(GameState gameState){
        int score = 0;
        // game ending moves
        //score = GameEndingMove(score, gameState);
        if(score!=0) return score;

        // Evaluate material balance
        score += EvaluateMaterialDiff(gameState);

        // Evaluate piece positioning
        score += EvaluatePositioning(gameState);

        // Evaluate king safety and control
        score += EvaluateKingSafety(gameState);
        score += EvaluateMobilityDiff(gameState);

        return score;
    }

    private int EvaluateMaterialDiff(GameState gameState){
        return EvaluateMaterial(gameState, TurnIndex) - EvaluateMaterial(gameState, 1 - TurnIndex);
    }

    private int EvaluatePositioning(GameState gameState){

        return EvaluateCenterControl(gameState);
    }


    private int EvaluateMobilityDiff(GameState gameState){
        int spaceControl = EvaluateMobility(gameState, TurnIndex),
            enemyControl = EvaluateMobility(gameState, 1-TurnIndex);
                
        return 5 * (spaceControl - enemyControl);
    }
    private int EvaluateKingThreat(GameState gameState)
    {
        int safetyScore = 0;
        PlayerState thisPlayer = gameState.PlayerStates[TurnIndex];

        KingBoard thisKing = (thisPlayer.PieceBoards['K'] as KingBoard);
        ulong kingBitBoard = thisKing.Bitboard;
        int kingBitIndex = thisPlayer.GetKingIndex();
        
        // Check for direct threats to the king
        foreach (PieceBoard pieceBoard in gameState.PlayerStates[1 - TurnIndex].PieceBoards.Values){
            foreach (var kvp in pieceBoard.ValidMovesMap)
            {
                ulong attackMoves = 0UL;
                // Check if the opponent can directly attack the king
                if(pieceBoard is KingBoard king){
                    attackMoves = king.GetAttackMoves();
                }else if (pieceBoard is PawnBoard pawn){
                    attackMoves = pawn.GetAttackMove(kvp.Key);
                }
                if((kingBitBoard & attackMoves)!=0){
                    safetyScore-=KingThreatPenalty;
                }

                // Check if opponent can attack king's escape moves
                safetyScore -= BitOps.CountSetBits((thisKing.ValidMovesMap[kingBitIndex] & attackMoves))*KingThreatPenalty;
            }

            
        }
        
        return safetyScore;
    }


    private List<Vector2Int> GenerateAllMoves(GameState gameState, int playerIndex){
        var moves = new List<Vector2Int>();
        var pieceBoards = gameState.PlayerStates[playerIndex].PieceBoards.Values;

        foreach (var pieceBoard in pieceBoards)
        {
            foreach (int pieceIndex in pieceBoard.ValidMovesMap.Keys)
            {
                //Debug.Log(pieceIndex+"is piece index "+pieceBoard.Type);
                var validMoves = gameState.GetMovesAllowed(pieceBoard, pieceIndex);
                while (validMoves != 0)
                {
                    ulong bit = validMoves & (~(validMoves - 1)); // Isolate the rightmost set bit
                    int toIndex = BitOps.BitScan(bit); // Get the index of the isolated bit
                    //Debug.Log(toIndex+"is to index");
                    if((bit & gameState.PlayerStates[1-TurnIndex].OccupancyBoard)!=0){
                        // Prioritize capturing moves
                        moves.Insert(0, new Vector2Int(pieceIndex, toIndex));
                    }else{
                        moves.Add(new Vector2Int(pieceIndex, toIndex));
                    }
                    // Clear the rightmost set bit to continue
                    validMoves ^= bit;
                }
            }
            
        }

        return moves;
    }

    
    

}
