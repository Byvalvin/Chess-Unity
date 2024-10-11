using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;


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
        Debug.Log(move.Substring(1));
        Vector2Int targetPosition = ChessNotationToVector2Int(move.Substring(1));
        char pieceChar = move[0];
        Debug.Log(pieceChar + " " + pieceTypeNotationMap[pieceChar]);

        foreach (PieceState piece in PieceStates)
        {
            Debug.Log(pieceChar + " " + pieceTypeNotationMap[pieceChar] + " and " + piece.Type);
            Debug.Log(piece.Position + "====" + targetPosition);
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
        string pieceChar = move[0].ToString();
        Vector2Int targetPosition = ChessNotationToVector2Int(move.Substring(isCapture ? 2 : 1));

        foreach (PieceState piece in PieceStates)
        {
            if (piece.Type.StartsWith(pieceChar, StringComparison.OrdinalIgnoreCase))
            {
                if (isCapture && piece.ValidMoves.Contains(targetPosition) && currentGame.IsCapture(targetPosition))
                {
                    return new[] { piece.Position, targetPosition };
                }
                else if (!isCapture && piece.ValidMoves.Contains(targetPosition))
                {
                    return new[] { piece.Position, targetPosition };
                }
            }
        }
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