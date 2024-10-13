using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json; // for saving and loading games
using System.Linq; // Add this line for LINQ


public class GameState{
    public event Action<PieceState> OnSelectedPieceChanged, OnPiecePromoted;
    private BoardState boardState;
    private PlayerState[] playerStates = new PlayerState[2];
    private int currentIndex = 0;
    private PieceState selectedPieceState = null, lastMovedPieceState = null; // To track the last moved piece;
    Vector2Int originalPosition;
    private bool checkmate = false;

    private string promoteTo = "";

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
   
    public string PromoteTo {// This will hold the type of piece the player has chosen to promote to
    get => promoteTo; 
    set
    {
        promoteTo = value;
        // OnPromotionChanged?.Invoke(promoteTo); // Example of notifying when it changes
    }
}
    public GameState(PlayerState p1, PlayerState p2){
        playerStates[0]=p1; playerStates[1]=p2;
        boardState = new BoardState();
        boardState.CreateBoardState(playerStates[0], playerStates[1]);
    }

    public GameState(GameState original){
        this.boardState = original.boardState.Clone();
        this.playerStates[0] = original.playerStates[0].Clone();
        this.playerStates[1] = original.playerStates[1].Clone();
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
    bool IsGameEnd(){
        foreach (PlayerState player in playerStates){ // ends when a player is in double check and cant move the king OR a player is in check and cant evade, capture attacker or block check path
            PieceState PlayerKing = player.GetKing();
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

    void End(){checkmate=true;}

    // for Bots
    public void MakeBotMove(Vector2Int from, Vector2Int to) {
        // Ensure the piece being moved is valid
        PieceState pieceToMove = boardState.GetTile(from).pieceState;
        selectedPieceState = pieceToMove;
        if (selectedPieceState != null && selectedPieceState.Colour == playerStates[currentIndex].Colour) {
            ExecuteMove(to);
        }
        selectedPieceState = null; // Deselect the piece after moving
    }

    public HashSet<Vector2Int> GetMovesAllowed(PieceState piece){ // using the game constraints to get moves allowed
        HashSet<Vector2Int> pieceMoves = FilterMoves(piece), gameValidMoves=new HashSet<Vector2Int>();

        // add enPassantMove for checking
        bool isAnEnPassantMove = lastMovedPieceState!=null 
                && piece.Type=="Pawn" && lastMovedPieceState.Type=="Pawn" 
                && Mathf.Abs(lastMovedPieceState.Position.x-piece.Position.x)==1
                && lastMovedPieceState.Position.y==piece.Position.y
                && (lastMovedPieceState as PawnState).CanBeCapturedEnPassant;
        if(isAnEnPassantMove){
            Vector2Int enPassantMove = lastMovedPieceState.Position+new Vector2Int(0, currentIndex==0 ? -1:1);
            pieceMoves.Add(enPassantMove);
        }

        bool isKing = piece is KingState;
        foreach (Vector2Int move in pieceMoves){
            // condition 1
            bool mustMoveKing = playerStates[currentIndex].DoubleCheck && isKing;

            // condition 2
            PieceState kingAttacker = playerStates[currentIndex].KingAttacker;
            bool canEvade=isKing, // move king
                canCapture=kingAttacker!=null && (kingAttacker.Position==move || isAnEnPassantMove), // cap attacker
                canBlock=kingAttacker!=null && (Utility // can block
                    .GetIntermediateLinePoints(kingAttacker.Position, playerStates[currentIndex].GetKing().Position)
                    .Contains(move)); 
            bool mustAvoidCheck = playerStates[currentIndex].InCheck && (canEvade || canCapture || canBlock);


            // condition 3: cant move a pinned piece
            bool pinnedPiece = false, pinnedPieceCanCaptureAttacker = false;
            PieceState attacker = GetAttacker(piece); // selected piece is attacked

            if(attacker!=null){
                HashSet<Vector2Int> tilesBetweenKingAndAttacker = Utility.GetIntermediateLinePoints(playerStates[currentIndex].GetKing().Position, attacker.Position);
                pinnedPiece = tilesBetweenKingAndAttacker.Contains(piece.Position);
                // if there are multiple pieces in th epath, any can move
                HashSet<Vector2Int> tBKaA_without_piecePos = new HashSet<Vector2Int>(tilesBetweenKingAndAttacker);
                tBKaA_without_piecePos.Remove(piece.Position);
                foreach (Vector2Int anotherPiecePosition in tBKaA_without_piecePos){
                    if(GetTile(anotherPiecePosition).HasPieceState()){
                        pinnedPiece = false;
                        break;
                    }
                    
                }
                
                HashSet<Vector2Int> allowedPinnedPieceMoves = tilesBetweenKingAndAttacker; // because a pinned piece can still attack
                allowedPinnedPieceMoves.Add(attacker.Position);
                pinnedPieceCanCaptureAttacker = allowedPinnedPieceMoves.Contains(move);
            }
            bool avoidPinTactic = !pinnedPiece || pinnedPieceCanCaptureAttacker;

            //Debug.Log(mustMoveKing + " " + mustAvoidCheck + " " + avoidPinTactic + " " + isAnEnPassantMove + " ");
            if(mustMoveKing || mustAvoidCheck || (!playerStates[currentIndex].IsInCheck() && avoidPinTactic) || isAnEnPassantMove)
                gameValidMoves.Add(move);    
        }

        return gameValidMoves;
    }
    HashSet<Vector2Int> GetAllPlayerMoves(PlayerState player){
        HashSet<Vector2Int> allMoves = new HashSet<Vector2Int>();
        foreach (PieceState piece in player.PieceStates)
            foreach (Vector2Int move in piece.ValidMoves)
                allMoves.Add(move);
        return allMoves;
    }

    // JUST FOR POSITIONS THE OPPOSING PLAYER PIECES ARE ATTACKING, not necessarily defended positions(same as defended positons only for pawns)
    HashSet<Vector2Int> GetAllPlayerAttackMoves(PlayerState player){
        HashSet<Vector2Int> allMoves = new HashSet<Vector2Int>();
        foreach (PieceState piece in player.PieceStates){
            bool isPawn = piece.Type=="Pawn";
            if(isPawn)
                allMoves.UnionWith(PawnAttackedTiles(piece)); // add the squares attacked by the Pawn, Pawn fwd moves not included here
            else
                foreach (Vector2Int move in piece.ValidMoves)
                    allMoves.Add(move); 
        }
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
        HashSet<Vector2Int> attackedTiles = new HashSet<Vector2Int>();
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
    bool FilterKnightMove(Vector2Int pos){
        return false; // Implement actual logic as needed
    }
    HashSet<Vector2Int> FilterKnightMoves(PieceState piece){
        if (piece == null) return null; // don't even bother
        HashSet<Vector2Int> knightMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
            if (FilterKnightMove(move))
                knightMoves.Add(move);
        return knightMoves;
    }
    bool FilterBishopMove(PieceState piece, Vector2Int pos){
        bool pieceAtpos = boardState.GetTile(pos).HasPieceState(),
            sameColourPieceAtPos = pieceAtpos && boardState.GetTile(pos).pieceState.Colour == piece.Colour;
        HashSet<Vector2Int> pointsBetween = Utility.GetIntermediatePoints(piece.Position, pos, Utility.MovementType.Diagonal);
        foreach (Vector2Int apos in pointsBetween){
            bool pieceAtApos = boardState.GetTile(apos).HasPieceState(),
                sameColourPieceAtAPos = pieceAtApos && boardState.GetTile(apos).pieceState.Colour==piece.Colour;
            bool isAttackingKingTile = pieceAtApos && boardState.GetTile(apos).pieceState.Type=="King" && !sameColourPieceAtAPos;
            if (pieceAtApos && (!isAttackingKingTile || sameColourPieceAtAPos))
                return false;
        }
        return !pieceAtpos || (pieceAtpos && !sameColourPieceAtPos);
    }
    HashSet<Vector2Int> FilterBishopMoves(PieceState piece){
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
    HashSet<Vector2Int> FilterRookMoves(PieceState piece){
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
    HashSet<Vector2Int> FilterQueenMoves(PieceState piece){
        if (piece == null) return null; // don't even bother
        HashSet<Vector2Int> queenMoves = new HashSet<Vector2Int>();
        foreach (var move in piece.ValidMoves)
            if (FilterQueenMove(move))
                queenMoves.Add(move);
        return queenMoves;
    }
    bool FilterKingMove(PieceState piece, Vector2Int pos){
        bool pieceAtpos = boardState.GetTile(pos).HasPieceState(),
            sameColourPieceAtPos = pieceAtpos && boardState.GetTile(pos).pieceState.Colour == piece.Colour;
        bool pieceAtposDefended = false; // King cant capture a defended piece
        if(pieceAtpos && !sameColourPieceAtPos){ // check if that piece is defended
            foreach (PieceState opposingPiece in playerStates[piece.Colour?1:0].PieceStates){
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
                                    pieceAtposDefended = pieceAtposDefended && !boardState.GetTile(point).HasPieceState(); // if a single piece on path, path is blocked and piece cant be defended
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

        // also castle moves
        int direction = pos.x-piece.Position.x;
        bool leftSide=direction<0, castleMove = piece.Position.y==pos.y && Math.Abs(direction)==2,
            canCastle = castleMove;
        if(castleMove){
            // determine the correct rook
            PieceState theRook = null;
            if(boardState.GetTile(leftSide?0:7, piece.Position.y).HasPieceState()
            && boardState.GetTile(leftSide?0:7, piece.Position.y).pieceState is RookState rookState)
                theRook = rookState;
            if(theRook!=null){
                /*
                3)no pieces between king and rook in that direction
                1)rooks first move
                4)no opps attack space between king and rook in that dir
                2)king/player to move not in check
                */
                // 1
                canCastle = theRook.FirstMove;
                //Debug.Log("castling 1"+canCastle);
                // 2
                canCastle = canCastle && !playerStates[piece.Colour?0:1].IsInCheck();
                //Debug.Log("castling 2"+canCastle);
                // 3
                HashSet<Vector2Int> spacesBetween = Utility.GetIntermediateNonDiagonalLinePoints(theRook.Position, piece.Position);
                foreach (Vector2Int space in spacesBetween)
                    canCastle = canCastle && boardState.GetTile(space).HasPieceState()==false;
                //Debug.Log("castling 3"+canCastle);
                //4
                foreach (PieceState opponentPiece in playerStates[piece.Colour?1:0].PieceStates)
                {
                    if(!canCastle) break;
                    // better to intersect a smaller set into a larger one
                    HashSet<Vector2Int> opPieceMoves = new HashSet<Vector2Int>(opponentPiece.ValidMoves);
                    opPieceMoves.IntersectWith(spacesBetween);
                    canCastle = canCastle && opPieceMoves.Count==0;
                }
                //Debug.Log("castling 4"+canCastle);

            }
        }
        return ((!pieceAtpos || (pieceAtpos && !sameColourPieceAtPos && !pieceAtposDefended)) && !castleMove) || canCastle;
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

    public HashSet<Vector2Int> FilterMoves(PieceState piece){
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

    private PieceState GetAttacker(PieceState piece)=>playerStates[piece.Colour ? 1 : 0].PieceStates.Find(p => p.ValidMoves.Contains(piece.Position));
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
        if (king == null) return;

        HashSet<Vector2Int> opposingMoves = GetAllPlayerAttackMoves(playerStates[player.Colour ? 1 : 0]);

        // Check how many opposing pieces can attack the king
        int attackingPiecesCount = 0;
        //player.KingAttacker = null;
        foreach (var move in opposingMoves){
            if (move == king.Position){
                attackingPiecesCount++;
                // Find the attacking piece
                PieceState attacker = GetAttacker(king);
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

    public void UpdateGameState(){
        // Reset and filter valid moves for each piece
        int bcount = 0;
        foreach (PlayerState player in playerStates)
            foreach (PieceState piece in player.PieceStates){
                piece.ResetValidMoves();
                piece.ValidMoves = FilterMoves(piece);
                
                if(piece is BishopState && player.Colour){
                    Debug.Log("I am bishop! "+bcount);
                    foreach (var item in piece.ValidMoves)
                    {
                        Debug.Log(item+ " for " + bcount);
                    }
                    bcount++;
                }

                // Reset en passant status after each move
                if (piece is PawnState pawn)
                    pawn.ResetEnPassant();   
            } 
        Opposition(); // Update the opposition

        // Check if playerStates are in check
        foreach (PlayerState player in playerStates){
            UpdateCheckStatus(player); //Debug.Log($"After UpdateCheckStatus: {player.PlayerName} InCheck: {player.InCheck}, DoubleCheck: {player.DoubleCheck}");
            UpdateKingAttack(player.GetKing()); // Update King's moves based on opponent pieces
        }                
    }

    
    public Vector2Int ExecuteMove(Vector2Int targetPosition){
        // Check for capture
        if (IsCapture(targetPosition)){
            PieceState captured = boardState.GetTile(targetPosition).pieceState;
            playerStates[currentIndex].Capture(captured);
            playerStates[(currentIndex + 1) % 2].RemovePieceState(captured);
            captured.Captured = true;
        }

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
        Debug.Log(targetPosition + (selectedPieceState==null?"ya its nul": selectedPieceState.Type));
        int direction = targetPosition.x-selectedPieceState.Position.x;
        bool leftSide=direction<0, castleMove = selectedPieceState.Position.y==targetPosition.y && Math.Abs(direction)==2;
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
        Vector2Int lastPosition = selectedPieceState.Position;
        lastMovedPieceState = selectedPieceState; // Store the last moved piece
        
        // promotion moves
        Debug.Log("preomitint to "+promoteTo);
        bool isPromotion = selectedPieceState is PawnState && targetPosition.y==(selectedPieceState.Colour?0:7); 
        if(isPromotion){
            // only handled move exxecution
            if(!string.IsNullOrEmpty(promoteTo)){
                Debug.Log(promoteTo + " is choice");
                // set proper params, loaction, colour, etc
                // create the piecestate
                PieceState replacementState = Objects.CreatePieceState(
                    promoteTo, 
                    selectedPieceState.Colour, 
                    targetPosition, 
                    selectedPieceState.MinPoint, 
                    selectedPieceState.MaxPoint
                );
                Debug.Log(replacementState+" is my replacement");
                playerStates[currentIndex].AddPieceState(replacementState);
                // call to game to create piece(for ui)-> set piecestate to piece, add piecestate and piece to playerstate and player
                boardState.MovePiece(selectedPieceState.Position, PieceState.heavenOrhell, remove:true);
                OnPiecePromoted?.Invoke(replacementState); // Trigger the promotion event
                
                // remove pawn from playerstate piecestates and player pieces-> heavenOrhell location
                
                
                
            }else{
                Debug.LogError("want to promote but cant");
            }
            
        }else{
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
        promotionUI.Show(OnPromotionSelected, promotionTile.MyColour, new Vector2(promotionTile.N,promotionTile.N), selectedPiece, promotionTile.State.Position);
    }

    private void OnPromotionSelected(Vector2Int targetPosition, string pieceType)
    {
        // Update the promoteTo variable in GameState
        state.PromoteTo = pieceType;
        if(state.PromoteTo!=""){
            state.ExecuteMove(targetPosition);
        }else{
            state.SelectedPieceState.Position = state.OriginalPosition; // Reset to original
        }
        Debug.Log(state.SelectedPieceState.Position + "is my pos");
        
        // Reset the promotion state
        isPromotionInProgress = false; // Reset after promotion is handled
    }

    // Player GUI
    private void UpdateSelectedPiece(PieceState newPieceState)
    {
        // Find the corresponding Piece based on the PieceState
        selectedPiece = FindPieceFromState(newPieceState);
    }
    private void HandlePiecePromotion(PieceState replacementState){
        PlayerState currentPlayerState = state.PlayerStates[state.PlayerIndex];

        // Remove the pawn from PlayerState
        if(state.SelectedPieceState is PawnState pawn)pawn.Promoted =true;
        currentPlayerState.RemovePieceState(state.SelectedPieceState);

        // Create the new Piece and add it to the player's pieces
        string pieceTypeName = replacementState.GetType().Name.Replace("State",""); // Get the type name from the PieceState
        Piece newPiece = Objects.CreatePiece(
            $"{pieceTypeName}{(currentPlayerState.Colour ? "W" : "B")}",
            pieceTypeName,
            replacementState, 
            currentPlayerState.TileSize, 
            replacementState.Position.x, 
            replacementState.Position.y, 
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
        Vector2Int fromPosition = fromTo[0];
        Vector2Int targetPosition = fromTo[1];

        // Ensure the piece being moved is valid
        state.MakeBotMove(fromPosition, targetPosition);
    }
    void ReleasePiece(){
        Vector2Int targetPosition = players[state.PlayerIndex].State.GetMove()[1];
        if (state.GetMovesAllowed(state.SelectedPieceState).Contains(targetPosition)){
            // promotion moves
            bool isPromotion = state.SelectedPieceState is PawnState && targetPosition.y==(state.SelectedPieceState.Colour?0:7); 
            if(isPromotion){
                // show promotion UI
                // Show promotion UI and wait for user input
                isPromotionInProgress = true; // Set the promotion state to true
                ShowPromotionOptions(targetPosition, state.SelectedPieceState.Colour); 
                Debug.Log(state.SelectedPieceState.Position + "is my pos 2");
            }else
                state.ExecuteMove(targetPosition);
            
        }else
            state.SelectedPieceState.Position = state.OriginalPosition; // Reset to original

        // Clear selection (this can be moved to OnPromotionSelected if needed)
        if (!isPromotionInProgress) {
            state.SelectedPieceState = null;
            selectedPiece = null;
        }
        //Debug.Log(state.SelectedPieceState.Position + "is my pos 3");
    }
    void HandleDragAndDrop(){
        if (selectedPiece != null){
            DragPiece();
            if (Utility.MouseUp())
                ReleasePiece();
        }
    }

    private void HandleInput(){
        if(state.Checkmate) return; // dont handl user input

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

        if (P1State is BotState)
            (P1State as BotState).CurrentGame = this.state;
        if (P2State is BotState)
            (P2State as BotState).CurrentGame = this.state;
    }

    private void InitializePlayers(string whitePlayerTypeName, string blackPlayerTypeName, string whitePlayerName, string blackPlayerName, string filePath)
    {
        Debug.Log("here1 " + whitePlayerTypeName + " " + blackPlayerTypeName);

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
