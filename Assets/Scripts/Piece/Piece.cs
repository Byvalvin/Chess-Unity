using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Piece : MonoBehaviour

{
    protected string type;
    protected bool colour;
    protected Color myColour;
    protected Vector2Int currentPos;
    protected HashSet<Vector2Int> validMoves = new HashSet<Vector2Int>();
    protected bool captured = false;
    private Vector2Int purgatory = new Vector2Int(-100,-100); // captured pieces go to purgatory

    protected float tileSize;
    protected Vector2Int minPoint, maxPoint;
    
    protected SpriteRenderer spriteR;
    protected Sprite pieceSprite;
    protected BoxCollider2D pieceCollider;
    protected float pieceColliderSize=1;


    // GUI
    Color lightColour = new Color(1f, 0.95f, 0.8f, 1f), // Cream
     darkColour = new Color(0.3f, 0.3f, 0.3f, 1f); // Charcoal
    
    /*
    Color lightColour = new Color(0.9f, 0.9f, 0.9f, 1f), // Very Light Gray
    darkColour = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark Gray;

    Color lightColour = new Color(1f, 0.95f, 0.8f, 1f), // Cream
     darkColour = new Color(0.3f, 0.3f, 0.3f, 1f); // Charcoal

    Color lightColour = new Color(1f, 0.94f, 0.8f, 1f); // Soft Beige
     darkColour = new Color(0.4f, 0.26f, 0.2f, 1f); // Soft Brown
    */

    protected void SetSprite()
    {
        if(pieceSprite!=null)
        {
            spriteR.sprite = pieceSprite;
            spriteR.color = colour? lightColour : darkColour;
        }
    }
    
    protected abstract void SetValidMoves();
    protected void SetPosition()
    {
        transform.position = new Vector3(tileSize*currentPos.x, tileSize*currentPos.y, 0);
        
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
    public Vector2Int Position
    {
        get{return currentPos;}
        set{
            currentPos=value;
            SetValidMoves();
            SetPosition();
        }
    }
    public string Type
    {
        get{return type;} 
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
    public HashSet<VectorInt2> ValidMoves
    {
        get{return validMoves;}
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

    public abstract bool CanMove(Vector2Int to); // checks if a piece can move to tile at to
    public virtual void Move(Vector2Int to) // just used tp update state since move check done on board
    {
        currentPos = to;
        SetPosition();

    }

    // GUI
    protected bool InBounds(Vector2Int pos)=>Utility.InBounds(minPoint, maxPoint, pos);

    public virtual void HandleInput() 
    {
    }

    

    // Start and Update methods can be overridden by derived classes as needed
    protected virtual void Start() 
    {
        spriteR = gameObject.AddComponent<SpriteRenderer>();

        pieceCollider = gameObject.AddComponent<BoxCollider2D>();
        pieceCollider.size = new Vector2(pieceColliderSize,pieceColliderSize); // need a pieceCollider

     }
    protected virtual void Update() 
    {
        HandleInput(); 
    }
}
