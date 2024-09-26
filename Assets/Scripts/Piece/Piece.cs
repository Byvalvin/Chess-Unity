using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Piece : MonoBehaviour

{
    // core variables
    protected string type;
    protected bool colour;
    protected Color myColour;
    protected Vector2Int currentPos;
    protected HashSet<Vector2Int> validMoves = new HashSet<Vector2Int>(), possibleMoves = new HashSet<Vector2Int>();
    protected bool captured = false;
    public static Vector2Int purgatory = new Vector2Int(-100,-100); // captured pieces go to purgatory

    protected float tileSize;
    protected Vector2Int minPoint, maxPoint;
    private bool firstMove = true;
    


    // GUI and display variable
    protected SpriteRenderer spriteR;
    protected Sprite pieceSprite;
    protected BoxCollider2D pieceCollider;
    protected float pieceColliderSize=1;
    /*
        Color lightColour = new Color(1f, 0.95f, 0.8f, 1f), // Cream
        darkColour = new Color(0.3f, 0.3f, 0.3f, 1f); // Charcoal
    */
    static Color[] LightColors = {
        new Color(1f, 0.95f, 0.8f, 1f), // Cream
        new Color(0.9f, 0.9f, 0.9f, 1f), // Very Light Gray
        new Color(1f, 0.94f, 0.8f, 1f), // Soft Beige
        new Color(1f, 1f, 0.8f, 1f), // Soft Yellow
        new Color(0.9f, 0.8f, 1f, 1f), // Light Purple
        new Color(0.8f, 1f, 1f, 1f), // Soft Cyan
        new Color(1f, 0.8f, 0.7f, 1f), // Soft Peach
        new Color(0.8f, 1f, 0.8f, 1f)  // Soft Green
    }
    ,   DarkColors = {
        new Color(0.3f, 0.3f, 0.3f, 1f), // Charcoal
        new Color(0.2f, 0.2f, 0.2f, 1f), // Dark Gray
        new Color(0.4f, 0.26f, 0.2f, 1f), // Soft Brown
        new Color(0.2f, 0.2f, 0.2f, 1f), // Dark Charcoal
        new Color(0.1f, 0.3f, 0.5f, 1f), // Very Dark Blue
        new Color(0.1f, 0.3f, 0.1f, 1f), // Dark Green
        new Color(0.4f, 0.2f, 0.1f, 1f), // Rich Brown
        new Color(0.4f, 0.4f, 0.5f, 1f)  // Dark Slate Gray
    };
    
    static int colourIndex = -1; // will generate same index for all pieces once
    Color lightColour, darkColour;

    // setup and update functions
    protected void SetSprite()
    {
        if(pieceSprite!=null)
        {
            spriteR.sprite = pieceSprite;
            spriteR.color = myColour;
        }
    }
    
    protected abstract void SetValidMoves();
    protected void SetPosition()
    {
        transform.position = new Vector3(tileSize*currentPos.x, tileSize*currentPos.y, 0);
        
    }
    protected HashSet<Vector2Int> FindAll(HashSet<Vector2Int> moveSet) => Utility.FindAll<Vector2Int>(moveSet, CanMove);

    public void ResetValidMoves() => SetValidMoves();

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
            SetPosition();
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
            myColour=colour? lightColour:darkColour;
        }
    }
    public HashSet<Vector2Int> ValidMoves
    {
        get{return validMoves;}
        set{validMoves=value;}
    }
    public Color MyColour
    {
        get{return myColour;}
        set{myColour=value;}
    }

    public float TileSize{
        get{return tileSize;}
        set{tileSize=value;}
    }

    public Vector2Int MinPoint{
        get{return minPoint;}
        set{minPoint=value;}
    }
    public Vector2Int MaxPoint{
        get{return maxPoint;}
        set{maxPoint=value;}
    }

    public Sprite PieceSprite{
        get{return pieceSprite;}
        set{pieceSprite=value;}
    }

    public float PieceColliderSize{
        get{return pieceColliderSize;}
        set{pieceColliderSize=value;}
    }

    // abstract functions sub classes must implement
    public abstract bool CanMove(Vector2Int to); // checks if a piece can move to tile at to
    public virtual void Move(Vector2Int to) // just used tp update state since move check done on board, make sure to call this base.Move() in sub classes if it is being overidden
    {
        Position = to;
        firstMove = false; // Mark as moved
    }

    // GUI
    protected bool InBounds(Vector2Int pos)=>Utility.InBounds(minPoint, maxPoint, pos);

    public virtual void HandleInput() 
    {
    }

    // Unity setup and game loop
    protected virtual void Awake(){
        if(colourIndex==-1) // generate once
            colourIndex = Random.Range(0, LightColors.Length);  
        lightColour=LightColors[colourIndex];
        darkColour=DarkColors[colourIndex]; 
    }

    // Start and Update methods can be overridden by derived classes as needed
    protected virtual void Start() 
    {
        spriteR = gameObject.AddComponent<SpriteRenderer>();

        pieceCollider = gameObject.AddComponent<BoxCollider2D>();
        pieceCollider.size = new Vector2(pieceColliderSize,pieceColliderSize); // need a pieceCollider

        SetPosition();
        //SetValidMoves();

     }
    protected virtual void Update() 
    {
        HandleInput(); 
    }
}
