using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json; // for saving and loading games
using System.Linq; // Add this line for LINQ

public class GameState{
    public event Action<PieceState> OnSelectedPieceChanged;
    private BoardState boardState;
    private PlayerState[] playerStates = new PlayerState[2];
    private int currentIndex = 0;
    private PieceState selectedPieceState = null, lastMovedPieceState = null; // To track the last moved piece;
    Vector2Int originalPosition;
    private bool checkmate = false;

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

    public bool Checkmate{
        get=>checkmate;
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
                foreach (Vector2Int anotherPiecePosition in tBKaA_without_piecePos)
                {
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
        foreach (PieceState piece in player.PieceStates)
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
        foreach (var move in common)
        {
            KingWhiteMoves.Remove(move);
            KingBlackMoves.Remove(move);
        }
        playerStates[0].GetKing().ValidMoves = KingWhiteMoves;
        playerStates[1].GetKing().ValidMoves = KingBlackMoves;
    }
    bool FilterPawnMove(PieceState piece, Vector2Int pos){
        bool pieceAtpos = boardState.GetTile(pos).HasPieceState(),
            sameColourPieceAtPos = pieceAtpos && boardState.GetTile(pos).pieceState.Colour == piece.Colour,
            isDiag = Mathf.Abs(piece.Position.x - pos.x)==1,
            isDoubleMove = Mathf.Abs(piece.Position.y - pos.y)==2; // also pawns cant jump pieces
        HashSet<Vector2Int> tilesBetween = Utility.GetIntermediateLinePoints(piece.Position, pos);

        bool pieceBetween = false;
        foreach (Vector2Int tilePos in tilesBetween){
            pieceBetween = boardState.GetTile(tilePos).HasPieceState();
            if(pieceBetween)
                break;
        }
        return (pieceAtpos && !sameColourPieceAtPos && isDiag) || (!pieceAtpos && !isDiag && (!isDoubleMove || isDoubleMove && !pieceBetween));
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
        foreach (Vector2Int apos in pointsBetween)
        {
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
        return (!pieceAtpos || (pieceAtpos && !sameColourPieceAtPos && !pieceAtposDefended));
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
        foreach (PlayerState player in playerStates){
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
        foreach (PlayerState player in playerStates)
        {
            UpdateCheckStatus(player); //Debug.Log($"After UpdateCheckStatus: {player.PlayerName} InCheck: {player.InCheck}, DoubleCheck: {player.DoubleCheck}");
            UpdateKingAttack(player.GetKing()); // Update King's moves based on opponent pieces
        }

                        
    }

    
    public Vector2Int ExecuteMove(Vector2Int targetPosition){
        if (isCapture(targetPosition)){
            PieceState captured = boardState.GetTile(targetPosition).pieceState;
            playerStates[currentIndex].Capture(captured);
            playerStates[(currentIndex + 1) % 2].RemovePieceState(captured);
            captured.Captured = true;
        }
        if (lastMovedPieceState is PawnState && lastMovedPieceState.Position.x == targetPosition.x){ // Handle en passant
            Vector2Int enPassantTarget = lastMovedPieceState.Position + new Vector2Int(0, currentIndex == 0 ? -1 : 1);
            if (targetPosition == enPassantTarget){ //Debug.Log("Execute EnPassant");
                // Remove the pawn that is captured en passant
                PieceState captured = boardState.GetTile(lastMovedPieceState.Position).pieceState;
                playerStates[currentIndex].Capture(captured);
                playerStates[(currentIndex + 1) % 2].RemovePieceState(captured);
                captured.Captured = true;
            }
        }
        boardState.MovePiece(selectedPieceState.Position, targetPosition);
        Vector2Int lastPosition = selectedPieceState.Position;
        selectedPieceState.Move(targetPosition);
        lastMovedPieceState = selectedPieceState; // Store the last moved piece

        UpdateGameState();
        SwitchPlayer();
        if(IsGameEnd())
            End();
        
        return lastPosition;
    }
    bool isCapture(Vector2Int targetPosition) => boardState.GetTile(targetPosition).HasPieceState();
}



















































//////////////////////////////////////////

public class Game : MonoBehaviour{
    private GameState state;
    private Board board;
    private Player[] players = new Player[2]; // only 2 playerStates for a chess game

    Piece selectedPiece;

    // Player GUI
    private void UpdateSelectedPiece(PieceState newPieceState)
    {
        // Find the corresponding Piece based on the PieceState
        selectedPiece = FindPieceFromState(newPieceState);
    }
    private Piece FindPieceFromState(PieceState pieceState)
    {
        foreach (Player player in players)
        {
            // Assuming you have access to a list of pieces
            foreach (Piece piece in player.Pieces) // Adjust this as necessary
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

        Vector2Int targetPosition = players[state.PlayerIndex].State.GetMove()[1]; // non-bot playerStates will use GUI so no need for from position
        HashSet<Vector2Int> gameValidMoves = state.GetMovesAllowed(state.SelectedPieceState);
        if(gameValidMoves.Contains(targetPosition)){
            state.ExecuteMove(targetPosition);
            
            //lastMovedPiece = selectedPiece; // Store the last moved piece
        }
        else{
            //selectedPiece.Position = state.OriginalPosition;
            state.SelectedPieceState.Position = state.OriginalPosition; // Reset to original
        }
        
        state.SelectedPieceState = null;
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
        if(state.Checkmate) return; // dont handl user input

        if(players[state.PlayerIndex] is Bot){
            BotMove();
            return;
        }

        if (Utility.MouseDown()) // Left mouse button
            SelectPiece();
        else if (selectedPiece != null)
            HandleDragAndDrop();
    }

    public void InitializeGame(string whitePlayerType, string blackPlayerType, string whitePlayerName, string blackPlayerName)
    {
        InitializePlayers(whitePlayerType, blackPlayerType, whitePlayerName, blackPlayerName);
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
        state.OnSelectedPieceChanged += UpdateSelectedPiece;
        if (P1State is BotState)
            (P1State as BotState).CurrentGame = this.state;
        if (P2State is BotState)
            (P2State as BotState).CurrentGame = this.state;
    }

    private void InitializePlayers(string whitePlayerTypeName, string blackPlayerTypeName, string whitePlayerName, string blackPlayerName)
    {
        Debug.Log("here1 " + whitePlayerTypeName + " " + blackPlayerTypeName);


        PlayerState P1State = CreatePlayerState(whitePlayerTypeName, whitePlayerName, true);
        PlayerState P2State = CreatePlayerState(blackPlayerTypeName, blackPlayerName, false);
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


    private PlayerState CreatePlayerState(string playerTypeName, string playerName, bool isWhite)
    {
        // Use reflection to instantiate the appropriate player state
        var type = System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
            .FirstOrDefault(t => t.Name == $"{playerTypeName}State");
        
        if (type != null)
        {
            PlayerState playerState = (PlayerState)Activator.CreateInstance(type, playerName, isWhite);
            //Debug.Log("Player stement" + playerState+" "+(playerState is AvengerState));
            return playerState;
        }
        return null; // Handle case where type is not found
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
        state.OnSelectedPieceChanged -= UpdateSelectedPiece;
    }
}
