using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json; // for saving and loading games
using System.Linq; // Add this line for LINQ


/*
man. Still project has thought me much to say the least. None the less the value of understand objects, like when they are the same reference, when they are different, when to clone when to just assign, building objects, separation of state from display, botting and how best to approach it, unity deployment, c#, inheritence, virtual and override, abstract classes, interfaces, the utility of utilities, chess notations, Unity UI (is the worst part of unity but is powerful if you know), why GameObject.Find() is bad, dynamics, static variables, singletons, the importance of structuring a project, UML dIAGREAMS AND SUCH. Asset management for Prefabs, stprites and so on. Different type of types!! i could go on but i'll stop here.
*/
public class GameState{
    public event Action<PieceState> OnSelectedPieceChanged, OnPiecePromoted;
    private BoardState boardState;
    private PlayerState[] playerStates = new PlayerState[2];
    private int currentIndex = 0;
    private PieceState selectedPieceState = null, lastMovedPieceState = null; // To track the last moved piece;
    Vector2Int originalPosition;
    private bool checkmate = false, gameover = false;

    private string promoteTo = "";
    private PawnState promotedPawnState = null;

    public BoardState CurrentBoardState{
        get=>boardState;
        set=>boardState=value;
    }
    public PlayerState[] PlayerStates=>playerStates;

    public int PlayerIndex => currentIndex;

    public PieceState SelectedPieceState{
        get=>selectedPieceState;
        set{
            selectedPieceState=value;
            OnSelectedPieceChanged?.Invoke(selectedPieceState);
            if(selectedPieceState!=null)
                originalPosition = selectedPieceState.Position; // Store original position
        }
    }
    public PieceState LastMovedPieceState=>lastMovedPieceState;

    public Vector2Int OriginalPosition => originalPosition;

    public bool Checkmate=>checkmate;
    public bool Gameover=>gameover;
   
    public string PromoteTo {// This will hold the type of piece the player has chosen to promote to
        get => promoteTo; 
        set{
            promoteTo = value;
            // OnPromotionChanged?.Invoke(promoteTo); // Example of notifying when it changes
        }
        
    }

    public PawnState PromotedPawnState{
        get=>promotedPawnState;
        set=>promotedPawnState=value;
    }

    public GameState(PlayerState p1, PlayerState p2){
        playerStates[0]=p1; playerStates[1]=p2;
        boardState = new BoardState();
        boardState.CreateBoardState(playerStates[0], playerStates[1]);
    }

    public GameState(GameState original){
        this.playerStates[0] = original.playerStates[0].Clone();
        this.playerStates[1] = original.playerStates[1].Clone();
        this.boardState = original.boardState.Clone(this.playerStates[0], this.playerStates[1]);
        
        this.currentIndex = original.currentIndex;
        this.selectedPieceState = original.selectedPieceState?.Clone();
        this.lastMovedPieceState = original.lastMovedPieceState?.Clone();
        this.originalPosition = original.originalPosition;
        this.checkmate = original.checkmate;
        this.promoteTo = original.promoteTo;
    }
    public GameState Clone()=>new GameState(this);

    public void SwitchPlayer()=>currentIndex = (currentIndex + 1) % playerStates.Length;

    public TileState GetTile(Vector2Int pos) => boardState.GetTile(pos);

    // Game ends

    public bool PlayerCheckmated(PlayerState player){ // ends when a player is in double check and cant move the king OR a player is in check and cant evade, capture attacker or block check path
        if(GetAllPlayerMoves(player).Count==0 && currentIndex==player.TurnIndex && player.IsInCheck()){
            Debug.Log($"GAME OVER:{player.PlayerName} IS CHECKMATED");
            return true;
        }
        return false;
    }
    public bool CheckCheckmate()=> PlayerCheckmated(playerStates[0]) || PlayerCheckmated(playerStates[1]);
    public bool CheckInsufficientMaterial(){
        if(playerStates[0].PieceStates.Count==1 && playerStates[1].PieceStates.Count==1){
            Debug.Log($"GAME OVER: DRAW-> INSUFFICIENT MATERIAL");
            return true;
        }
        return false;
    }
    public bool PlayerStalemated(PlayerState player){
        if(GetAllPlayerMoves(player).Count==0 && currentIndex==player.TurnIndex && !player.IsInCheck()){
            Debug.Log($"GAME OVER: DRAW-> {player.PlayerName} STALEMATED");
            return true;
        }
        return false;
    }
    public bool CheckStalemate()=> PlayerStalemated(playerStates[0]) || PlayerStalemated(playerStates[1]);
    public bool IsGameEnd()=>CheckCheckmate() || CheckStalemate() || CheckInsufficientMaterial();
    void End(){gameover=true;}

    // for Bots
    public void MakeBotMove(Vector2Int from, Vector2Int to) {
        // Ensure the piece being moved is valid
        PieceState pieceToMove = boardState.GetTile(from).pieceState;
        selectedPieceState = pieceToMove;

        if(IsPromotion(selectedPieceState, to)){
            promotedPawnState=selectedPieceState as PawnState; // to replace
            promoteTo=(playerStates[currentIndex] as BotState).PromoteTo; // to know what bot wants to promore to 
        }
        if (selectedPieceState != null && selectedPieceState.Colour == playerStates[currentIndex].Colour) {
            ExecuteMove(to);
        }
        selectedPieceState = null; // Deselect the piece after moving
    }

        // Method to check if en passant is valid
    private bool IsEnPassantMove(PieceState piece)
    {
        return lastMovedPieceState != null 
            && piece.Type == "Pawn" 
            && lastMovedPieceState.Type == "Pawn" 
            && Mathf.Abs(lastMovedPieceState.Position.x - piece.Position.x) == 1
            && lastMovedPieceState.Position.y == piece.Position.y
            && (lastMovedPieceState as PawnState).CanBeCapturedEnPassant;
    }
    // Calculate the en passant move position
    private Vector2Int CalculateEnPassantMove(PieceState piece)=>lastMovedPieceState.Position + new Vector2Int(0, currentIndex == 0 ? -1 : 1);
    
    // Validate if a move is legal considering check and pin conditions
    private bool IsValidMove(Vector2Int move, PieceState piece, bool isKing)
    {
        // condition 3: cant move a pinned piece, but can move within a range
        PieceState pinner = IsPinnedPiece(piece);
        bool pinnedPiece = pinner!=null, moveInRange = withinPinnedPieceMovement(piece, pinner, move);
        bool avoidPinTactic = !pinnedPiece || moveInRange;

        // condition 1
        bool mustMoveKing = playerStates[currentIndex].DoubleCheck;
        if(mustMoveKing) return isKing;
        
        // condition 2
        bool check = playerStates[currentIndex].InCheck, mustAvoidCheck = false;;
        if(check){
            PieceState kingAttacker = playerStates[currentIndex].KingAttacker;
            bool canEvade = isKing,
                canCapture = kingAttacker!=null && ((kingAttacker.Position == move||IsEnPassantMove(piece)) && avoidPinTactic),
                canBlock = kingAttacker!=null && (Utility.GetIntermediateLinePoints(kingAttacker.Position, playerStates[currentIndex].GetKing().Position).Contains(move) && avoidPinTactic);
            mustAvoidCheck = (canEvade || canCapture || canBlock);
            return mustAvoidCheck;
        }

        return avoidPinTactic || IsEnPassantMove(piece);
    }

    // Check if the piece is pinned
    private PieceState IsPinnedPiece(PieceState piece){
        PieceState pinner = null;
        List<PieceState> allAttackers = GetAllAttackers(piece);
        foreach (PieceState attacker in allAttackers){
            if(pinner!=null) break; // one attacker pin is enough to count

            HashSet<Vector2Int> tilesBetweenKingAndAttacker;
            switch(attacker.Type){
                case "Bishop":
                    tilesBetweenKingAndAttacker = Utility.GetIntermediateDiagonalLinePoints(playerStates[currentIndex].GetKing().Position, attacker.Position);
                    break;
                case "Rook":
                    tilesBetweenKingAndAttacker = Utility.GetIntermediateNonDiagonalLinePoints(playerStates[currentIndex].GetKing().Position, attacker.Position);
                    break;
                case "Queen":
                    tilesBetweenKingAndAttacker = Utility.GetIntermediateLinePoints(playerStates[currentIndex].GetKing().Position, attacker.Position);
                    break;
                default:
                    // other attacker types cant pin
                    tilesBetweenKingAndAttacker = new HashSet<Vector2Int>(); //empty
                    break;
            }
            if(tilesBetweenKingAndAttacker.Contains(piece.Position) && !HasOtherPiecesInPath(piece, tilesBetweenKingAndAttacker))
                pinner = attacker;
        }
        return pinner;
    }
    // Check for other pieces in the path
    private bool HasOtherPiecesInPath(PieceState piece, HashSet<Vector2Int> tilesBetween)
    {
        HashSet<Vector2Int> tilesWithoutPiecePos = new HashSet<Vector2Int>(tilesBetween);
        tilesWithoutPiecePos.Remove(piece.Position);

        foreach (Vector2Int anotherPiecePosition in tilesWithoutPiecePos)
            if (GetTile(anotherPiecePosition).HasPieceState())
                return true;
        return false;
    }

    bool withinPinnedPieceMovement(PieceState piece, PieceState attacker, Vector2Int move){
        bool pinnedPieceCanCaptureAttacker = false;
        if(attacker!=null){
            HashSet<Vector2Int> tilesBetweenKingAndAttacker = Utility.GetIntermediateLinePoints(playerStates[currentIndex].GetKing().Position, attacker.Position);
            HashSet<Vector2Int> allowedPinnedPieceMoves = new HashSet<Vector2Int>(tilesBetweenKingAndAttacker); // because a pinned piece can still attack
            allowedPinnedPieceMoves.Add(attacker.Position);
            return allowedPinnedPieceMoves.Contains(move);
        }
        return pinnedPieceCanCaptureAttacker; 
    }



    // Main method to get allowed moves for a piece
    public HashSet<Vector2Int> GetMovesAllowed(PieceState piece){
        HashSet<Vector2Int> pieceMoves = FilterMoves(piece);
        HashSet<Vector2Int> gameValidMoves = new HashSet<Vector2Int>();

        // Check for en passant move
        if (IsEnPassantMove(piece)){
            Vector2Int enPassantMove = CalculateEnPassantMove(piece);
            pieceMoves.Add(enPassantMove);
        }

        bool isKing = piece is KingState;

        foreach (Vector2Int move in pieceMoves){
            if (IsValidMove(move, piece, isKing)){
                gameValidMoves.Add(move);
            }
        }

        return gameValidMoves;
    }

    HashSet<Vector2Int> GetAllPlayerMoves(PlayerState player){
        HashSet<Vector2Int> allMoves = new HashSet<Vector2Int>();
        foreach (PieceState piece in player.PieceStates)
            allMoves.UnionWith(GetMovesAllowed(piece));
        return allMoves;
    }

    // JUST FOR POSITIONS THE OPPOSING PLAYER PIECES ARE ATTACKING, not necessarily defended positions(same as defended positons only for pawns)
    // since we are usuing sets all moves will be unique so to check for double checks, have each piece have its own attack moves set
    HashSet<Vector2Int> GetPieceAttackMoves(PieceState pieceState)=>pieceState is PawnState? PawnAttackedTiles(pieceState):pieceState.ValidMoves;
    HashSet<Vector2Int> GetAllPlayerAttackMoves(PlayerState player){
        HashSet<Vector2Int> allMoves = new HashSet<Vector2Int>();
        foreach (PieceState piece in player.PieceStates)
            allMoves.UnionWith(GetPieceAttackMoves(piece)); // add the squares attacked by the Pawn, Pawn fwd moves not included here

        return allMoves;
    }
    public HashSet<Vector2Int> PawnAttackedTiles(PieceState piece){
        HashSet<Vector2Int> attackedTiles = new HashSet<Vector2Int>();
        Vector2Int left = piece.Colour?  new Vector2Int(1,-1) : new Vector2Int(-1,1),
                right = piece.Colour? new Vector2Int(-1,-1) : new Vector2Int(1,1);
        if(boardState.InBounds(piece.Position+left)) attackedTiles.Add(piece.Position+left);
        if(boardState.InBounds(piece.Position+right)) attackedTiles.Add(piece.Position+right);
        return attackedTiles;
    }
    public HashSet<Vector2Int> KingAttackedTiles(PieceState piece){
        HashSet<Vector2Int> attackedTiles, allAttackedTiles = Utility.GetSurroundingPoints(piece.Position);
        attackedTiles = Utility.FindAll<Vector2Int>(allAttackedTiles,boardState.InBounds);
        return attackedTiles;
    }
    public HashSet<Vector2Int> KnightAttackedTiles(PieceState piece){
        HashSet<Vector2Int> attackedTiles, allAttackedTiles = Utility.GetKnightMoves(piece.Position);
        attackedTiles = Utility.FindAll<Vector2Int>(allAttackedTiles, boardState.InBounds);
        return attackedTiles;
    }

     void Opposition(){
        // Find common elements
        HashSet<Vector2Int> KingWhiteMoves = playerStates[0].GetKing().ValidMoves,
                    KingBlackMoves = playerStates[1].GetKing().ValidMoves;
        HashSet<Vector2Int> common = new HashSet<Vector2Int>(KingWhiteMoves);
        common.IntersectWith(KingBlackMoves);

        // Remove common elements from both sets
        foreach (var move in common){
            KingWhiteMoves.Remove(move);
            KingBlackMoves.Remove(move);
        }
        playerStates[0].GetKing().ValidMoves = KingWhiteMoves;
        playerStates[1].GetKing().ValidMoves = KingBlackMoves;
    }
    bool FilterPawnMove(PieceState piece, Vector2Int pos){
        var tile = boardState.GetTile(pos);
        bool pieceAtPos = tile.HasPieceState();
        bool sameColourPieceAtPos = pieceAtPos && tile.pieceState.Colour == piece.Colour;
        bool isDiagonal = Mathf.Abs(piece.Position.x - pos.x) == 1;
        bool isDoubleMove = Mathf.Abs(piece.Position.y - pos.y) == 2;

        if (isDoubleMove && pieceAtPos) return false; // Can't jump over pieces

        HashSet<Vector2Int> tilesBetween = Utility.GetIntermediateLinePoints(piece.Position, pos);
        bool pieceBetween = tilesBetween.Any(tilePos => boardState.GetTile(tilePos).HasPieceState());

        return (pieceAtPos && !sameColourPieceAtPos && isDiagonal) ||
            (!pieceAtPos && !isDiagonal && (!isDoubleMove || !pieceBetween));
    }

    HashSet<Vector2Int> FilterPawnMoves(PieceState piece){
        if (piece == null) return null; // don't even bother
        HashSet<Vector2Int> pawnMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
            if (FilterPawnMove(piece, move))
                pawnMoves.Add(move);
        return pawnMoves;
    }
    bool FilterKnightMove(PieceState piece, Vector2Int pos){
        bool pieceAtPos = boardState.GetTile(pos).HasPieceState();
        bool sameColourPieceAtPos = pieceAtPos && boardState.GetTile(pos).pieceState.Colour == piece.Colour;

        // Check for valid knight move: 2 squares in one direction and 1 square in another
        int dx = Mathf.Abs(piece.Position.x - pos.x);
        int dy = Mathf.Abs(piece.Position.y - pos.y);
        
        // A knight move is valid if it moves in an L-shape
        bool isValidMove = (dx == 2 && dy == 1) || (dx == 1 && dy == 2);
        
        return isValidMove && ( !pieceAtPos || (pieceAtPos && !sameColourPieceAtPos));
    }
    HashSet<Vector2Int> FilterKnightMoves(PieceState piece){
        if (piece == null) return null; // don't even bother
        HashSet<Vector2Int> knightMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
            if (FilterKnightMove(piece, move))
                knightMoves.Add(move);
        return knightMoves;
    }
    bool SlidingPieceFilter(PieceState piece, Vector2Int pos, HashSet<Vector2Int> pointsBetween){
        bool pieceAtpos = boardState.GetTile(pos).HasPieceState(),
            sameColourPieceAtPos = pieceAtpos && boardState.GetTile(pos).pieceState.Colour == piece.Colour;
        foreach (Vector2Int apos in pointsBetween){
            bool pieceAtApos = boardState.GetTile(apos).HasPieceState(),
                sameColourPieceAtAPos = pieceAtApos && boardState.GetTile(apos).pieceState.Colour==piece.Colour;
            bool isAttackingKingTile = pieceAtApos && boardState.GetTile(apos).pieceState.Type=="King" && !sameColourPieceAtAPos;
            if (pieceAtApos && (!isAttackingKingTile || sameColourPieceAtAPos))
                return false;
        }
        return !pieceAtpos || (pieceAtpos && !sameColourPieceAtPos);
    }
    bool FilterBishopMove(PieceState piece, Vector2Int pos)=>SlidingPieceFilter(piece, pos, Utility.GetIntermediateDiagonalLinePoints(piece.Position, pos));
    HashSet<Vector2Int> FilterBishopMoves(PieceState piece){
        if (piece == null) return null; // don't even bother
        HashSet<Vector2Int> bishopMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
            if (FilterBishopMove(piece, move))
                bishopMoves.Add(move);
        return bishopMoves;
    }
    bool FilterRookMove(PieceState piece, Vector2Int pos)=>SlidingPieceFilter(piece, pos, Utility.GetIntermediateNonDiagonalLinePoints(piece.Position, pos));
    HashSet<Vector2Int> FilterRookMoves(PieceState piece){
        if (piece == null) return null; // don't even bother
        HashSet<Vector2Int> rookMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
            if (FilterRookMove(piece, move))
                rookMoves.Add(move);
        return rookMoves;
    }
    bool FilterQueenMove(PieceState piece, Vector2Int pos)=>SlidingPieceFilter(piece, pos, Utility.GetIntermediateLinePoints(piece.Position, pos)); // Determine movement type (diagonal or straight)
    HashSet<Vector2Int> FilterQueenMoves(PieceState piece){
        if (piece == null) return null; // don't even bother
        HashSet<Vector2Int> queenMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
            if (FilterQueenMove(piece, move))
                queenMoves.Add(move);
        return queenMoves;
    }

    private bool IsPieceDefended(Vector2Int pos, PieceState piece) {
        foreach (PieceState opposingPiece in playerStates[piece.Colour ? 1 : 0].PieceStates) {
            if (!boardState.InBounds(opposingPiece.Position)) continue; // Skip out of bounds

            if (opposingPiece.Position == pos) continue; // Skip itself

            bool isDefending = false;
            switch (opposingPiece.Type) {
                case "King":
                    isDefending = KingAttackedTiles(opposingPiece).Contains(pos);
                    break;
                case "Knight":
                    isDefending = KnightAttackedTiles(opposingPiece).Contains(pos);
                    break;
                case "Pawn":
                    isDefending = PawnAttackedTiles(opposingPiece).Contains(pos);
                    break;
                default: // Queen, Rook, Bishop
                    isDefending = IsPieceDefendedBySlidingPiece(opposingPiece, pos);
                    break;
            }

            if (isDefending) return true; // Found a defender
        }
        return false; // No defenders found
    }

    private bool IsPieceDefendedBySlidingPiece(PieceState opposingPiece, Vector2Int targetPos) {
        HashSet<Vector2Int> pointsBetweenAndEnds;
        switch (opposingPiece.Type) {
            case "Bishop":
                pointsBetweenAndEnds = Utility.GetIntermediateDiagonalLinePoints(opposingPiece.Position, targetPos, includeEnds: true);
                break;
            case "Rook":
                pointsBetweenAndEnds = Utility.GetIntermediateNonDiagonalLinePoints(opposingPiece.Position, targetPos, includeEnds: true);
                break;
            case "Queen":
                pointsBetweenAndEnds = Utility.GetIntermediateLinePoints(opposingPiece.Position, targetPos, includeEnds: true);
                break;
            default:
                return false; // Not a defending piece
        }
        if(pointsBetweenAndEnds.Count==0) return false; // there is no path

        // Check if the path is clear, there is the edge case where there are no point between but the piece is still defended, this will ignore the for loop and return true
        pointsBetweenAndEnds.Remove(opposingPiece.Position);
        pointsBetweenAndEnds.Remove(targetPos);
        foreach (Vector2Int point in pointsBetweenAndEnds) {
            if (boardState.GetTile(point).HasPieceState()) {
                return false; // Path is blocked
            }
        }
        return true; // There are defending points
    }

    private bool CanCastle(PieceState piece, Vector2Int pos, int direction) {
        if(piece is not KingState) return false; // only kings can castle

        bool leftSide = direction < 0;
       
        // Determine the correct rook
        PieceState theRook = boardState.GetTile(leftSide ? 0 : 7, piece.Position.y).pieceState as RookState;
        if (theRook == null || !theRook.FirstMove || playerStates[piece.Colour ? 0 : 1].IsInCheck()) {
            return false; // Rook not available or other conditions not met
        }

        // Check spaces between king and rook
        HashSet<Vector2Int> spacesBetween = Utility.GetIntermediateNonDiagonalLinePoints(theRook.Position, piece.Position);
        foreach (Vector2Int space in spacesBetween) {
            if (boardState.GetTile(space).HasPieceState()) return false; // Path blocked
        }

        // Check opponent's attack on the spaces
        foreach (PieceState opponentPiece in playerStates[piece.Colour ? 1 : 0].PieceStates) {
            HashSet<Vector2Int> opPieceMoves = new HashSet<Vector2Int>(opponentPiece.ValidMoves);
            opPieceMoves.IntersectWith(spacesBetween);
            if (opPieceMoves.Count > 0) return false; // Opponent can attack the spaces
        }

        return true; // All castling conditions met
    }
    

    bool FilterKingMove(PieceState piece, Vector2Int pos) {
        bool pieceAtpos = boardState.GetTile(pos).HasPieceState();
        bool sameColourPieceAtPos = pieceAtpos && boardState.GetTile(pos).pieceState.Colour == piece.Colour;
        bool pieceAtposDefended = false; // King can't capture a defended piece

        // Check if there's an opponent's piece at the target position
        if (pieceAtpos && !sameColourPieceAtPos) 
            pieceAtposDefended = IsPieceDefended(pos, piece);
        
        // Check for castling
        int direction = pos.x - piece.Position.x;
        bool castleMove = piece.Position.y == pos.y && Math.Abs(direction) == 2, canCastle = castleMove && CanCastle(piece, pos, direction);
        // The move is valid if:
        // 1. There is no piece at the target position (no capturing) and not a castling move,
        // 2. There is an opponent's piece that is not defended and not a castling move, 
        // 3. The move is a valid castling move.
        return canCastle || (!castleMove && (!pieceAtpos  || (pieceAtpos && !sameColourPieceAtPos && !pieceAtposDefended)));
    }


    HashSet<Vector2Int> FilterKingMoves(PieceState piece){
        if (piece == null) return null; // don't even bother
        HashSet<Vector2Int> kingMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves){
            if (FilterKingMove(piece, move))
                kingMoves.Add(move);
        }
        return kingMoves;
    }

    // Filtering methods for each piece type
    public HashSet<Vector2Int> FilterMoves(PieceState piece){
        if (piece == null) return null;
        return piece.Type switch
        {
            "King" => FilterKingMoves(piece),
            "Queen" => FilterQueenMoves(piece),
            "Rook" => FilterRookMoves(piece),
            "Knight" => FilterKnightMoves(piece),
            "Bishop" => FilterBishopMoves(piece),
            "Pawn" => FilterPawnMoves(piece),
            _ => throw new ArgumentException($"Unknown piece type: {piece.Type}")
        };
    }

    private PieceState GetAttacker(PieceState piece)=>playerStates[piece.Colour ? 1 : 0].PieceStates.Find(p =>(p is PawnState)?PawnAttackedTiles(p).Contains(piece.Position): p.ValidMoves.Contains(piece.Position));
    private List<PieceState> GetAllAttackers(PieceState piece){
        List<PieceState> attackers = new List<PieceState>();
        
        foreach (PieceState opposingPiece in playerStates[piece.Colour ? 1 : 0].PieceStates){
            HashSet<Vector2Int> attackedTiles = opposingPiece is PawnState? PawnAttackedTiles(opposingPiece) : opposingPiece.ValidMoves; 
            if (attackedTiles.Contains(piece.Position))
                attackers.Add(opposingPiece);
        }
        return attackers;
    }

    private void UpdateKingAttack(PieceState king){
        HashSet<Vector2Int> opposingMoves = GetAllPlayerAttackMoves(playerStates[king.Colour ? 1:0]);
        HashSet<Vector2Int> kingMoves = new HashSet<Vector2Int>();
        foreach(Vector2Int move in king.ValidMoves){
            if(!opposingMoves.Contains(move))
                kingMoves.Add(move);
        }
        king.ValidMoves = kingMoves;
    }
    private void UpdateCheckStatus(PlayerState player){
        PieceState king = player.GetKing();
        if (king == null) return; // poterntial error to log

        // Check how many opposing pieces can attack the king
        int attackingPiecesCount = 0;
        HashSet<Vector2Int> opposingMoves;
        foreach (PieceState opposingPiece in playerStates[player.Colour ? 1 : 0].PieceStates){
            if(opposingPiece is KingState) continue;
            opposingMoves = GetPieceAttackMoves(opposingPiece);
            if(opposingMoves.Contains(king.Position)){ // king attack!
                attackingPiecesCount++;
                // Find the attacking piece
                PieceState attacker = GetAttacker(king);
                if (attacker != null){
                    player.KingAttacker = attacker; // Set the attacker
                }else{
                    Debug.LogError("Attacker count went up but could not find attacker");
                }
            }
            if(attackingPiecesCount >= 2) // if there is 1 attacker we found them, if two(or more) doesnt matter the attacker, its double check
                break;
        }
        // Update player's check status
        player.InCheck = attackingPiecesCount == 1;
        player.DoubleCheck = attackingPiecesCount > 1;
    }

    public void UpdateGameState(){

        // Reset and filter valid moves for each piece
        foreach (PlayerState player in playerStates){
            if(player.GetKing() is not KingState){
                Debug.Log(player + " lost their king now");
            }
            foreach (PieceState piece in player.PieceStates){
                piece.ResetValidMoves();
                piece.ValidMoves = FilterMoves(piece);
                // Reset en passant status after each move
                if (piece is PawnState pawn)
                    pawn.ResetEnPassant();   
            } 
        }
        Opposition(); // Update the opposition

        // Check if playerStates are in check
        foreach (PlayerState player in playerStates){
            UpdateCheckStatus(player); //Debug.Log($"After UpdateCheckStatus: {player.PlayerName} InCheck: {player.InCheck}, DoubleCheck: {player.DoubleCheck}");
            UpdateKingAttack(player.GetKing()); // Update King's moves based on opponent pieces
        }                
    }

    
    public Vector2Int ExecuteMove(Vector2Int targetPosition){
        Vector2Int lastPosition = default; // needed for bots

        // Check for capture
        if (IsCapture(targetPosition)){
            PieceState captured = boardState.GetTile(targetPosition).pieceState;
            if(captured is KingState){
                Debug.Log("The king has been captured by " + (selectedPieceState == null ? promotedPawnState : selectedPieceState));
            }
            boardState.MovePiece(targetPosition, default, true); // remove from tile/board
            playerStates[currentIndex].Capture(captured); // remove from playerstate
            playerStates[(currentIndex + 1) % 2].RemovePieceState(captured);
            captured.Captured = true; // set final resting place
        }

        // promotion moves
        if(IsPromotion(targetPosition)){
            // only handled move exxecution
            if(playerStates[currentIndex] is BotState botState)
                promoteTo = botState.PromoteTo;
            if(!string.IsNullOrEmpty(promoteTo)){
                //Debug.Log(promoteTo + " is choice");
                // set proper params, loaction, colour, etc
                // create the piecestate
                PieceState replacementState = Objects.CreatePieceState(
                    promoteTo, 
                    promotedPawnState.Colour, 
                    targetPosition, 
                    promotedPawnState.MinPoint, 
                    promotedPawnState.MaxPoint
                );
                
                // Remove the pawnstate
                
                boardState.MovePiece(promotedPawnState.Position, targetPosition, true); // remove from tile/board
                PlayerState currentPlayerState = playerStates[currentIndex]; // remove from playerstate
                currentPlayerState.RemovePieceState(promotedPawnState);
                promotedPawnState.Promoted = true; // set final resting place
                if(currentPlayerState is BotState botState1) // reset promotion choice if bot move
                    botState1.PromoteTo = "";

                // Add replacementstate
                GetTile(replacementState.Position).pieceState = replacementState; // set for tile/board
                playerStates[currentIndex].AddPieceState(replacementState); // set for player

                // call to game to create piece(for ui)-> set piecestate to piece, add piece to player
                OnPiecePromoted?.Invoke(replacementState); // Trigger the promotion event
                
            }else{
                Debug.LogError("want to promote but cant");
            }
            // record lastPiece data
            lastPosition = promotedPawnState.Position;
            lastMovedPieceState = promotedPawnState; // Store the last moved piece
            promoteTo=""; promotedPawnState=null; // reset
        }else{
            // Handle en passant
            if (lastMovedPieceState is PawnState lastPawn && lastPawn.CanBeCapturedEnPassant && SelectedPieceState is PawnState) {
                Vector2Int enPassantTarget = lastPawn.Position + new Vector2Int(0, currentIndex == 0 ? -1 : 1);
                if (targetPosition == enPassantTarget) {
                    PieceState captured = boardState.GetTile(lastPawn.Position).pieceState;
                    playerStates[currentIndex].Capture(captured);
                    playerStates[(currentIndex + 1) % 2].RemovePieceState(captured);
                    captured.Captured = true;
                }
            }

            // is a castleMove, already moved king now move correct rook
            int direction = targetPosition.x-selectedPieceState.Position.x;
            bool leftSide=direction<0,
                    castleMove = selectedPieceState.Position.y==targetPosition.y
                                && Math.Abs(direction)==2
                                && selectedPieceState is KingState; // only kings castle
            if(castleMove){
                // determine the correct rook
                PieceState theRook = null;
                Vector2Int rookCastlePosition = default;
                if(boardState.GetTile(leftSide?0:7, selectedPieceState.Position.y).HasPieceState()
                && boardState.GetTile(leftSide?0:7, selectedPieceState.Position.y).pieceState is RookState rookState){
                    theRook = rookState;
                    rookCastlePosition = new Vector2Int(selectedPieceState.Position.x+ direction+(leftSide?1:-1), selectedPieceState.Position.y);
                    boardState.MovePiece(theRook.Position, rookCastlePosition);
                    theRook.Move(rookCastlePosition);
                }
            }
            // record lastPiece data
            lastPosition = selectedPieceState.Position;
            lastMovedPieceState = selectedPieceState; // Store the last moved piece
            
                
            // Move the piece
            boardState.MovePiece(selectedPieceState.Position, targetPosition);
            selectedPieceState.Move(targetPosition);

        }

        //updates
        UpdateGameState();
        SwitchPlayer();
        if(IsGameEnd())
            End();

        return lastPosition;
    }

    public bool IsCapture(Vector2Int targetPosition) => boardState.GetTile(targetPosition).HasPieceState();
    public bool IsPromotion(Vector2Int targetPosition)=>promotedPawnState!=null && promotedPawnState is PawnState && targetPosition.y==(promotedPawnState.Colour?0:7);
    public static bool IsPromotion(PieceState pieceState, Vector2Int targetPosition)=>pieceState is PawnState && targetPosition.y==(pieceState.Colour?0:7);
}



















































//////////////////////////////////////////

public class Game : MonoBehaviour{
    private GameState state;
    private Board board;
    private Player[] players = new Player[2]; // only 2 playerStates for a chess game

    Piece selectedPiece;
    private bool isPromotionInProgress = false;


    // Call this method when a pawn reaches the last rank
    public void ShowPromotionOptions(Vector2Int promotionTileLocation, bool isWhitePlayer)
    {
        // Determine tile color and size
        Tile promotionTile = board.GetTile(promotionTileLocation);
        
        // Show the promotion UI
        PromotionUI promotionUI = gameObject.AddComponent<PromotionUI>(); // Add the PromotionUI component to the Game object
        promotionUI.Show(OnPromotionSelected, promotionTile.MyColour, new Vector2(promotionTile.N,promotionTile.N), selectedPiece.MyColour, promotionTileLocation);
    }

    private void OnPromotionSelected(Vector2Int targetPosition, string pieceType){
        state.SelectedPieceState = null; // an extra nullifier for sselectedPiece state because black queens always leave the promoted piece
    
        // Update the promoteTo variable in GameState
        state.PromoteTo = pieceType;
        if(state.PromoteTo!=""){
            state.ExecuteMove(targetPosition);
        }else{
            state.SelectedPieceState = state.PromotedPawnState;
            state.PromotedPawnState = null;
            state.SelectedPieceState.Position = state.OriginalPosition; // Reset to original
        }
        // Reset the promotion state
        isPromotionInProgress = false; // Reset after promotion is handled
    }

    // Player GUI
    private void UpdateSelectedPiece(PieceState newPieceState){
        // Find the corresponding Piece based on the PieceState
        selectedPiece = FindPieceFromState(newPieceState);
    }
    private void HandlePiecePromotion(PieceState replacementState){
        PlayerState currentPlayerState = state.PlayerStates[state.PlayerIndex];

        // Create the new Piece and add it to the player's pieces
        string pieceTypeName = replacementState.GetType().Name.Replace("State",""); // Get the type name from the PieceState
        Piece newPiece = Objects.CreatePiece(
            $"{pieceTypeName}{(currentPlayerState.Colour ? "W" : "B")}p",
            pieceTypeName,
            replacementState, 
            currentPlayerState.TileSize,
            board.PieceScaleFactor
        );

        if (newPiece != null){
            Player currentPlayer = players[state.PlayerIndex]; // Get the current player
            currentPlayer.AddPiece(newPiece); 
        }else
            Debug.LogError("Failed to create the new piece during promotion.");
        
    }
    private Piece FindPieceFromState(PieceState pieceState)
    {
        foreach (Player player in players)
        {
            // Assuming you have access to a list of pieces
            foreach (Piece piece in player.Pieces)
            {
                if (piece.State == pieceState)
                {
                    return piece;
                }
            }
        }
        return null; // Or handle the case where no match is found
    }
    void SelectPiece(){
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        Collider2D collision = Physics2D.OverlapPoint(mousePosition);
        if (collision != null){
            Piece piece = collision.GetComponent<Piece>();
            if (piece != null && players[state.PlayerIndex].State.Colour == piece.State.Colour){ // only allow selection for the player to play
                selectedPiece = piece; //Select piece
                state.SelectedPieceState = selectedPiece.State;
            }
        }
    }
    void DragPiece(){
        Vector2 mousePosition = Utility.GetMouseWorldPosition();
        if (selectedPiece != null)
            selectedPiece.transform.position = new Vector3(mousePosition.x, mousePosition.y, 0); // move piece with mouse
    }

    void BotMove() {
        Vector2Int[] fromTo = players[state.PlayerIndex].State.GetMove();
        if(fromTo == null){
            Debug.Log("Bot got no moves left");
            return;
        }
        Vector2Int fromPosition = fromTo[0];
        Vector2Int targetPosition = fromTo[1];

        // Ensure the piece being moved is valid
        state.MakeBotMove(fromPosition, targetPosition);
    }
    void ReleasePiece(){
        Vector2Int targetPosition = players[state.PlayerIndex].State.GetMove()[1];
        if (state.GetMovesAllowed(state.SelectedPieceState).Contains(targetPosition)){
            // promotion moves
            if(GameState.IsPromotion(state.SelectedPieceState, targetPosition)){
                // show promotion UI
                // Show promotion UI and wait for user input
                isPromotionInProgress = true; // Set the promotion state to true
                state.PromotedPawnState = state.SelectedPieceState as PawnState;
                ShowPromotionOptions(targetPosition, state.SelectedPieceState.Colour); 
            }else
                state.ExecuteMove(targetPosition);
            
        }else
            state.SelectedPieceState.Position = state.OriginalPosition; // Reset to original
            
        state.SelectedPieceState = null;
        selectedPiece = null;
        
    }
    void HandleDragAndDrop(){
        if (selectedPiece != null){
            DragPiece();
            if (Utility.MouseUp())
                ReleasePiece();
        }
    }

    private void HandleInput(){
        if(state.Gameover) return; // dont handl user input

        if(players[state.PlayerIndex] is Bot){
            BotMove();
            return;
        }

        if (Utility.MouseDown()) // Left mouse button
            SelectPiece();
        else if (selectedPiece != null)
            if(!isPromotionInProgress) // can use drag and drop while promoting
                HandleDragAndDrop();
    }

    public void InitializeGame(string whitePlayerType, string blackPlayerType, string whitePlayerName, string blackPlayerName, string filePath)
    {
        InitializePlayers(whitePlayerType, blackPlayerType, whitePlayerName, blackPlayerName, filePath);
        // Initialize board after players are set
        InitializeBoard();
        state.UpdateGameState(); // ready to start
    }

    private void InitializeBoard()
    {
        board = gameObject.AddComponent<Board>();
        board.State = state.CurrentBoardState;
        board.CreateBoard(players[0], players[1]);
        Debug.Log("Board created");
    }

    private void InitializeGameState(PlayerState P1State, PlayerState P2State)
    {
        state = new GameState(P1State, P2State);

        // subcriptions
        state.OnSelectedPieceChanged += UpdateSelectedPiece;
        state.OnPiecePromoted += HandlePiecePromotion; // Subscribe to promotion events

        if (P1State is BotState botState1)
            botState1.CurrentGame = this.state;
        if (P2State is BotState botState2)
            botState2.CurrentGame = this.state;
    }

    private void InitializePlayers(string whitePlayerTypeName, string blackPlayerTypeName, string whitePlayerName, string blackPlayerName, string filePath){
        PlayerState P1State = Objects.CreatePlayerState(whitePlayerTypeName, whitePlayerName, true, filePath);
        PlayerState P2State = Objects.CreatePlayerState(blackPlayerTypeName, blackPlayerName, false, filePath);
        InitializeGameState(P1State, P2State);

        // Dynamically add the components using the Type objects
        // Convert the selected type names to Type objects
        Type whitePlayerType = Type.GetType(whitePlayerTypeName);
        Type blackPlayerType = Type.GetType(blackPlayerTypeName);
        if (whitePlayerType == null || blackPlayerType == null){
            Debug.LogError("Could not find player types!");
            return;
        }
        Player P1 = gameObject.AddComponent(whitePlayerType) as Player,
            P2 = gameObject.AddComponent(blackPlayerType) as Player;
            /*
        Debug.Log("PlayerPlayer stement" + P1+" "+(P1 is Player) + (P1 is Avenger));
        Debug.Log("PlayerPlayer stement" + P2+" "+(P2 is Player) + (P2 is Avenger));
        */

        // Ensure that P1 and P2 are not null after adding components
        if (P1 == null || P2 == null)
        {
            Debug.LogError("Failed to add player components!");
            return;
        }

        P1.State = P1State;
        P2.State = P2State;

        Debug.Log($"P1: {P1}, P2: {P2}");
        players[0] = P1;
        players[1] = P2;
    }



    void Awake() {
        Debug.Log("Awake called");
        //InitializeGame();
    }

    // Start is called before the first frame update
    void Start(){
        //state.UpdateGameState();
    }
    // Update is called once per frame
    void Update(){
        if(state!=null)HandleInput();
    }

    private void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks
        if(state!=null){
            state.OnSelectedPieceChanged -= UpdateSelectedPiece;
            state.OnPiecePromoted -= HandlePiecePromotion; // Unsubscribe to avoid memory leaks
        }
    }
}
