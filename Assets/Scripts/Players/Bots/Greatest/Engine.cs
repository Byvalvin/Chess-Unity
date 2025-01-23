using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class Engine : Bot
{
    // This will remain mostly unchanged, just like your Leela Bot class.
}

public abstract class EngineState : BotState
{
    protected Process engineProcess;
    protected StreamWriter writer;
    protected StreamReader reader;

    // Define internal variables to hold calculation parameters
    protected int timeLimitInMs;
    protected int maxDepth;
    protected int maxNodes;

    protected string enginePath;

    // Constructor that takes the engine's executable path, player name, and color
    public EngineState(string enginePath, string playerName, bool isWhite) : base(playerName, isWhite)
    {
        this.enginePath = Path.Combine(Application.streamingAssetsPath, enginePath);
        // // Replace backslashes with forward slashes
        this.enginePath = this.enginePath.Replace("\\", "/");

        // Initialize the engine process
        engineProcess = new Process();
        engineProcess.StartInfo.FileName = this.enginePath;
        engineProcess.StartInfo.UseShellExecute = false;
        engineProcess.StartInfo.RedirectStandardInput = true;
        engineProcess.StartInfo.RedirectStandardOutput = true;
        engineProcess.StartInfo.CreateNoWindow = true;

        engineProcess.Start();
        writer = engineProcess.StandardInput;
        reader = engineProcess.StandardOutput;

        // Initialize engine with UCI command
        SendUciCommand("uci");
        string output = ReadUciOutput(); // Read response to confirm initialization
        UnityEngine.Debug.Log("Engine Initialized: " + output);
    }

    public EngineState(EngineState original) : base(original)
    {
        this.enginePath = original.enginePath;
        this.timeLimitInMs = original.timeLimitInMs;
        this.maxDepth = original.maxDepth;
        this.maxNodes = original.maxNodes;
        this.writer = original.writer;
        this.reader = original.reader;
        this.engineProcess = original.engineProcess;
    }

    public abstract override PlayerState Clone();

    // Method to set the position (FEN string) in the engine
    protected void SetPosition(string fen)=>SendUciCommand($"position fen {fen}");
    

    // Method to get the best move from the engine
    protected string GetBestMove()
    {
        SendUciCommand($"go movetime {timeLimitInMs} depth {maxDepth} nodes {maxNodes}");

        string output = ReadUciOutput();

        // Loop to read the output until we find the best move
        while (output != null && !output.StartsWith("bestmove"))
            output = ReadUciOutput();  // Read next line if it doesn't start with 'bestmove'

        // If we find a line that starts with "bestmove", extract the move
        if (output != null && output.StartsWith("bestmove"))
        {
            string bestMove = output.Replace("bestmove ", "").Split(' ')[0].Trim();
            return bestMove;
        }

        return string.Empty; // If no best move found, return empty
    }

    // Common method for setting the dynamic calculation parameters
    protected void SetCalculationParameters(int moveCount)
    {
        if (moveCount <= 10) // Early game
        {
            timeLimitInMs = 2000;
            maxDepth = 15;
            maxNodes = 1000000;
        }
        else if (moveCount <= 40) // Midgame
        {
            timeLimitInMs = 3000;
            maxDepth = 12;
            maxNodes = 500000;
        }
        else // Endgame
        {
            timeLimitInMs = 1500;
            maxDepth = 10;
            maxNodes = 300000;
        }

        UnityEngine.Debug.Log($"Set Calculation Parameters -> Time: {timeLimitInMs} ms, Depth: {maxDepth}, Nodes: {maxNodes}");
    }

    // Method to send a UCI command
    protected void SendUciCommand(string command)
    {
        writer.WriteLine(command);
        writer.Flush();
    }

    // Method to read a line of UCI output
    protected string ReadUciOutput() => reader.ReadLine();

    // Convert a UCI move to the internal board index
    protected Vector2Int ConvertMoveToVector(string move)
    {
        int from = ConvertUciToIndex(move.Substring(0, 2));
        int to = ConvertUciToIndex(move.Substring(2, 2));
        return new Vector2Int(from, to);
    }

    // Convert a UCI position like "e2" to the internal board index
    protected int ConvertUciToIndex(string uciPosition){
        char file = uciPosition[0];
        char rank = uciPosition[1];

        int fileIndex = file - 'a'; 
        int rankIndex = rank - '1'; 

        return rankIndex * 8 + fileIndex;
    }

    // // Override this method in subclasses to return the best move
    public override Vector2Int GetMove(){
        string fen = CurrentGame.FEN();
        SetPosition(fen);

        SetCalculationParameters(CurrentGame.MoveCount);

        string bestMove = GetBestMove();
        UnityEngine.Debug.Log("Best move: " + bestMove);

         // Check if the move is a promotion (e.g., "e7e8q")
        if (bestMove.Length == 5){// A promotion move will have 5 characters like "e7e8q"
            PromoteTo = Char.ToUpper(bestMove[bestMove.Length - 1]);
        }

        return ConvertMoveToVector(bestMove);
    }

    public override void Close(){
        SendUciCommand("quit");
        writer.Close();
        reader.Close();
        engineProcess.Kill();
    }
}
