using System.Collections.Generic;
using System;
using UnityEngine;
public abstract class PieceBoard
{
    public ulong Bitboard { get; set; }
    public char Type { get; set; }
    public bool IsWhite { get; set; }
    protected HashSet<int> FirstMovers { get; set; }

    public Dictionary<int, ulong> ValidMovesMap{get; protected set;}

    public static Vector2Int purgatory = new Vector2Int(-100, -100);

    public PieceBoard(bool isWhite, ulong startingBitboard = 0)
    {
        IsWhite = isWhite;
        Bitboard = startingBitboard;
        FirstMovers = new HashSet<int>();
        ValidMovesMap = new Dictionary<int, ulong>(); // will set later

        // set first moves
        if (startingBitboard != 0)
            for (int i = 0; i < 64; i++)
                if ((startingBitboard & (BitOps.a1 << i)) != 0)
                    FirstMovers.Add(i);
            
        
    }

    public PieceBoard(PieceBoard original){
        IsWhite = original.IsWhite;
        Bitboard = original.Bitboard;
        FirstMovers = new HashSet<int>(original.FirstMovers);
        ValidMovesMap = new Dictionary<int, ulong>(original.ValidMovesMap);
    }

    public abstract PieceBoard Clone();

    public abstract void ResetValidMoves(ulong friendBoard, int index, ulong enemyBoard = 0, bool includeFriends = false);
    protected ulong CheckDirection(ulong friendBoard, ulong enemyBoard, int index, Func<int, int, int> moveFunc, bool includeFriends)
    {
        ulong directionMoves = 0UL;

        for (int i = 1; i < 8; i++)
        {
            int newIndex = moveFunc(index, i);
            if(Type=='B')Debug.Log(!BitOps.IsValidMove(index, newIndex) + " cant moving this direction" + index +" to"+newIndex);
            if (!BitOps.IsValidMove(index, newIndex)) break;

            ulong newBit = BitOps.a1 << newIndex;
             if(Type=='B')Debug.Log(((friendBoard & newBit) != 0 && !includeFriends) + " " +((enemyBoard & newBit) != 0 || (friendBoard & newBit) != 0 && includeFriends));

             if(Type=='B')Debug.Log((((friendBoard | enemyBoard) & newBit)==0) + " " + ((enemyBoard & newBit) != 0) + " " + ((friendBoard & newBit) != 0 && includeFriends));
             
            if ((friendBoard & newBit) != 0 && !includeFriends) break; // Blocked by friendly piece
            if ( (enemyBoard & newBit)!=0  || ((friendBoard & newBit)!=0 && includeFriends)) 
            {
                directionMoves |= newBit; // Add capture move
                break; // Stop if occupied
            }
            directionMoves |= newBit; //(friendBoard | enemyBoard) & newBit)==0 
        }

        return directionMoves;
    }
    public virtual bool CanMove(int fromIndex, int toIndex){
        // Use the IsValidMove method to check the move
        return BitOps.IsValidMove(fromIndex, toIndex);
    }
 
    public void Move(int fromIndex, int toIndex){
        Bitboard &= ~(BitOps.a1 << fromIndex);
        Bitboard |= (BitOps.a1 << toIndex);

        ValidMovesMap.Remove(fromIndex); // the piece is no longer there

        if (FirstMovers.Contains(fromIndex))
            FirstMovers.Remove(fromIndex);
    }

    public void RemovePiece(int index){
         // Clear the bit corresponding to the index, effectively removing the piece
        Bitboard &= ~(BitOps.a1 << index);
        
        // Optionally, you might want to clean up any other related data structures
        // For instance, if you're tracking the position of pieces in a dictionary, you could remove that entry
        if (FirstMovers.Contains(index))
            FirstMovers.Remove(index); // Remove any first move tracking for the captured piece

        // remove moves for removed piece
        ValidMovesMap.Remove(index);

        // Log or handle any additional cleanup as necessary
        Debug.Log($"Removed piece from index {index}. Remaining bitboard: {Bitboard}");
    }

    public static void PrintBitboard(ulong bitboard){
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
