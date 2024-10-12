using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileState
{
    private Vector2Int position;
    private bool colour; // Assuming 'true' is white and 'false' is black
    private float min, max, minx, miny, maxx, maxy;

    public PieceState pieceState;

    public Vector2Int Position
    {
        get => position;
        set => position = value;
    }

    public bool Colour
    {
        get => colour;
        set => colour = value;
    }

    public float Min
    {
        get => min;
        set
        {
            min = value;
            minx = min; miny = min;
        }
    }
    public float Max
    {
        get => max;
        set
        {
            max = value;
            maxx = max; maxy = max;
        }
    }

    public TileState() { }

    // Copy constructor
    public TileState(TileState original)
    {
        this.position = original.position;
        this.colour = original.colour;
        this.Min = original.min;
        this.Max = original.max;
        
        pieceState = original.pieceState?.Clone();
    }

    public TileState Clone() => new TileState(this); // Clone method

    public bool HasPieceState() => pieceState != null;
}

public class Tile : MonoBehaviour
{
    private TileState state;
    private float n; // length of tile
    private Color myColour;
    [SerializeField] private Shader UnlitColorShader; // Assign this in the inspector, a shader to add colour to tiles
    private static Dictionary<bool, Material> tileMaterials = new Dictionary<bool, Material>();
    private Renderer tileRenderer; // Cached Renderer reference

    public Piece piece;

    public TileState State
    {
        get => state;
        set {
            state = value;
            myColour = state.Colour ? Color.white : Color.black;
        }
    }
    
    public float N
    {
        get => n;
        set
        {
            if (state.Min < value && value < state.Max)
            {
                n = value;
                ScaleTile();
            }
        }
    }

    public Color MyColour => myColour;

    private void RenderTileColour()
    {
        if (!tileMaterials.TryGetValue(state.Colour, out Material material))
        {
            material = new Material(UnlitColorShader);
            material.color = myColour;
            tileMaterials[state.Colour] = material;
        }

        tileRenderer.material = material; // Set material from cache
    }

    private void ScaleTile() => transform.localScale = new Vector3(n, n, 1); // Adjust scale for visual representation

    void Awake(){
        UnlitColorShader = Shader.Find("Unlit/Color");
    }

    void Start()
    {
        tileRenderer = GetComponent<Renderer>(); // Cache the Renderer reference
        RenderTileColour();
        ScaleTile();
    }

    void Update()
    {
        // Update tile behavior if needed
    }
}
