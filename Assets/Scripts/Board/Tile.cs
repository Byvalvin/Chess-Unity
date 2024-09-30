using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; // for basic arrays


public class TileState
{
    // Start is called before the first frame update
    private Vector2 position;
    private bool colour;
    private float n; // length of tile
    private float min,max,minx,miny,maxx,maxy;

    
    public Piece piece;

    public Vector2 Position{
        get{return position;}
        set{position = value;}
    }

    
    public bool Colour{
        get{return colour;}
        set{colour = value;}
    }
    public float N{
        get{return n;}
        set{
            if(minx<value && value<maxx){
                n=value;
            }
        }
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

    public bool HasPiece(){
        return piece!=null;
    }
}

public class Tile : MonoBehaviour
{
    TileState state;
    private Material tileMaterial; //store for reuse

    public TileState State{
        get=>state;
        set=>state=value;
    }
    private void RenderTileColour(){
        if(tileMaterial==null)
        {
            tileMaterial = new Material(Shader.Find("Unlit/Color")); // Use an unlit shader
            GetComponent<Renderer>().material = tileMaterial; // set material once
        }
        tileMaterial.color = state.Colour ? Color.white : Color.black;
    }

    private void ScaleTile()=>transform.localScale = new Vector3(state.N, state.N, 1); // Adjust scale for visual representation


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
