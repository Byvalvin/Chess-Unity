using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
There are 11 classes: 
Game, Board, Tile, Player, Piece, King, Queen, Rook, Bishop, Knight,
Pawn. The Game is composed with Board, 
The Game is aggregated with Players, 
The Board is aggregated with Tiles, 
Tiles may or may not have Pieces, 
The Player has Pieces and 
King, Queen, Rook, Bishop and Knight inherit from 
Piece(an abstract class)
*/
public class Game : MonoBehaviour
{
    private Board board;
    private Player[] players = new Player[2]; // only 2 players for a chess game
    private int currentIndex = 0;

    private Piece selectedPiece = null;

    Vector2Int originalPosition;

    public void SwitchPlayer()
    {
        currentIndex = (currentIndex + 1) % players.Length;
    }

    // Piece Movement Logic
      HashSet<Vector2Int> GetAllPlayerMoves(Player player)
    {
        HashSet<Vector2Int> allMoves = new HashSet<Vector2Int>();
        foreach (Piece piece in player.Pieces)
        {
            foreach (Vector2Int move in piece.ValidMoves)
            {
                if (!(piece.Type == "Pawn" && piece.Position.x == move.x))
                    allMoves.Add(move);
            }
        }
        return allMoves;
    }
    bool FilterPawnMove(Piece piece, Vector2Int pos)
    {
        bool pieceAtpos = board.GetTile(pos).HasPiece(),
            sameColourPieceAtPos = pieceAtpos && board.GetTile(pos).piece.Colour == piece.Colour,
            isDiag = Mathf.Abs(piece.Position.x - pos.x) == 1;

        return (pieceAtpos && !sameColourPieceAtPos && isDiag) || (!pieceAtpos && !isDiag);
    }
    HashSet<Vector2Int> FilterPawnMoves(Piece piece)
    {
        if (piece == null) return null; // don't even bother

        HashSet<Vector2Int> pawnMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
        {
            if (FilterPawnMove(piece, move))
                pawnMoves.Add(move);
        }
        return pawnMoves;
    }

    bool FilterKnightMove(Vector2Int pos)
    {
        return false; // Implement actual logic as needed
    }
    HashSet<Vector2Int> FilterKnightMoves(Piece piece)
    {
        if (piece == null) return null; // don't even bother

        HashSet<Vector2Int> knightMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
        {
            if (FilterKnightMove(move))
                knightMoves.Add(move);
        }
        return knightMoves;
    }

    bool FilterBishopMove(Piece piece, Vector2Int pos)
    {
        bool pieceAtpos = board.GetTile(pos).HasPiece(),
            sameColourPieceAtPos = pieceAtpos && board.GetTile(pos).piece.Colour == piece.Colour;

        HashSet<Vector2Int> pointsBetween = Utility.GetIntermediatePoints(piece.Position, pos, Utility.MovementType.Diagonal);

        foreach (Vector2Int apos in pointsBetween)
        {
            if (board.GetTile(apos).HasPiece())
            {
                return false;
            }
        }

        return !pieceAtpos || (pieceAtpos && !sameColourPieceAtPos);
    }
    HashSet<Vector2Int> FilterBishopMoves(Piece piece)
    {
        if (piece == null) return null; // don't even bother

        HashSet<Vector2Int> bishopMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
        {
            if (FilterBishopMove(piece, move))
                bishopMoves.Add(move);
        }
        return bishopMoves;
    }

    bool FilterRookMove(Vector2Int pos)
    {
        return false; // Implement actual logic as needed
    }
    HashSet<Vector2Int> FilterRookMoves(Piece piece)
    {
        if (piece == null) return null; // don't even bother

        HashSet<Vector2Int> rookMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
        {
            if (FilterRookMove(move))
                rookMoves.Add(move);
        }
        return rookMoves;
    }

    bool FilterQueenMove(Vector2Int pos)
    {
        return false; // Implement actual logic as needed
    }
    HashSet<Vector2Int> FilterQueenMoves(Piece piece)
    {
        if (piece == null) return null; // don't even bother

        HashSet<Vector2Int> queenMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
        {
            if (FilterQueenMove(move))
                queenMoves.Add(move);
        }
        return queenMoves;
    }

    bool FilterKingMove(Piece piece, Vector2Int pos)
    {
        bool pieceAtpos = board.GetTile(pos).HasPiece(),
            sameColourPieceAtPos = pieceAtpos && board.GetTile(pos).piece.Colour == piece.Colour;

        return (!pieceAtpos || (pieceAtpos && !sameColourPieceAtPos));
    }
    HashSet<Vector2Int> FilterKingMoves(Piece piece)
    {
        if (piece == null) return null; // don't even bother

        HashSet<Vector2Int> kingMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
        {
            if (FilterKingMove(piece, move))
                kingMoves.Add(move);
        }
        return kingMoves;
    }

    public HashSet<Vector2Int> FilterMoves(Piece piece)
    {
        if (piece == null) return null; // no piece was passed

        switch (piece.Type)
        {
            case "King":
                return FilterKingMoves(piece);
            case "Queen":
                return FilterQueenMoves(piece);
            case "Rook":
                return FilterRookMoves(piece);
            case "Knight":
                return FilterKnightMoves(piece);
            case "Bishop":
                return FilterBishopMoves(piece);
            case "Pawn":
                return FilterPawnMoves(piece);
            default:
                Debug.Log("Bryhh"+piece.Type);
                return null;
        }
    }




    // Player GUI
    void SelectPiece()
    {
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        Collider2D collision = Physics2D.OverlapPoint(mousePosition);
        if (collision != null)
        {
            Piece piece = collision.GetComponent<Piece>();
            if (piece != null && players[currentIndex].Colour == piece.Colour) // only allow selection for the player to play
            {
                selectedPiece = piece; //Select piece
                originalPosition = selectedPiece.Position; // Store original position
            }
        }
    }
    void DragPiece()
    {
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        if (selectedPiece != null)
        {
            selectedPiece.transform.position = new Vector3(mousePosition.x, mousePosition.y, 0); // move piece with mouse
        }
    }

    bool isCapture(Vector2Int targetPosition) => board.GetTile(targetPosition).HasPiece();
    void ReleasePiece()
    {
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        Vector2Int targetPosition = Utility.RoundVector2(mousePosition / board.TileSize);
        HashSet<Vector2Int> validMoves = FilterMoves(selectedPiece);

        foreach (Vector2Int move in validMoves)
        {
            Debug.Log(move);
        }
        if (validMoves.Contains(targetPosition))
        {
            if (isCapture(targetPosition))
            {
                Piece captured = board.GetTile(targetPosition).piece;
                players[currentIndex].Capture(captured);
                players[(currentIndex + 1) % 2].RemovePiece(captured);
                captured.Captured = true;
            }

            board.MovePiece(selectedPiece.Position, targetPosition);
            selectedPiece.Move(targetPosition);
            UpdateGameState();
            SwitchPlayer();
        }
        else
        {
            selectedPiece.Position = originalPosition; // Reset to original position
        }
        selectedPiece = null; // Deselect piece
    }
    void HandleDragAndDrop()
    {
        if (selectedPiece != null)
        {
            DragPiece();
            if (Utility.MouseUp())
            {
                ReleasePiece();
            }
        }
    }

    private void UpdateKingAttack(Piece king)
    {
       // Debug.Log("Check King "+king.Type + " " + king.Colour);
        HashSet<Vector2Int> opposingMoves = GetAllPlayerMoves(players[king.Colour ? 1:0]);

        HashSet<Vector2Int> kingMoves = new HashSet<Vector2Int>();
        foreach(Vector2Int move in king.ValidMoves)
        {
            if(!opposingMoves.Contains(move))
            {
                kingMoves.Add(move);
            }
        }
        king.ValidMoves = kingMoves;

    }
    private void UpdateGameState()
    {
        // update all piece moves

        // track kings for special updates
        Piece KingWhite=null, KingBlack=null;

        Debug.Log($"Player {players[1].PlayerName} has {players[1].Pieces.Count} pieces.");

        // P1 pieces
        foreach(Piece piece in players[0].Pieces)
        {
            Debug.Log("Check Piece "+piece+" "+piece.Type+"|");
            if(piece.Type=="King")
            {
                KingWhite=piece;
            }
            piece.ResetValidMoves();
            piece.ValidMoves = FilterMoves(piece);
        }
        // P2 pieces
        foreach(Piece piece in players[1].Pieces)
        {
            if(piece.Type=="King")
            {
                KingBlack=piece;
            }
            piece.ResetValidMoves();
            piece.ValidMoves = FilterMoves(piece);
        }

        // update King moves based on opponent pieces
        UpdateKingAttack(KingWhite);
        UpdateKingAttack(KingBlack);
        
    }

    private void HandleInput()
    {
        if (Utility.MouseDown()) // Left mouse button
        {
            SelectPiece();
        }
        else if (selectedPiece != null)
        {
            HandleDragAndDrop();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Player P1 = gameObject.AddComponent<Player>(), P2 = gameObject.AddComponent<Player>();
        P1.PlayerName = "P1"; P2.PlayerName = "P2";
        P1.Colour = true; P2.Colour = false;

        players[0] = P1;
        players[1] = P2;

        board = gameObject.AddComponent<Board>();
        board.CreateBoard(P1, P2);
        UpdateGameState();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }
}
