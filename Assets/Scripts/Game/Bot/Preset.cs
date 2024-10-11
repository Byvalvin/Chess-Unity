using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

/*
1. Basic Moves
Pawns:
Move: e4 (pawn moves to e4)
Pieces:
Knight moves: Nf3 (knight to f3)
Bishop moves: Bc4 (bishop to c4)

2. Captures
Pawns:
Capture: exd5 (pawn on e captures on d5)
Pieces:
Knight captures: Nxe5 (knight captures on e5)

3. Check and Checkmate
Check:
Rf7+ (rook to f7 with check)
Checkmate:
Qh5# (queen to h5 checkmate)

4. Castling
Kingside Castling:
O-O (king-side castling)
Queenside Castling:
O-O-O (queen-side castling)

5. Promotion
Promotion Example:
e8=Q (pawn promotes to queen on e8)

6. Disambiguation
Two Knights on the Same File:
Knight on b1 moves to c3: Nbc3
Knight on b3 moves to c3: Nbc3

7. Disambiguation with Rank and File
If two knights are on different files:
Knight on b1: N1b1-c3
Knight on d1: N2d1-c3

8. Ambiguous Moves
If a queen can move from either a3 or b3 to c4:
From a3: Qac4
From b3: Qbc4

9. Game Notation
Complete Game Sample:
e4 e5
Nf3 Nc6
Bb5 a6
Ba4 Nf6
O-O Be7
This covers a variety of notation cases in chess, demonstrating the flexibility and detail available in the notation system!
*/
public class PresetState : BotState
{
    private static readonly Dictionary<char, string> pieceTypeNotationMap = new Dictionary<char, string>
    {
        { 'K', "King" },
        { 'Q', "Queen" },
        { 'R', "Rook" },
        { 'B', "Bishop" },
        { 'N', "Knight" },
        // Add other mappings if needed
    };
    private int moveIndex = 0;
    private List<string> MoveList; // List to store move strings

    public PresetState(string _playerName, bool _colour, string filePath) : base(_playerName, _colour)
    {
        MoveList = new List<string>();
        LoadMovesFromFile(filePath);
    }

    public PresetState(PresetState original) : base(original)
    {
        moveIndex = original.moveIndex;
        MoveList = new List<string>(original.MoveList);
    }

    public override Vector2Int[] GetMove()
    {
        if (moveIndex < MoveList.Count)
        {
            string moveString = MoveList[moveIndex]; // Get the next move string
            moveIndex++;
            return ParseMove(moveString); // Parse it into Vector2Int[]
        }
        else
        {
            Debug.Log("No more moves available.");
            return null; // or return a default move
        }
    }

    private void LoadMovesFromFile(string filePath)
    {
        // Load the file and read moves line by line
        string[] lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                var columns = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                string moveString = this.Colour ? columns[0] : columns[1];
                MoveList.Add(moveString.Trim()); // Add the move string directly
            }
        }
    }

    private Vector2Int[] ParseMove(string move)
    {
        bool isCheck = move.EndsWith("+");
        bool isCheckmate = move.EndsWith("#");

        // Remove check/checkmate symbols for parsing
        if (isCheck || isCheckmate)
        {
            move = move.Substring(0, move.Length - 1);
        }

        if (move.Length == 2) return HandlePawnMove(move);
        if (move == "O-O" || move == "O-O-O") return HandleCastlingMove(move);
        if (move.Length == 3) return HandlePieceMove(move);
        if (move.Length == 4) return HandleCaptureOrSpecialMove(move);
        return null; // Invalid move
    }

    private Vector2Int[] HandlePawnMove(string move)
    {
        Vector2Int targetPosition = ChessNotationToVector2Int(move);
        foreach (PieceState piece in PieceStates)
        {
            if (piece.Type == "Pawn" && piece.ValidMoves.Contains(targetPosition))
            {
                Debug.Log("move is "+ new[] { piece.Position, targetPosition });
                return new[] { piece.Position, targetPosition };
            }
        }
        return null; // No valid pawn move found
    }

    private Vector2Int[] HandlePieceMove(string move)
    {
        // need to habdle exd5 AND STUFF TOO FOR THREE PIECES OR MAKE A FOUR PIECE AND TURN exd5 to Ped5
        Vector2Int targetPosition = ChessNotationToVector2Int(move.Substring(1));
        char pieceChar = move[0];
        foreach (PieceState piece in PieceStates)
        {
            if (pieceTypeNotationMap[pieceChar]==piece.Type && piece.ValidMoves.Contains(targetPosition))
            {
                return new[] { piece.Position, targetPosition };
            }
        }
        return null; // No valid piece move found
    }

    private Vector2Int[] HandleCaptureOrSpecialMove(string move)
    {
        bool isCapture = move[1] == 'x';
        // can process and delegat to two or three piece OR FOUR PIECE (Qac4 vs Qbc4(can even inlcue ed5 as Ped5) so not Qxc4),
        /* so if move is castle
                -> return castle moves
            else
            if move has - or movelength >=6 Nb1-c3 vs Nd1-c3 OR N1b1-c3 vs N2d1-c3
                -> return disamguituion
            if move has =
                -> return promotion move
            else
                ->return process move(move reduced to 2,3 or 4 and handled)
        */

        // char pieceChar = move[0];
        // Vector2Int targetPosition = ChessNotationToVector2Int(move.Substring(isCapture ? 2 : 1));

        // foreach (PieceState piece in PieceStates)
        // {
        //     if (pieceTypeNotationMap[pieceChar]==piece.Type)
        //     {
        //         if (isCapture && piece.ValidMoves.Contains(targetPosition) && currentGame.IsCapture(targetPosition))
        //         {
        //             return new[] { piece.Position, targetPosition };
        //         }
        //         else if (!isCapture && piece.ValidMoves.Contains(targetPosition))
        //         {
        //             return new[] { piece.Position, targetPosition };
        //         }
        //     }
        // }

        return null; // No valid capture or special move found
    }

    private Vector2Int[] HandleCastlingMove(string move)
    {
        if (move == "O-O")
        {
            return HandleKingsideCastling();
        }
        else if (move == "O-O-O")
        {
            return HandleQueensideCastling();
        }
        return null; // Not a valid castling move
    }

    private Vector2Int[] HandleKingsideCastling()
    {
        // Implement kingside castling logic
        Vector2Int castleFrom = new Vector2Int(3, Colour?7:0),
                castleTo = new Vector2Int(1, Colour?7:0);
        if(GetKing().Position==castleFrom && GetKing().ValidMoves.Contains(castleTo))
            return new[]{castleFrom, castleTo};
        
        return null; // Placeholder
    }

    private Vector2Int[] HandleQueensideCastling()
    {
        // Implement queenside castling logic
        Vector2Int castleFrom = new Vector2Int(3, Colour?7:0),
                castleTo = new Vector2Int(5, Colour?7:0);
        if(GetKing().Position==castleFrom && GetKing().ValidMoves.Contains(castleTo))
            return new[]{castleFrom, castleTo};

        return null; // Placeholder
    }

    private Vector2Int ChessNotationToVector2Int(string notation)
    {
        int x = 7 - (notation[0] - 'a');
        int y = 7 - (notation[1] - '1');
        return new Vector2Int(x, y);
    }
}



public class Preset : Bot
{
    protected override void Awake()
    {
        //state = new PresetState();
    }
    
    // Start is called before the first frame update
    protected override void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }
}