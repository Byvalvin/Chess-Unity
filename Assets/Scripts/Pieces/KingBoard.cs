using UnityEngine;
using System;
using System.Linq;


public class KingBoard : PieceBoard
{
    // Predefined castling masks for both white and black
    // private static readonly ulong WhiteKingsideMask = (BitOps.a1 << 5) | (BitOps.a1 << 6); // f1, g1
    // private static readonly ulong WhiteQueensideMask = (BitOps.a1 << 1) | (BitOps.a1 << 2) | (BitOps.a1 << 3); // b1, c1, d1
    // private static readonly ulong BlackKingsideMask = (BitOps.a1 << 61) | (BitOps.a1 << 62); // f8, g8
    // private static readonly ulong BlackQueensideMask = (BitOps.a1 << 57) | (BitOps.a1 << 58) | (BitOps.a1 << 59); // b8, c8, d8

    public static readonly ulong WhiteKingsideMask = 0x0000000000000060,
                                WhiteKingSideMove = 0x0000000000000040; // f1, g1'

    public static readonly ulong WhiteQueensideMask = 0x000000000000000E,
                                WhiteQueenSideMove = 0x0000000000000004; // b1, c1, d1
    public static readonly ulong BlackKingsideMask = 0x6000000000000000,
                                BlackKingSideMove = 0x4000000000000000; // f8, g8
    public static readonly ulong BlackQueensideMask = 0x0E00000000000000,
                                BlackQueenSideMove = 0x0400000000000000; // b8, c8, d8
    
    // Array of movement functions to check
    static readonly Func<int, int, int>[] moveFunctions = new Func<int, int, int>[]
    {
        BitOps.ForwardMove,
        BitOps.BackwardMove,
        BitOps.LeftMove,
        BitOps.RightMove,
        BitOps.Diagonal1Move,
        BitOps.Diagonal2Move,
        BitOps.Diagonal3Move,
        BitOps.Diagonal4Move
    };

    // public int MyIndex{get; private set;}
    public int MyIndex=>BitOps.GetFirstSetBitIndexBSR(Bitboard);


    public KingBoard(bool IsWhite, ulong startingBitboard = 0) : base(IsWhite, startingBitboard)
    {
        Type = 'K';
    }

    public KingBoard(KingBoard original) : base(original) {}

    public override PieceBoard Clone() => new KingBoard(this);

    // public override void ResetValidMoves(ulong friendBoard, int index, ulong enemyBoard){
    //     MyIndex = index;
    //     base.ResetValidMoves(friendBoard, index, enemyBoard);
    // }
  
    public override ulong GetValidMoves(ulong friendBoard, int index, ulong enemyBoard = 0, bool includeFriends = false)
    {
        ulong validMoves = 0UL;

        foreach (var moveFunc in moveFunctions){
            int newIndex = moveFunc(index, 1); // one in each dir
            if (BitOps.IsValidMove(index, newIndex, BitOps.MovementType.King))
            {
                ulong newBit = BitOps.a1 << newIndex;
                if ((friendBoard & newBit) == 0 || includeFriends)
                {
                    validMoves |= newBit;
                }
            }
        }

        if (FirstMovers.Count != 0) // Only add castling moves if king hasn't moved
        {
            ulong occupancyBoard = friendBoard | enemyBoard;

            // Get castling moves
            ulong castlingMoves = GetCastlingMoves(occupancyBoard, friendBoard);

            // add castling moves to validMoves, will filter in gamestate
            validMoves |= castlingMoves;
        }

        return validMoves; // Return all valid moves
    }

    public ulong GetAttackMoves(){ // all valid king attack mvoes so defense is proper
        ulong allKingMoves = 0UL;
        int kingIndex = MyIndex;

        foreach (var moveFunc in moveFunctions){
            int newIndex = moveFunc(kingIndex, 1); // one in each dir
            if (BitOps.IsValidMove(kingIndex, newIndex, BitOps.MovementType.King))
                allKingMoves |= BitOps.a1 << newIndex;    
        }
        return allKingMoves;
    }

    public ulong GetCastlingMoves(ulong occupancyBoard, ulong friendBoard)
    {
        ulong castlingMoves = 0UL;

        castlingMoves |= GetKingsideCastlingMoves(occupancyBoard, friendBoard);
        castlingMoves |= GetQueensideCastlingMoves(occupancyBoard, friendBoard);

        return castlingMoves; // Return the castling moves
    }

    public ulong GetCastlingMoves() =>  IsWhite? (WhiteKingSideMove | WhiteQueenSideMove) : (BlackKingSideMove | BlackQueenSideMove); 
    
    public ulong GetKingsideCastlingMoves(ulong occupancyBoard, ulong friendBoard)
    {
        ulong rookBit = (ulong)(IsWhite ? 0x0000000000000080 : 0x8000000000000000); // h1 or h8
        if ((friendBoard & rookBit) != 0) // Check if the rook is present
        {
            return CheckCastling(occupancyBoard, rookBit, IsWhite? new int[] { 5, 6 } : new int[] { 61, 62 }); // Kingside
        }
        return 0; // No castling moves available
    }

    public ulong GetQueensideCastlingMoves(ulong occupancyBoard, ulong friendBoard)
    {
        ulong rookBit = (ulong)(IsWhite ? 0x0000000000000001 : 0x0100000000000000); // a1 or a8
        if ((friendBoard & rookBit) != 0) // Check if the rook is present
        {
            return CheckCastling(occupancyBoard, rookBit, IsWhite ? new int[] { 1, 2, 3 } : new int[] { 57, 58, 59 }); // Queenside
        }
        return 0; // No castling moves available
    }

    private ulong CheckCastling(ulong occupancyBoard, ulong rookBit, int[] spaces)
    {
        // Check that all spaces between the king and rook are empty
        foreach (int space in spaces)
        {
            if ((occupancyBoard & (BitOps.a1 << space)) != 0)
            {
                //Debug.Log("castle space blocked " + space);
                return 0; // Space is not empty
            }
        }

        // Determine the new position for the king after castling
        ulong newKingPosition = BitOps.a1 << spaces[1]; 

        return newKingPosition; // Return the new king position
    }

    public ulong GetKingsideCastlingMove() => FirstMovers.Count != 0? IsWhite? WhiteKingSideMove : BlackKingSideMove
                                                :
                                                0;
    public ulong GetQueensideCastlingMove() => FirstMovers.Count != 0? IsWhite? WhiteQueenSideMove : BlackQueenSideMove
                                                :
                                                0;
    public ulong GetKingsideCastlingMove(ulong occupancyBoard) => ( occupancyBoard & (IsWhite? WhiteKingsideMask:BlackKingsideMask) )==0? GetKingsideCastlingMove()                
                                                :
                                                0;
    public ulong GetQueensideCastlingMove(ulong occupancyBoard) => ( occupancyBoard & (IsWhite? WhiteQueensideMask:BlackQueensideMask) )==0? GetQueensideCastlingMove()
                                                :
                                                0;

    private ulong CheckKingSideCastling(ulong occupancyBoard) 
        => (occupancyBoard & ~(IsWhite?WhiteKingsideMask:BlackKingsideMask))==0 ? 
                (GetKingsideCastlingMove()) : 0;
    private ulong CheckQueenSideCastling(ulong occupancyBoard) 
        => (occupancyBoard & ~(IsWhite?WhiteQueensideMask:BlackQueensideMask))==0 ? 
                (GetQueensideCastlingMove()) : 0;
    

    

    // private ulong CheckCastling(ulong occupancyBoard, ulong rookBit)
    // {
    //     // Check that all spaces between the king and rook are empty
    //     foreach (int space in spaces)
    //     {
    //         if ((occupancyBoard & (BitOps.a1 << space)) != 0)
    //         {
    //             Debug.Log("castle space blocked " + space);
    //             return 0; // Space is not empty
    //         }
    //     }

    //     // Determine the new position for the king after castling
    //     ulong newKingPosition = BitOps.a1 << spaces[1]; 

    //     return newKingPosition; // Return the new king position
    // }
}




/*
Index 0: 1UL << 0 → 0x0000000000000001
Index 1: 1UL << 1 → 0x0000000000000002
Index 2: 1UL << 2 → 0x0000000000000004
Index 3: 1UL << 3 → 0x0000000000000008
Index 4: 1UL << 4 → 0x0000000000000010
Index 5: 1UL << 5 → 0x0000000000000020
Index 6: 1UL << 6 → 0x0000000000000040
Index 7: 1UL << 7 → 0x0000000000000080
Index 8: 1UL << 8 → 0x0000000000000100
Index 9: 1UL << 9 → 0x0000000000000200
Index 10: 1UL << 10 → 0x0000000000000400
Index 11: 1UL << 11 → 0x0000000000000800
Index 12: 1UL << 12 → 0x0000000000001000
Index 13: 1UL << 13 → 0x0000000000002000
Index 14: 1UL << 14 → 0x0000000000004000
Index 15: 1UL << 15 → 0x0000000000008000
Index 16: 1UL << 16 → 0x0000000000010000
Index 17: 1UL << 17 → 0x0000000000020000
Index 18: 1UL << 18 → 0x0000000000040000
Index 19: 1UL << 19 → 0x0000000000080000
Index 20: 1UL << 20 → 0x0000000000100000
Index 21: 1UL << 21 → 0x0000000000200000
Index 22: 1UL << 22 → 0x0000000000400000
Index 23: 1UL << 23 → 0x0000000000800000
Index 24: 1UL << 24 → 0x0000000001000000
Index 25: 1UL << 25 → 0x0000000002000000
Index 26: 1UL << 26 → 0x0000000004000000
Index 27: 1UL << 27 → 0x0000000008000000
Index 28: 1UL << 28 → 0x0000000010000000
Index 29: 1UL << 29 → 0x0000000020000000
Index 30: 1UL << 30 → 0x0000000040000000
Index 31: 1UL << 31 → 0x0000000080000000
Index 32: 1UL << 32 → 0x0000000100000000
Index 33: 1UL << 33 → 0x0000000200000000
Index 34: 1UL << 34 → 0x0000000400000000
Index 35: 1UL << 35 → 0x0000000800000000
Index 36: 1UL << 36 → 0x0000001000000000
Index 37: 1UL << 37 → 0x0000002000000000
Index 38: 1UL << 38 → 0x0000004000000000
Index 39: 1UL << 39 → 0x0000008000000000
Index 40: 1UL << 40 → 0x0000010000000000
Index 41: 1UL << 41 → 0x0000020000000000
Index 42: 1UL << 42 → 0x0000040000000000
Index 43: 1UL << 43 → 0x0000080000000000
Index 44: 1UL << 44 → 0x0000100000000000
Index 45: 1UL << 45 → 0x0000200000000000
Index 46: 1UL << 46 → 0x0000400000000000
Index 47: 1UL << 47 → 0x0000800000000000
Index 48: 1UL << 48 → 0x0001000000000000
Index 49: 1UL << 49 → 0x0002000000000000
Index 50: 1UL << 50 → 0x0004000000000000
Index 51: 1UL << 51 → 0x0008000000000000
Index 52: 1UL << 52 → 0x0010000000000000
Index 53: 1UL << 53 → 0x0020000000000000
Index 54: 1UL << 54 → 0x0040000000000000
Index 55: 1UL << 55 → 0x0080000000000000
Index 56: 1UL << 56 → 0x0100000000000000
Index 57: 1UL << 57 → 0x0200000000000000
Index 58: 1UL << 58 → 0x0400000000000000
Index 59: 1UL << 59 → 0x0800000000000000
Index 60: 1UL << 60 → 0x1000000000000000
Index 61: 1UL << 61 → 0x2000000000000000
Index 62: 1UL << 62 → 0x4000000000000000
Index 63: 1UL << 63 → 0x8000000000000000
*/