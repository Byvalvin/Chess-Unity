using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; // for basic arrays


public class TileState
{
    // Start is called before the first frame update
    private Vector2 position;
    private bool colour;
    private float min,max,minx,miny,maxx,maxy;

    public PieceState pieceState;

    public Vector2 Position{
        get{return position;}
        set{position = value;}
    }

    public bool Colour{
        get{return colour;}
        set{colour = value;}
    }

    public float Min{
        get{return min;}
        set{
            min=value;
            minx=min; miny=min;
        }
    }
    public float Max{
        get{return max;}
        set{
            max=value;
            maxx=max; maxy=max;
        }
    }

    public TileState(){}

    // Copy constructor
    public TileState(TileState original)
    {
        this.position = original.position;
        this.colour = original.colour;
        this.Min = original.min;
        this.Max = original.max;
        
        // Clone the piece state if it exists
        pieceState = original.pieceState?.Clone();
    }

    public TileState Clone() => new TileState(this); // Clone method

    public bool HasPieceState(){
        return pieceState!=null;
    }
}

public class Tile : MonoBehaviour
{
    TileState state;
    private float n; // length of tile
    private Material tileMaterial; //store for reuse

    public Piece piece;
    
    public TileState State{
        get=>state;
        set=>state=value;
    }
    public float N{
        get{return n;}
        set{
            if(state.Min<value && value<state.Max){
                n=value;
                //ScaleTile();
            }
        }
    }
    private void RenderTileColour(){
        if(tileMaterial==null)
        {
            tileMaterial = new Material(Shader.Find("Unlit/Color")); // Use an unlit shader
            GetComponent<Renderer>().material = tileMaterial; // set material once
        }
        tileMaterial.color = state.Colour ? Color.white : Color.black;
    }

    private void ScaleTile()=>transform.localScale = new Vector3(n, n, 1); // Adjust scale for visual representation


    void Start()
    {
        RenderTileColour();
        ScaleTile();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
