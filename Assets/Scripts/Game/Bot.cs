using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : Player
{
    public override Vector2Int[] GetMove()
    {
        Vector2Int moveFrom = new Vector2Int(3,1), moveTo = new Vector2Int(3,3);
        return new Vector2Int[]{moveFrom, moveTo};
    }
    private void Evaluate()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
