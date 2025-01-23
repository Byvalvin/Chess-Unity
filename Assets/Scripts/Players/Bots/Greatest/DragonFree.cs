using UnityEngine;

public class Dragon : Engine // Actually Komodo but Dragon sounds wayy cooler
{
    // This will remain mostly unchanged, just like your Leela Bot class.
}

public class DragonState : EngineState{
    // Path to your Dragon executable (adjust the path as needed)
    private const string dragonPath = "ChessEngines/dragon/dragon_05e2a7/Windows/dragon-64bit-avx2"; // Change this to the actual Dragon executable path

    public DragonState(string playerName, bool isWhite) 
        : base(dragonPath, playerName, isWhite) { }

    public DragonState(DragonState original) : base(original) { }

    public override PlayerState Clone() => new DragonState(this);
}





