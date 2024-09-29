using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; // for basic arrays

public class Tile : MonoBehaviour
{
    // Start is called before the first frame update
    private Vector2 position;
    private bool colour;
    private float n; // length of tile
    private float min,max,minx,miny,maxx,maxy;

    private Material tileMaterial; //store for reuse

    public Tile(Tile original){ // copy constructor
        this.position = original.position;
        this.colour = original.colour;
        this.n = original.n;
        this.min = original.min;
        this.max = original.max;
        this.minx = original.minx;
        this.miny = original.miny;
        this.maxx = original.maxx;
        this.maxy = original.maxy;

        // Clone the piece if it exists (assuming Piece has a Clone method)
        this.piece = original.piece != null ? (Piece)Activator.CreateInstance(original.piece.GetType(), original.piece) : null;

        // Note: The material reference may not be duplicated since materials are shared in Unity.
        // If you want to duplicate the material, you could uncomment the following line:
        // this.tileMaterial = new Material(original.tileMaterial);
        
        // Handle material as needed
        /*
        if (original.tileMaterial != null)
        {
            this.tileMaterial = new Material(original.tileMaterial); // Duplicate the material
        }
        else
        {
            this.tileMaterial = null; // If there's no material, set it to null
        }

        // Render the tile color based on the copied properties
        RenderTileColour();
        */
    }

    private void RenderTileColour(){
        if(tileMaterial==null)
        {
            tileMaterial = new Material(Shader.Find("Unlit/Color")); // Use an unlit shader
            GetComponent<Renderer>().material = tileMaterial; // set material once
        }
        tileMaterial.color = colour ? Color.white : Color.black;
    }

    public Piece piece;

    public Vector2 Position{
        get{return position;}
        set{position = value;}
    }
    public bool Colour{
        get{return colour;}
        set{
            colour = value;
            RenderTileColour();
        }
    }
    public float N{
        get{return n;}
        set{
            if(minx<value && value<maxx)
            {
                n=value;
                transform.localScale = new Vector3(n, n, 1); // Adjust scale for visual representation
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

    public bool HasPiece(){
        return piece!=null;
    }
    void Start()
    {
        RenderTileColour();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
