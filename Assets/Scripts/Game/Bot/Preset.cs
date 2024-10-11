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
        {'P', "Pawn"}, // will manual add P for explicit pawn notation for none forward pawn moves
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

        if (move.Contains("=")) return HandlePromotionMove(move);
        if (move == "O-O" || move == "O-O-O") return HandleCastlingMove(move);
        if (move.Length == 2) return HandlePawnMove(move);
        if (move.Length == 3) return HandleSimplePieceMove(move);
        if (move.Length >= 4) return HandleSpecialMove(move);
        return null; // Invalid move
    }



    private Vector2Int[] HandleSpecialMove(string move)
    {
        string processedMove = ProcessCapture(move);
        processedMove=ExplicitPawnMove(processedMove);
        if(processedMove.Length >= 6 && processedMove.Contains('-')) return HandleDisamiguationMove(processedMove);
    
        if (processedMove.Length == 3) return HandleSimplePieceMove(processedMove);
        if (processedMove.Length == 4) return HandleComplexPieceMove(processedMove);
        
        return null; // No valid capture or special move found
    }

    private Vector2Int[] HandleDisamiguationMove(string move){ // further processing
        string[] fromAndToInfo = move.Split('-');
        string fromInfo = fromAndToInfo[0].Trim(), toInfo = fromAndToInfo[1].Trim();
        char pieceChar = fromInfo[0];

        Vector2Int startPosition = ChessNotationToVector2Int(lastNChars(fromInfo, 2)); // last two characters
        Vector2Int targetPosition = ChessNotationToVector2Int(toInfo); 

        foreach (PieceState piece in PieceStates){
            if (pieceTypeNotationMap[pieceChar]==piece.Type && piece.ValidMoves.Contains(targetPosition) && piece.Position==startPosition)
            {
                Debug.Log("move is disamg"+ new[] { piece.Position, targetPosition });
                return new[] { piece.Position, targetPosition };
            }
        }
        return null; // No valid pawn move found

        
    }
    private Vector2Int[] HandlePromotionMove(string move){
        string[] fromAndToInfo = move.Split('=');
        string fromInfo = fromAndToInfo[0].Trim(), toInfo = fromAndToInfo[1].Trim();
        string processedMove = ProcessCapture(fromInfo);
        
        if(processedMove.Length==2) return HandlePawnMove(processedMove);
        processedMove = ExplicitPawnMove(processedMove);
        return HandleComplexPieceMove(processedMove);

    }

    string ProcessCapture(string move)=> move.Contains('x')? move.Replace("x","") : move;
    string ExplicitPawnMove(string move)=>char.IsLower(move[0])? "P"+move : move;
































    private Vector2Int[] HandleCastlingMove(string move)
    {
        return move switch
        {
            "O-O"=>HandleKingsideCastling(),
            "O-O-O"=>HandleQueensideCastling(),
            _=>null
        };

    }
    private Vector2Int[] HandleKingsideCastling(){
        // Implement kingside castling logic
        Vector2Int castleFrom = new Vector2Int(3, Colour?7:0),
                castleTo = new Vector2Int(1, Colour?7:0);
        if(GetKing().Position==castleFrom && GetKing().ValidMoves.Contains(castleTo))
            return new[]{castleFrom, castleTo};
        return null;
    }
    private Vector2Int[] HandleQueensideCastling(){
        // Implement queenside castling logic
        Vector2Int castleFrom = new Vector2Int(3, Colour?7:0),
                castleTo = new Vector2Int(5, Colour?7:0);
        if(GetKing().Position==castleFrom && GetKing().ValidMoves.Contains(castleTo))
            return new[]{castleFrom, castleTo};
        return null; 
    }
    private Vector2Int[] HandlePawnMove(string move) // 2 char string. fwd pawn move
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

    private Vector2Int[] HandleSimplePieceMove(string move) // 3 char string
    {
        Vector2Int targetPosition = ChessNotationToVector2Int(lastNChars(move,2)); // last 2 chars (starts from index 1 for 3 char string)
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
    private Vector2Int[] HandleComplexPieceMove(string move) // 4 char string
    {
        // need to habdle exd5 AND STUFF TOO FOR THREE PIECES OR MAKE A FOUR PIECE AND TURN exd5 to Ped5
        int startX = 7 - (move[1]-'a');
        Vector2Int targetPosition = ChessNotationToVector2Int(lastNChars(move,2)); // last two characters, (start from index 2 for 4 char string)
        char pieceChar = move[0];
        foreach (PieceState piece in PieceStates)
        {
            if (pieceTypeNotationMap[pieceChar]==piece.Type && piece.ValidMoves.Contains(targetPosition) && piece.Position.x==startX)
            {
                return new[] { piece.Position, targetPosition };
            }
        }
        return null; // No valid piece move found
    }

    private Vector2Int ChessNotationToVector2Int(string notation)
    {
        int x = 7 - (notation[0] - 'a');
        int y = 7 - (notation[1] - '1');
        return new Vector2Int(x, y);
    }
    private string lastNChars(string inputString, int n) => inputString.Substring(inputString.Length-n);
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