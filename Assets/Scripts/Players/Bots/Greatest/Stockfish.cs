using UnityEngine;

public class Stockfish : Engine
{
    // This will remain mostly unchanged, just like your Leela Bot class.
}

public class StockfishState : EngineState{
    private const string stockfishPath = "ChessEngines/stockfish-windows-x86-64-sse41-popcnt/stockfish/stockfish-windows-x86-64-sse41-popcnt";

    public StockfishState(string playerName, bool isWhite) 
        : base(stockfishPath, playerName, isWhite) { }

    public StockfishState(StockfishState original) : base(original) { }

    public override PlayerState Clone() => new StockfishState(this);

}





