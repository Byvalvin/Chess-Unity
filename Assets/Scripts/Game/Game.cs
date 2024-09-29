using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour{
    private Board board;
    private Player[] players = new Player[2]; // only 2 players for a chess game
    private int currentIndex = 0;
    private Piece selectedPiece = null, lastMovedPiece = null; // To track the last moved piece;
    Vector2Int originalPosition;
    private bool checkmate = false;

    public void SwitchPlayer()=>currentIndex = (currentIndex + 1) % players.Length;

    // Game ends
    bool IsGameEnd()
    {
        foreach (Player player in players){ // ends when a player is in double check and cant move the king OR a player is in check and cant evade, capture attacker or block check path
            Piece PlayerKing = player.GetKing();
            if(player.IsInCheck()){
                if(player.DoubleCheck){
                    if(PlayerKing.ValidMoves.Count==0){
                        Debug.Log($"GAME OVER:{player.PlayerName} IS DOUBLE CHECKMATED");
                        return true;
                    }
                }
                else if(player.InCheck){
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
    public HashSet<Vector2Int> GetMovesAllowed(Piece piece){ // using the game constraints to get moves allowed
        HashSet<Vector2Int> pieceMoves = FilterMoves(piece), gameValidMoves=new HashSet<Vector2Int>();

        // add enPassantMove for checking
        bool isAnEnPassantMove = lastMovedPiece!=null 
                && piece.Type=="Pawn" && lastMovedPiece.Type=="Pawn" 
                && Mathf.Abs(lastMovedPiece.Position.x-piece.Position.x)==1
                && lastMovedPiece.Position.y==piece.Position.y
                && (lastMovedPiece as Pawn).CanBeCapturedEnPassant;
        if(isAnEnPassantMove){
            Vector2Int enPassantMove = lastMovedPiece.Position+new Vector2Int(0, currentIndex==0 ? -1:1);
            pieceMoves.Add(enPassantMove);
        }

        bool isKing = piece is King;
        foreach (Vector2Int move in pieceMoves){
            // condition 1
            bool mustMoveKing = players[currentIndex].DoubleCheck && isKing;

            // condition 2
            Piece kingAttacker = players[currentIndex].KingAttacker;
            bool canEvade=isKing, // move king
                canCapture=kingAttacker!=null && (kingAttacker.Position==move || isAnEnPassantMove), // cap attacker
                canBlock=kingAttacker!=null && (Utility // can block
                    .GetIntermediateLinePoints(kingAttacker.Position, players[currentIndex].GetKing().Position)
                    .Contains(move)); 
            bool mustAvoidCheck = players[currentIndex].InCheck && (canEvade || canCapture || canBlock);


            // condition 3: cant move a pinned piece
            bool pinnedPiece = false, pinnedPieceCanCaptureAttacker = false;
            Piece attacker = GetAttacker(piece); // selected piece is attacked
            if(attacker!=null){
                HashSet<Vector2Int> tilesBetweenKingAndAttacker = Utility.GetIntermediateLinePoints(players[currentIndex].GetKing().Position, attacker.Position);
                pinnedPiece = tilesBetweenKingAndAttacker.Contains(piece.Position);
                HashSet<Vector2Int> allowedPinnedPieceMoves = tilesBetweenKingAndAttacker; // because a pinned piece can still attack
                allowedPinnedPieceMoves.Add(attacker.Position);
                pinnedPieceCanCaptureAttacker = allowedPinnedPieceMoves.Contains(move);
            }
            bool avoidPinTactic = !pinnedPiece || pinnedPieceCanCaptureAttacker;

            //Debug.Log(mustMoveKing + " " + mustAvoidCheck + " " + avoidPinTactic + " " + isAnEnPassantMove + " ");
            if(mustMoveKing || mustAvoidCheck || (!players[currentIndex].IsInCheck() && avoidPinTactic) || isAnEnPassantMove)
                gameValidMoves.Add(move);
            
        }

        return gameValidMoves;
    }
    HashSet<Vector2Int> GetAllPlayerMoves(Player player){
        HashSet<Vector2Int> allMoves = new HashSet<Vector2Int>();
        foreach (Piece piece in player.Pieces)
            foreach (Vector2Int move in piece.ValidMoves)
                allMoves.Add(move);
        return allMoves;
    }

    // JUST FOR POSITIONS THE OPPOSING PLAYER PIECES ARE ATTACKING, not necessarily defended positions(same as defended positons only for pawns)
    HashSet<Vector2Int> GetAllPlayerAttackMoves(Player player){
        HashSet<Vector2Int> allMoves = new HashSet<Vector2Int>();
        foreach (Piece piece in player.Pieces)
        {
            bool isPawn = piece.Type=="Pawn";
            if(isPawn)
                allMoves.UnionWith(PawnAttackedTiles(piece)); // add the squares attacked by the Pawn, Pawn fwd moves not included here
            else
                foreach (Vector2Int move in piece.ValidMoves)
                    allMoves.Add(move); 
        }
        return allMoves;
    }
    HashSet<Vector2Int> PawnAttackedTiles(Piece piece){
        HashSet<Vector2Int> attackedTiles = new HashSet<Vector2Int>();
        Vector2Int left = piece.Colour?  new Vector2Int(1,-1) : new Vector2Int(-1,1),
                right = piece.Colour? new Vector2Int(-1,-1) : new Vector2Int(1,1);
        if(board.InBounds(piece.Position+left)) attackedTiles.Add(piece.Position+left);
        if(board.InBounds(piece.Position+right)) attackedTiles.Add(piece.Position+right);
        return attackedTiles;
    }
    HashSet<Vector2Int> KingAttackedTiles(Piece piece){
        HashSet<Vector2Int> attackedTiles, allAttackedTiles = Utility.GetSurroundingPoints(piece.Position);
        attackedTiles = Utility.FindAll<Vector2Int>(allAttackedTiles,board.InBounds);
        return attackedTiles;
    }
    HashSet<Vector2Int> KnightAttackedTiles(Piece piece){
        HashSet<Vector2Int> attackedTiles = new HashSet<Vector2Int>();
        return attackedTiles;
    }
    
    void Opposition(){
        // Find common elements
        HashSet<Vector2Int> KingWhiteMoves = players[0].GetKing().ValidMoves,
                    KingBlackMoves = players[1].GetKing().ValidMoves;
        HashSet<Vector2Int> common = new HashSet<Vector2Int>(KingWhiteMoves);
        common.IntersectWith(KingBlackMoves);

        // Remove common elements from both sets
        foreach (var move in common)
        {
            KingWhiteMoves.Remove(move);
            KingBlackMoves.Remove(move);
        }
        players[0].GetKing().ValidMoves = KingWhiteMoves;
        players[1].GetKing().ValidMoves = KingBlackMoves;
    }
    bool FilterPawnMove(Piece piece, Vector2Int pos){

        bool pieceAtpos = board.GetTile(pos).HasPiece(),
            sameColourPieceAtPos = pieceAtpos && board.GetTile(pos).piece.Colour == piece.Colour,
            isDiag = Mathf.Abs(piece.Position.x - pos.x)==1,
            isDoubleMove = Mathf.Abs(piece.Position.y - pos.y)==2; // also pawns cant jump pieces
        HashSet<Vector2Int> tilesBetween = Utility.GetIntermediateLinePoints(piece.Position, pos);

        bool pieceBetween = false;
        foreach (Vector2Int tilePos in tilesBetween){
            pieceBetween = board.GetTile(tilePos).HasPiece();
            if(pieceBetween)
                break;
        }
        return (pieceAtpos && !sameColourPieceAtPos && isDiag) || (!pieceAtpos && !isDiag && (!isDoubleMove || isDoubleMove && !pieceBetween));
    }
    HashSet<Vector2Int> FilterPawnMoves(Piece piece){
        if (piece == null) return null; // don't even bother
        HashSet<Vector2Int> pawnMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
            if (FilterPawnMove(piece, move))
                pawnMoves.Add(move);
        return pawnMoves;
    }
    bool FilterKnightMove(Vector2Int pos){
        return false; // Implement actual logic as needed
    }
    HashSet<Vector2Int> FilterKnightMoves(Piece piece){
        if (piece == null) return null; // don't even bother
        HashSet<Vector2Int> knightMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
            if (FilterKnightMove(move))
                knightMoves.Add(move);
        return knightMoves;
    }
    bool FilterBishopMove(Piece piece, Vector2Int pos){
        bool pieceAtpos = board.GetTile(pos).HasPiece(),
            sameColourPieceAtPos = pieceAtpos && board.GetTile(pos).piece.Colour == piece.Colour;
        HashSet<Vector2Int> pointsBetween = Utility.GetIntermediatePoints(piece.Position, pos, Utility.MovementType.Diagonal);
        foreach (Vector2Int apos in pointsBetween)
        {
            bool pieceAtApos = board.GetTile(apos).HasPiece(),
                sameColourPieceAtAPos = pieceAtApos && board.GetTile(apos).piece.Colour==piece.Colour;
            bool isAttackingKingTile = pieceAtApos && board.GetTile(apos).piece.Type=="King" && !sameColourPieceAtAPos;
            if (pieceAtApos && (!isAttackingKingTile || sameColourPieceAtAPos))
                return false;
        }
        return !pieceAtpos || (pieceAtpos && !sameColourPieceAtPos);
    }
    HashSet<Vector2Int> FilterBishopMoves(Piece piece){
        if (piece == null) return null; // don't even bother
        HashSet<Vector2Int> bishopMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
            if (FilterBishopMove(piece, move))
                bishopMoves.Add(move);
        return bishopMoves;
    }
    bool FilterRookMove(Vector2Int pos){
        return false; // Implement actual logic as needed
    }
    HashSet<Vector2Int> FilterRookMoves(Piece piece){
        if (piece == null) return null; // don't even bother
        HashSet<Vector2Int> rookMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
            if (FilterRookMove(move))
                rookMoves.Add(move);
        return rookMoves;
    }

    bool FilterQueenMove(Vector2Int pos){
        return false; // Implement actual logic as needed
    }
    HashSet<Vector2Int> FilterQueenMoves(Piece piece){
        if (piece == null) return null; // don't even bother
        HashSet<Vector2Int> queenMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
            if (FilterQueenMove(move))
                queenMoves.Add(move);
        return queenMoves;
    }
    bool FilterKingMove(Piece piece, Vector2Int pos){
        bool pieceAtpos = board.GetTile(pos).HasPiece(),
            sameColourPieceAtPos = pieceAtpos && board.GetTile(pos).piece.Colour == piece.Colour;
        bool pieceAtposDefended = false; // King cant capture a defended piece
        if(pieceAtpos && !sameColourPieceAtPos){ // check if that piece is defended
            foreach (Piece opposingPiece in players[piece.Colour?1:0].Pieces){
                if(opposingPiece.Position != pos) {//make sure piece isnt "defending" itself lol
                    switch(opposingPiece.Type){
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
                            HashSet<Vector2Int> pointsBetweenAndEnds;
                            switch(opposingPiece.Type){
                                case "Bishop":
                                    pointsBetweenAndEnds = Utility.GetIntermediateDiagonalLinePoints(opposingPiece.Position, pos, includeEnds:true);
                                    break;
                                case "Rook":
                                    pointsBetweenAndEnds = Utility.GetIntermediateNonDiagonalLinePoints(opposingPiece.Position, pos, includeEnds:true);
                                    break;
                                case "Queen":
                                    pointsBetweenAndEnds = Utility.GetIntermediateLinePoints(opposingPiece.Position, pos, includeEnds:true);
                                    break;
                                default:
                                    pointsBetweenAndEnds = new HashSet<Vector2Int>();
                                    break;
                            }
                            
                            pieceAtposDefended = pointsBetweenAndEnds.Count != 0; // if set is empty, then opposingPiece is not a defender
                            if(pointsBetweenAndEnds.Count > 2){ // if there is a defender then only do this check if there are tiles between the defender and defended
                                pointsBetweenAndEnds.Remove(opposingPiece.Position); pointsBetweenAndEnds.Remove(pos);
                                foreach (Vector2Int point in pointsBetweenAndEnds){
                                    pieceAtposDefended = pieceAtposDefended && !board.GetTile(point).HasPiece(); // if a single piece on path, path is blocked and piece cant be defended
                                    if(!pieceAtposDefended)
                                        break; // there is another piece blocking the defense, onto next candidate
                                }
                            }
                            break;
                    }         
                }
                if(pieceAtposDefended) // piece is defended by one other piece already so can stop
                    break;
            }
        }
        return (!pieceAtpos || (pieceAtpos && !sameColourPieceAtPos && !pieceAtposDefended));
    }
    HashSet<Vector2Int> FilterKingMoves(Piece piece){
        if (piece == null) return null; // don't even bother
        HashSet<Vector2Int> kingMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves){
            if (FilterKingMove(piece, move))
                kingMoves.Add(move);
        }
        return kingMoves;
    }

    public HashSet<Vector2Int> FilterMoves(Piece piece){
        if (piece == null) return null; // no piece was passed
        switch (piece.Type){
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

    private Piece GetAttacker(Piece piece)=>players[piece.Colour ? 1 : 0].Pieces.Find(p => p.ValidMoves.Contains(piece.Position));
    private void UpdateKingAttack(Piece king){
        HashSet<Vector2Int> opposingMoves = GetAllPlayerAttackMoves(players[king.Colour ? 1:0]);
        HashSet<Vector2Int> kingMoves = new HashSet<Vector2Int>();
        foreach(Vector2Int move in king.ValidMoves){
            if(!opposingMoves.Contains(move))
                kingMoves.Add(move);
        }
        king.ValidMoves = kingMoves;
    }
    private void UpdateCheckStatus(Player player){
        Piece king = player.GetKing();
        if (king == null) return;

        HashSet<Vector2Int> opposingMoves = GetAllPlayerAttackMoves(players[player.Colour ? 1 : 0]);

        // Check how many opposing pieces can attack the king
        int attackingPiecesCount = 0;
        foreach (var move in opposingMoves){
            if (move == king.Position){
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
    }

    private void UpdateGameState(){
        // Reset and filter valid moves for each piece
        foreach (Player player in players){
            foreach (Piece piece in player.Pieces){
                piece.ResetValidMoves();
                piece.ValidMoves = FilterMoves(piece);

                // Reset en passant status after each move
                if (piece is Pawn pawn)
                    pawn.ResetEnPassant();
                    
            }
        }
        Opposition(); // Update the opposition

        // Check if players are in check
        foreach (Player player in players)
        {
            UpdateCheckStatus(player); //Debug.Log($"After UpdateCheckStatus: {player.PlayerName} InCheck: {player.InCheck}, DoubleCheck: {player.DoubleCheck}");
            UpdateKingAttack(player.GetKing()); // Update King's moves based on opponent pieces
        }

    }

    // Player GUI
    void SelectPiece(){
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        Collider2D collision = Physics2D.OverlapPoint(mousePosition);
        if (collision != null){
            Piece piece = collision.GetComponent<Piece>();
            if (piece != null && players[currentIndex].Colour == piece.Colour){ // only allow selection for the player to play
                selectedPiece = piece; //Select piece
                originalPosition = selectedPiece.Position; // Store original position
            }
        }
    }
    void DragPiece(){
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        if (selectedPiece != null)
            selectedPiece.transform.position = new Vector3(mousePosition.x, mousePosition.y, 0); // move piece with mouse
    }

    void ExecuteMove(Vector2Int targetPosition){
        if (isCapture(targetPosition)){
            Piece captured = board.GetTile(targetPosition).piece;
            players[currentIndex].Capture(captured);
            players[(currentIndex + 1) % 2].RemovePiece(captured);
            captured.Captured = true;
        }
        if (lastMovedPiece is Pawn && lastMovedPiece.Position.x == targetPosition.x){ // Handle en passant
            Vector2Int enPassantTarget = lastMovedPiece.Position + new Vector2Int(0, currentIndex == 0 ? -1 : 1);
            if (targetPosition == enPassantTarget){ //Debug.Log("Execute EnPassant");
                // Remove the pawn that is captured en passant
                Piece captured = board.GetTile(lastMovedPiece.Position).piece;
                players[currentIndex].Capture(captured);
                players[(currentIndex + 1) % 2].RemovePiece(captured);
                captured.Captured = true;
            }
        }
        board.MovePiece(selectedPiece.Position, targetPosition);
        selectedPiece.Move(targetPosition);
        lastMovedPiece = selectedPiece; // Store the last moved piece
        UpdateGameState();
        SwitchPlayer();
        if(IsGameEnd())
            End();
    }
    bool isCapture(Vector2Int targetPosition) => board.GetTile(targetPosition).HasPiece();

    void BotMove() {
        Vector2Int[] fromTo = players[currentIndex].GetMove();
        Vector2Int fromPosition = fromTo[0];
        Vector2Int targetPosition = fromTo[1];

        // Ensure the piece being moved is valid
        Piece pieceToMove = board.GetTile(fromPosition).piece;
        selectedPiece = pieceToMove;
        if (selectedPiece != null && selectedPiece.Colour == players[currentIndex].Colour) {
            ExecuteMove(targetPosition);
        
        }
        selectedPiece = null; // Deselect the piece after moving
    }
    void ReleasePiece(){
        Vector2Int targetPosition = players[currentIndex].GetMove()[1]; // non-bot players will use GUI so no need for from position
        HashSet<Vector2Int> gameValidMoves = GetMovesAllowed(selectedPiece);
        if(gameValidMoves.Contains(targetPosition))
            ExecuteMove(targetPosition);
        else
            selectedPiece.Position = originalPosition; // Reset to original
        selectedPiece = null; // Deselect piece
    }
    void HandleDragAndDrop(){
        if (selectedPiece != null){
            DragPiece();
            if (Utility.MouseUp())
                ReleasePiece();
        }
    }

    private void HandleInput(){
        if(checkmate) return; // dont handl user input

        if(players[currentIndex] is Bot){
            BotMove();
            return;
        }

        if (Utility.MouseDown()) // Left mouse button
            SelectPiece();
        else if (selectedPiece != null)
            HandleDragAndDrop();
    }

    void Awake(){
        //Player P1 = gameObject.AddComponent<Player>(), P2 = gameObject.AddComponent<Player>();
        Player P1 = gameObject.AddComponent<Player>(), P2 = gameObject.AddComponent<Randi>();
        P1.PlayerName = "P1"; P2.PlayerName = "P2";
        P1.Colour = true; P2.Colour = false;

        if(P1 is Bot)
            (P1 as Bot).CurrentGame = this;
        if(P2 is Bot)
            (P2 as Bot).CurrentGame = this;

        players[0] = P1; players[1] = P2;

        board = gameObject.AddComponent<Board>();
        board.CreateBoard(P1, P2);
    }

    // Start is called before the first frame update
    void Start(){
        UpdateGameState();
    }
    // Update is called once per frame
    void Update(){
        HandleInput();
    }
}
