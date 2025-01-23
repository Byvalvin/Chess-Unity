using UnityEngine;

public class Leela : Engine
{
    // Your bot class can be expanded later with more logic if needed.
}

public class LeelaState : EngineState{
    private const string leelaPath = "ChessEngines/lc0-v0.31.2-windows-cpu-dnnl/lc0";

    public LeelaState(string playerName, bool isWhite) 
        : base(leelaPath, playerName, isWhite) { }

    public LeelaState(LeelaState original) : base(original) { }

    public override PlayerState Clone() => new LeelaState(this);
}






