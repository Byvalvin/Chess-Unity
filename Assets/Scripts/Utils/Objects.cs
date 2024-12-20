using System;
using System.Collections.Generic;
using UnityEngine;

public static class Objects
{
    private static readonly Dictionary<string, Type> PlayerStateTypes = new Dictionary<string, Type>();

    static Objects()
    {
        // Cache player state types on initialization
        foreach (var type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
        {
            // Exclude abstract types and specifically exclude BotState
            if (type.IsSubclassOf(typeof(PlayerState)) && !type.IsAbstract && type != typeof(BotState))
            {
                PlayerStateTypes[type.Name] = type;
            }
        }

        // Add PlayerState class itself
        PlayerStateTypes[typeof(PlayerState).Name] = typeof(PlayerState);
    }

    public static PlayerState CreatePlayerState(string playerTypeName, string playerName, bool isWhite)
    {
        if (PlayerStateTypes.TryGetValue($"{playerTypeName}State", out var type))
        {
            return (PlayerState)Activator.CreateInstance(type, playerName, isWhite);
        }

        // Optional: log an error or throw an exception if the type is not found
        //Debug.LogError($"Player type '{playerTypeName}State' not found.");
        return null;
    }
}


public static class ZobristHashing
{
    // Zobrist table: [64 positions] x [12 types (6 for each player)]
    private static readonly ulong[,] zobristTable = new ulong[64, 12];  
    private static readonly System.Random random = new System.Random();

    // Initialize the Zobrist table with random values
    static ZobristHashing()
    {
        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                // Cast random.Next() to uint before shifting
                zobristTable[i, j] = ((ulong)(uint)random.Next() << 32) | (ulong)(uint)random.Next();
            }
        }
    }

    // Get the piece index for each piece type (White pieces 0-5, Black pieces 6-11)
    public static int GetPieceIndex(char pieceType, bool isWhite)
    {
        switch (pieceType)
        {
            case 'P': return isWhite ? 0 : 6;  // Pawn
            case 'R': return isWhite ? 1 : 7;  // Rook
            case 'N': return isWhite ? 2 : 8;  // Knight
            case 'B': return isWhite ? 3 : 9;  // Bishop
            case 'Q': return isWhite ? 4 : 10; // Queen
            case 'K': return isWhite ? 5 : 11; // King
            default: throw new InvalidOperationException("Invalid piece type");
        }
    }

    // This method calculates the hash for a full game state
    public static ulong CalculateHash(GameState gameState)
    {
        ulong hash = 0;

        // Iterate over each player state
        foreach (PlayerState playerState in gameState.PlayerStates)
        {
            // Iterate over each piece board (PawnBoard, RookBoard, etc.)
            foreach (PieceBoard pieceBoard in playerState.PieceBoards.Values)
            {
                // For each piece type, get the set bit positions
                List<int> bitPositions = BitOps.GetAllSetBitIndicesLinear(pieceBoard.Bitboard);

                // XOR the Zobrist values for each bit position
                foreach (int bitPosition in bitPositions)
                {
                    int pieceIndex = GetPieceIndex(pieceBoard.Type, pieceBoard.IsWhite);
                    hash ^= zobristTable[bitPosition, pieceIndex];
                }
            }
        }

        // Optionally, include player turn information (e.g., whose turn it is)
        hash ^= gameState.currentIndex == 0 ? 1UL : 0UL;

        return hash;
    }

    public static ulong UpdateHash(ulong currentHash, int fromIndex, int toIndex, char piecetype, bool isWhite)
    {
        // XOR out the old piece and XOR in the new piece at the destination
        int fromPieceIndex = GetPieceIndex(piecetype, isWhite);
        currentHash ^= zobristTable[fromIndex, fromPieceIndex];  // XOR out the old piece
        currentHash ^= zobristTable[toIndex, fromPieceIndex];    // XOR in the new piece

        return currentHash;
    }
}


public static class Profiler
{
    private static Dictionary<string, float> timings = new Dictionary<string, float>();
    private static Dictionary<string, int> counts = new Dictionary<string, int>();

    public static void Start(string label)
    {
        if (!timings.ContainsKey(label))
        {
            timings[label] = 0;
            counts[label] = 0;
        }
    }

    public static void Stop(string label)
    {
        float elapsedTime = Time.realtimeSinceStartup;
        timings[label] += elapsedTime;
        counts[label]++;
    }

    public static void LogTimings()
    {
        foreach (var entry in timings)
        {
            string label = entry.Key;
            float totalTime = entry.Value;
            int count = counts[label];
            Debug.Log($"{label}: {totalTime} seconds over {count} runs");
        }
    }
}

