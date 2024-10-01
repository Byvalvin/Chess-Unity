using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class PieceState {
    protected string type;
    protected bool colour;
    protected Vector2Int currentPos;
    protected static Vector2Int minPoint, maxPoint;
    public HashSet<Vector2Int> validMoves = new HashSet<Vector2Int>();
    public bool captured = false;
    public static Vector2Int purgatory = new Vector2Int(-100,-100); // captured pieces go to purgatory
    public bool firstMove = true;




    public PieceState(bool _colour, Vector2Int _currentPos, Vector2Int _minPoint, Vector2Int _maxPoint){
        colour=_colour; currentPos=_currentPos; minPoint=_minPoint; maxPoint=_maxPoint;
    }

    // Copy constructor
    public PieceState(PieceState original) {
        colour = original.Colour; // also set myColour
        //myColour = original.myColour;
        currentPos = original.currentPos;
        captured = original.captured;

        firstMove = original.firstMove;
        validMoves = new HashSet<Vector2Int>(original.validMoves);
    }

    public bool Captured
    {
        get{return captured;}
        set
        {
            captured=value;
            if(captured) Position=purgatory;
        }
    }

    // Getters and Setters
    public bool FirstMove{
        get=>firstMove;
    }
    public Vector2Int Position
    {
        get{return currentPos;}
        set{
            currentPos=value;
        }
    }
    public string Type
    {
        get{return type;}
        protected set{type=value;} 
    }
    public bool Colour
    {
        get{return colour;}
        set
        {
            colour=value;
            //myColour=colour? lightColour:darkColour;
        }
    }
    public HashSet<Vector2Int> ValidMoves
    {
        get{return validMoves;}
        set{validMoves=value;}
    }



    public Vector2Int MinPoint{
        get{return minPoint;}
        set{minPoint=value;}
    }
    public Vector2Int MaxPoint{
        get{return maxPoint;}
        set{maxPoint=value;}
    }

    public void ResetValidMoves()=>SetValidMoves();
    public virtual void Move(Vector2Int to) {
        Position = to;
        firstMove = false; // Mark as moved
    }


    protected abstract void SetValidMoves();
    public abstract bool CanMove(Vector2Int to);
    public abstract PieceState Clone(); 


    protected bool InBounds(Vector2Int pos)=>Utility.InBounds(minPoint, maxPoint, pos);
    protected HashSet<Vector2Int> FindAll(HashSet<Vector2Int> moveSet) => Utility.FindAll<Vector2Int>(moveSet, CanMove);
}

public abstract class Piece : MonoBehaviour {
    // Core variables
    protected PieceState state;
    protected SpriteRenderer spriteR;
    protected Sprite pieceSprite;
    protected Color myColour;
    protected BoxCollider2D pieceCollider;
    protected float pieceColliderSize = 1;
    protected float tileSize;

    static Color[] LightColors = {
        new Color(1f, 0.95f, 0.8f, 1f), // Cream
        new Color(0.9f, 0.9f, 0.9f, 1f), // Very Light Gray
        new Color(1f, 0.94f, 0.8f, 1f), // Soft Beige
        new Color(1f, 1f, 0.8f, 1f), // Soft Yellow
        new Color(0.9f, 0.8f, 1f, 1f), // Light Purple
        new Color(0.8f, 1f, 1f, 1f), // Soft Cyan
        new Color(1f, 0.8f, 0.7f, 1f), // Soft Peach
        new Color(0.8f, 1f, 0.8f, 1f)  // Soft Green
    };

    static Color[] DarkColors = {
        new Color(0.3f, 0.3f, 0.3f, 1f), // Charcoal
        new Color(0.2f, 0.2f, 0.2f, 1f), // Dark Gray
        new Color(0.4f, 0.26f, 0.2f, 1f), // Soft Brown
        new Color(0.2f, 0.2f, 0.2f, 1f), // Dark Charcoal
        new Color(0.1f, 0.3f, 0.5f, 1f), // Very Dark Blue
        new Color(0.1f, 0.3f, 0.1f, 1f), // Dark Green
        new Color(0.4f, 0.2f, 0.1f, 1f), // Rich Brown
        new Color(0.4f, 0.4f, 0.5f, 1f)  // Dark Slate Gray
    };

    static int colourIndex = -1; // Will generate same index for all pieces once
    Color lightColour, darkColour;

    
    public PieceState State{
        get=>state;
        set{
            state=value;
            SetPosition();
        }
    }
    public Sprite PieceSprite{
        get{return pieceSprite;}
        set{pieceSprite=value;}
    }
    public Color MyColour
    {
        get{return myColour;}
        set{myColour=value;}
    }

    public float PieceColliderSize{
        get{return pieceColliderSize;}
        set{pieceColliderSize=value;}
    }

    public float TileSize{
        get{return tileSize;}
        set{tileSize=value;}
    }


   protected void SetSprite(){
        if(pieceSprite!=null){
            spriteR.sprite = pieceSprite;
            spriteR.color = myColour;
        }
    }
    protected void SetPosition() {
        transform.position = new Vector3(tileSize * state.Position.x, tileSize * state.Position.y, 0);
    }

    public virtual void Move(Vector2Int to) {
        if (state.CanMove(to)) {
            state.Move(to);
            SetPosition();
        }
    }

    public virtual void HandleInput() {
        // Handle input logic
    }


    protected virtual void Awake() {
        if (colourIndex == -1) // Generate once
            colourIndex = Random.Range(0, LightColors.Length);
        lightColour = LightColors[colourIndex];
        darkColour = DarkColors[colourIndex];

    }
    // Start method can be overridden by derived classes as needed
    protected virtual void Start() {
        spriteR = gameObject.AddComponent<SpriteRenderer>();

        pieceCollider = gameObject.AddComponent<BoxCollider2D>();
        pieceCollider.size = new Vector2(pieceColliderSize, pieceColliderSize); // need a pieceCollider

        SetPosition();
    }

    protected virtual void Update() {
        HandleInput();
    }

}
