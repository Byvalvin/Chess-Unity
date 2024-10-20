using System.Collections.Generic;
using UnityEngine;

public class PawnBoard : PieceBoard
{
    public PawnBoard(bool isWhite, ulong startingBitboard = 0) : base(isWhite, startingBitboard)
    {
        Type = 'P';
    }

    public PawnBoard(PawnBoard original) : base(original) { }

    public override PieceBoard Clone() => new PawnBoard(this);

    public override HashSet<int> ValidMoves(int index)
    {
        Debug.Log("Valid moves for"+index);
        HashSet<int> validMoves = new HashSet<int>();
        int direction = IsWhite ? 1 : -1;

        // Forward move
        int forwardIndex = BitOps.ForwardMove(index, direction);
        if (BitOps.InBounds(forwardIndex) && ((Bitboard & (BitOps.a1 << forwardIndex)) == 0))
        {
            validMoves.Add(forwardIndex);
        }

        // First move: two squares
        if (!FirstMoveMap.ContainsKey(index) || (FirstMoveMap.ContainsKey(index) && FirstMoveMap[index]))
        {
            int doubleForwardIndex = BitOps.ForwardMove(index, direction * 2);
            Debug.Log(doubleForwardIndex+"dfwd");
            if (BitOps.InBounds(doubleForwardIndex) && ((Bitboard & (BitOps.a1 << doubleForwardIndex)) == 0))
            {
                validMoves.Add(doubleForwardIndex);
            }
        }

        // Diagonal captures
        int leftCaptureIndex = BitOps.Diagonal3Move(index, direction);
        if (BitOps.InBounds(leftCaptureIndex) && ((Bitboard & (BitOps.a1 << leftCaptureIndex)) != 0))
        {
            validMoves.Add(leftCaptureIndex);
        }

        int rightCaptureIndex = BitOps.Diagonal4Move(index, direction);
        if (BitOps.InBounds(rightCaptureIndex) && ((Bitboard & (BitOps.a1 << rightCaptureIndex)) != 0))
        {
            validMoves.Add(rightCaptureIndex);
        }

        foreach (var item in validMoves)
        {
            Debug.Log("pawn move"+item);
        }
        

        return validMoves;
    }
}
