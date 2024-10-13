using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq; // Add this line for LINQ

public static class Objects
{
    public static PlayerState CreatePlayerState(string playerTypeName, string playerName, bool isWhite, string filePath)
    {
        // Use reflection to instantiate the appropriate player state
        var type = System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
            .FirstOrDefault(t => t.Name == $"{playerTypeName}State");
        
        if (type != null)
        {
            PlayerState playerState = (playerTypeName=="Preset")? 
                new PresetState(playerName, isWhite, filePath) 
                : 
                (PlayerState)Activator.CreateInstance(type, playerName, isWhite);
            //Debug.Log("Player stement" + playerState+" "+(playerState is AvengerState));
            return playerState;
        }
        return null; // Handle case where type is not found
    }

    public static PieceState CreatePieceState(string pieceTypeName, bool _colour, Vector2Int _currentPos, Vector2Int _minPoint, Vector2Int _maxPoint)
    {
        // Use Type.GetType to get the type of the piece state dynamically
        Type pieceStateType = Type.GetType(pieceTypeName + "State"); // Assumes the class names are in the format "KingState", "QueenState", etc.
        if (pieceStateType == null){
            Debug.LogError($"Could not find type: {pieceTypeName}State");
            return null;
        }

        // Create an instance of the PieceState using reflection
        PieceState pieceState = (PieceState)Activator.CreateInstance(pieceStateType, new object[] { _colour, _currentPos, _minPoint, _maxPoint });
        if (pieceState == null){
            Debug.LogError($"Failed to create instance of type: {pieceTypeName}State");
            return null;
        }
        return pieceState;
    }

    public static Piece CreatePiece(string objectName, string pieceTypeName, PieceState pieceState, float tileSize, int x, int y, float pieceScaleFactor){
        GameObject PieceObject = new GameObject(objectName);
        // Convert the type string to a Type object
        Type pieceType = Type.GetType(pieceTypeName);
        Piece piece = PieceObject.AddComponent(pieceType) as Piece;
        if (piece == null){
            Debug.LogError($"Failed to add component of type: {pieceTypeName}");
            return null;
        }
        
        piece.State = pieceState;
        piece.TileSize = tileSize;
        piece.PieceSprite = Board.sprites[$"{pieceTypeName}"];
        piece.PieceColliderSize = 1/pieceScaleFactor;

        // Set UI
        PieceObject.transform.position = new Vector3(x * tileSize, y * tileSize, 0);
        PieceObject.transform.localScale = new Vector3(tileSize * pieceScaleFactor, tileSize * pieceScaleFactor, 1); // Adjust based on sprite size

        return piece;

    }

}
