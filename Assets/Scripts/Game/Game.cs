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

    private bool checkmate = false;

    public void SwitchPlayer()
    {
        currentIndex = (currentIndex + 1) % players.Length;
    }

    // Game ends
    bool IsGameEnd()
    {
        // ends when a player is in double check and cant move the king OR a player is in check and cant evade, capture attacker or block check path
        foreach (Player player in players)
        {
            Piece PlayerKing = player.Pieces[0];
            if(player.IsInCheck()){

                if(player.DoubleCheck)
                {
                    if(PlayerKing.ValidMoves.Count==0){
                        Debug.Log($"GAME OVER:{player.PlayerName} IS DOUBLE CHECKMATED");
                        return true;
                    }
                }
                else if(player.InCheck)
                {
                    bool evade = PlayerKing.ValidMoves.Count != 0,
                        capture = GetAllPlayerAttackMoves(player).Contains(player.KingAttacker.Position);
                    
                    HashSet<Vector2Int> blockingMoves = GetAllPlayerMoves(player);
                    blockingMoves.IntersectWith(Utility.GetIntermediateLinePoints(PlayerKing.Position, player.KingAttacker.Position, includeEnds:true));
                    bool block = blockingMoves.Count != 0;
                    
                    if(!(evade || capture || block)){
                        Debug.Log($"GAME OVER:{player.PlayerName} IS CHECKMATED");
                        return true;
                    }
                    

                }
            }
        }

        return false;
    }

    void End()
    {
        checkmate=true;
    }

    // Piece Movement Logic
    HashSet<Vector2Int> GetAllPlayerMoves(Player player)
    {
        HashSet<Vector2Int> allMoves = new HashSet<Vector2Int>();
        foreach (Piece piece in player.Pieces)
            foreach (Vector2Int move in piece.ValidMoves)
                allMoves.Add(move);

        return allMoves;
    }

    // JUST FOR POSITIONS THE OPPOSING PLAYER PIECES ARE ATTACKING, not necessarily defended positions(same as defended positons only for pawns)
    HashSet<Vector2Int> GetAllPlayerAttackMoves(Player player)
    {
        HashSet<Vector2Int> allMoves = new HashSet<Vector2Int>();
        foreach (Piece piece in player.Pieces)
        {
            bool isPawn = piece.Type=="Pawn";
            //Debug.Log(piece.Type+" "+piece.Colour);
            if(isPawn)
            {
                // add the squares attacked by the Pawn, Pawn fwd moves not included here
                allMoves.UnionWith(PawnAttackedTiles(piece));
            }
            else
            {
                foreach (Vector2Int move in piece.ValidMoves)
                    allMoves.Add(move);     
            }
            /*
            foreach (var item in allMoves)
            {
                Debug.Log(item + " in all moves");
            }
            */

        }
        return allMoves;
    }
    HashSet<Vector2Int> PawnAttackedTiles(Piece piece)
    {
        HashSet<Vector2Int> attackedTiles = new HashSet<Vector2Int>();
        Vector2Int left = piece.Colour?  new Vector2Int(1,-1) : new Vector2Int(-1,1),
                right = piece.Colour? new Vector2Int(-1,-1) : new Vector2Int(1,1);
        
        if(board.InBounds(piece.Position+left)) attackedTiles.Add(piece.Position+left);
        if(board.InBounds(piece.Position+right)) attackedTiles.Add(piece.Position+right);

        return attackedTiles;
    }
    HashSet<Vector2Int> KingAttackedTiles(Piece piece)
    {
        HashSet<Vector2Int> attackedTiles, allAttackedTiles = Utility.GetSurroundingPoints(piece.Position);

        attackedTiles = Utility.FindAll<Vector2Int>(allAttackedTiles,board.InBounds);

        return attackedTiles;
    }
    HashSet<Vector2Int> KnightAttackedTiles(Piece piece)
    {
        HashSet<Vector2Int> attackedTiles = new HashSet<Vector2Int>();
        return attackedTiles;
    }
    
    void Opposition()
    {
        // Find common elements
        HashSet<Vector2Int> KingWhiteMoves = players[0].Pieces[0].ValidMoves,
                    KingBlackMoves = players[1].Pieces[0].ValidMoves;
        HashSet<Vector2Int> common = new HashSet<Vector2Int>(KingWhiteMoves);
        common.IntersectWith(KingBlackMoves);

        // Remove common elements from both sets
        foreach (var move in common)
        {
            KingWhiteMoves.Remove(move);
            KingBlackMoves.Remove(move);
        }
        players[0].Pieces[0].ValidMoves = KingWhiteMoves;
        players[1].Pieces[0].ValidMoves = KingBlackMoves;

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
            bool pieceAtApos = board.GetTile(apos).HasPiece();
            bool isAttackingKingTile = pieceAtApos && board.GetTile(apos).piece.Type=="King" && !board.GetTile(apos).piece.Colour==piece.Colour;
            if (pieceAtApos && !isAttackingKingTile)
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

        bool pieceAtposDefended = false; // King cant capture a defended piece
        if(pieceAtpos && !sameColourPieceAtPos) // check if that piece is defended
        {
            foreach (Piece opposingPiece in players[piece.Colour?1:0].Pieces)
            {
                switch(opposingPiece.Type)
                {
                    case "King":
                        pieceAtposDefended = KingAttackedTiles(opposingPiece).Contains(pos);
                        break;
                    case "Knight":
                        pieceAtposDefended = KnightAttackedTiles(opposingPiece).Contains(pos);
                        break;
                    case "Pawn":
                        pieceAtposDefended = PawnAttackedTiles(opposingPiece).Contains(pos);
                        break;
                    default: // Queen, Rook, Bishop
                        HashSet<Vector2Int> pointsBetweenAndEnds = Utility.GetIntermediateLinePoints(opposingPiece.Position, pos, includeEnds:true);
                        pieceAtposDefended = pointsBetweenAndEnds.Contains(pos);
                        if(pointsBetweenAndEnds.Count > 2) // there will always be two because of the ends
                            foreach (Vector2Int point in pointsBetweenAndEnds)
                            {
                            pieceAtposDefended = pieceAtposDefended && !board.GetTile(pos).HasPiece(); // if a single piece on path, path is blocked and piece cant be defended
                            }
                        break;
                    
                }
                /*
                if(pieceAtposDefended){
                    Debug.Log(pos + " " + pieceAtposDefended + " " + piece.Type);
                    break;
                }
                */

                
            }
        }

        return (!pieceAtpos || (pieceAtpos && !sameColourPieceAtPos && !pieceAtposDefended));
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
        
        Debug.Log("KingMoves "+piece.Colour);
        foreach (var item in kingMoves)
        {
            Debug.Log(item) ; 
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

    private Piece GetAttacker(Piece piece)
    {
        return players[piece.Colour ? 1 : 0].Pieces.Find(p => p.ValidMoves.Contains(piece.Position));
    }
    private void UpdateKingAttack(Piece king)
    {
        // Debug.Log("Check King "+king.Type + " " + king.Colour);
        HashSet<Vector2Int> opposingMoves = GetAllPlayerAttackMoves(players[king.Colour ? 1:0]);

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

    private void UpdateCheckStatus(Player player)
    {
        Piece king = player.Pieces[0];
        if (king == null) return;

        HashSet<Vector2Int> opposingMoves = GetAllPlayerAttackMoves(players[player.Colour ? 1 : 0]);

        // Check how many opposing pieces can attack the king
        int attackingPiecesCount = 0;
        foreach (var move in opposingMoves)
        {
            if (move == king.Position)
            {
                attackingPiecesCount++;
                // Find the attacking piece
                Piece attacker = GetAttacker(king);
                if (attacker != null) 
                    player.KingAttacker = attacker; // Set the attacker
                
            }
            if(attackingPiecesCount >= 2)
                break;
        }

        // Update player's check status
        player.InCheck = attackingPiecesCount == 1;
        player.DoubleCheck = attackingPiecesCount > 1;

        //Debug.Log($"Updating check status for {player.PlayerName}. InCheck: {player.InCheck}, DoubleCheck: {player.DoubleCheck}");
    }


    private void UpdateGameState()
    {
        // Reset and filter valid moves for each piece
        foreach (Player player in players)
        {
            foreach (Piece piece in player.Pieces)
            {
                piece.ResetValidMoves();
                piece.ValidMoves = FilterMoves(piece);
            }
        }

        Opposition(); // Update the opposition

        // Check if players are in check
        foreach (Player player in players)
        {
            //Debug.Log($"Before UpdateCheckStatus: {player.PlayerName} InCheck: {player.InCheck}, DoubleCheck: {player.DoubleCheck}");
            UpdateCheckStatus(player);
            //Debug.Log($"After UpdateCheckStatus: {player.PlayerName} InCheck: {player.InCheck}, DoubleCheck: {player.DoubleCheck}");
            UpdateKingAttack(player.Pieces[0]); // Update King's moves based on opponent pieces
        }

        /*
        Debug.Log("Player: "+ players[0].PlayerName);
        foreach(Piece piece in players[0].Pieces)
        {
            Debug.Log(piece);
            foreach (var item in piece.ValidMoves)
            {
                Debug.Log(item);
            }
            Debug.Log("----------------------");
        }
        Debug.Log("|||||||||||||||||||||||||||||");
        Debug.Log("Player: "+ players[1].PlayerName);
        foreach(Piece piece in players[1].Pieces)
        {
            Debug.Log(piece);
            foreach (var item in piece.ValidMoves)
            {
                Debug.Log(item);
            }
            Debug.Log("----------------------");
        }
        */

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

    void ExecuteMove(Vector2Int targetPosition)
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
        //Debug.Log($"Before UpdateGameState: {players[1 - currentIndex].PlayerName} InCheck: {players[1 - currentIndex].InCheck}, DoubleCheck: {players[1 - currentIndex].DoubleCheck}");
        UpdateGameState();
        //Debug.Log($"After UpdateGameState: {players[1 - currentIndex].PlayerName} InCheck: {players[1 - currentIndex].InCheck}, DoubleCheck: {players[1 - currentIndex].DoubleCheck}");

        SwitchPlayer();
        
        // can do game over Logic here actually
        // if DoubleChecked opponent
        // check if they can move
        // else if SingleChecked opponent
        // check if they can move
        // if cant move just end game here
        if(IsGameEnd())
            End();
    }

    bool isCapture(Vector2Int targetPosition) => board.GetTile(targetPosition).HasPiece();
    void ReleasePiece()
    {
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        Vector2Int targetPosition = Utility.RoundVector2(mousePosition / board.TileSize);
        HashSet<Vector2Int> validMoves = FilterMoves(selectedPiece);

        /*
        foreach (Vector2Int move in validMoves)
        {
            Debug.Log(move);
        }
        */
        //Debug.Log($"{players[1-currentIndex].PlayerName} in check after move attempt: {players[1-currentIndex].InCheck}");
        //Debug.Log(players[currentIndex].PlayerName + "in check "+players[currentIndex].IsInCheck() + " " + players[currentIndex].InCheck + " " + players[currentIndex].DoubleCheck);



        if(players[currentIndex].IsInCheck()) //GetAllPlayerAttackMoves(players[1-currentIndex]).Contains(players[currentIndex].Pieces[0].Position)
        {
            //Double Check
            if(players[currentIndex].DoubleCheck)
            {
                // move king
                if(selectedPiece.Type=="King" && validMoves.Contains(targetPosition) )
                {
                    ExecuteMove(targetPosition);
                }
                else
                {
                    selectedPiece.Position = originalPosition; // Reset to original position 
                }
            }
            //Single Check
            else if(players[currentIndex].InCheck)
            {
                bool canEvade=selectedPiece.Type=="King", // move king
                    canCapture=players[currentIndex].KingAttacker.Position==targetPosition, // cap attacker
                    canBlock=Utility.GetIntermediateLinePoints(players[currentIndex].KingAttacker.Position,players[currentIndex].Pieces[0].Position)
                        .Contains(targetPosition); // can block

                if( validMoves.Contains(targetPosition) && (canEvade || canCapture || canBlock))
                {
                    ExecuteMove(targetPosition);
                }
                else
                {
                    selectedPiece.Position = originalPosition; // Reset to original position
                }
            }
            else{
                selectedPiece.Position = originalPosition; // Reset to original position
            } 

        }
        else
        {
            // cant move a pinned piece
            bool pinnedPiece = false; // assume selectePiece not pinned
            bool pinnedPieceCanCaptureAttacker = false;

            Piece attacker = GetAttacker(selectedPiece); // selected piece is attacked
            if(attacker!=null){
                HashSet<Vector2Int> tilesBetweenKingAndAttacker = Utility.GetIntermediateLinePoints(players[currentIndex].Pieces[0].Position, attacker.Position);
                pinnedPiece = tilesBetweenKingAndAttacker.Contains(selectedPiece.Position);

                // because a pinned piece can still attack
                HashSet<Vector2Int> allowedPinnedPieceMoves = tilesBetweenKingAndAttacker;
                allowedPinnedPieceMoves.Add(attacker.Position);
                pinnedPieceCanCaptureAttacker = allowedPinnedPieceMoves.Contains(targetPosition);
            }

            if(pinnedPiece && !pinnedPieceCanCaptureAttacker)
            {
                //Debug.Log("pin");
                selectedPiece.Position = originalPosition; // Reset to original position 
            }
            else
            {
                if (validMoves.Contains(targetPosition))
                {
                    ExecuteMove(targetPosition);
                }
                else{
                    selectedPiece.Position = originalPosition; // Reset to original position
                } 
            }

            
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

    private void HandleInput()
    {
        if(checkmate) return; // dont handl user input

        if (Utility.MouseDown()) // Left mouse button
        {
            SelectPiece();
        }
        else if (selectedPiece != null)
        {
            HandleDragAndDrop();
        }
    }

    void Awake()
    {
        Player P1 = gameObject.AddComponent<Player>(), P2 = gameObject.AddComponent<Player>();
        P1.PlayerName = "P1"; P2.PlayerName = "P2";
        P1.Colour = true; P2.Colour = false;

        players[0] = P1;
        players[1] = P2;

        board = gameObject.AddComponent<Board>();
        board.CreateBoard(P1, P2);
        
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateGameState();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }
}
