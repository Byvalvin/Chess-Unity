using System.Collections.Generic;
using System;
using UnityEngine;
public abstract class PieceBoard
{
    public static event Action OnPieceMoved; // update the gameState(currentPlayer, board)
    public ulong Bitboard { get; set; }
    public char Type { get; set; }
    public bool IsWhite { get; set; }
    protected Dictionary<int, bool> FirstMoveMap { get; set; }

    public PieceBoard(bool isWhite, ulong startingBitboard = 0)
    {
        IsWhite = isWhite;
        Bitboard = startingBitboard;
        FirstMoveMap = new Dictionary<int, bool>();

        if (startingBitboard != 0)
        {
            for (int i = 0; i < 64; i++)
            {
                if ((startingBitboard & (BitOps.a1 << i)) != 0)
                {
                    FirstMoveMap[i] = true;
                    Debug.Log(IsWhite + ": " +i+" "+FirstMoveMap[i]);
                }
            }
        }
    }

    public PieceBoard(PieceBoard original)
    {
        IsWhite = original.IsWhite;
        Bitboard = original.Bitboard;
        FirstMoveMap = new Dictionary<int, bool>(original.FirstMoveMap);
    }

    public abstract PieceBoard Clone();
    public virtual bool CanMove(int fromIndex, int toIndex){
        // Use the IsValidMove method to check the move
        return BitOps.IsValidMove(fromIndex, toIndex) && ValidMoves(fromIndex).Contains(toIndex);
    }
    public abstract HashSet<int> ValidMoves(int index);

    public void Move(int fromIndex, int toIndex)
    {
        if(CanMove(fromIndex, toIndex)){
            Bitboard &= ~(BitOps.a1 << fromIndex);
            Bitboard |= (BitOps.a1 << toIndex);

            if (FirstMoveMap.ContainsKey(fromIndex))
            {
                FirstMoveMap[fromIndex] = false;
            }

            // ALERT LISTENERS
            OnPieceMoved?.Invoke();
        }
    }

    public static void PrintBitboard(ulong bitboard)
    {
        // Convert the bitboard to a binary string and pad with leading zeros
        string binaryString = Convert.ToString((long)bitboard, 2).PadLeft(64, '0');

        // Print the binary string in a readable format
        for (int i = 0; i < 8; i++)
        {
            // Take a substring for each rank (8 bits)
            string rank = binaryString.Substring(i * 8, 8);
            // Reverse the rank to match chessboard orientation
            char[] reversedRank = rank.ToCharArray();
            Array.Reverse(reversedRank);
            Debug.Log(new string(reversedRank)); // Print each rank
        }
    }
}
