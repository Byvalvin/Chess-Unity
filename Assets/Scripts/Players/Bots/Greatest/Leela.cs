using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;
using System.IO;

public class Leela : Bot
{
    // Your bot class can be expanded later with more logic if needed.
}

public class LeelaState : BotState
{
    private Process lcZeroProcess;
    private StreamWriter writer;
    private StreamReader reader;

    // Define internal variables to hold calculation parameters
    private int timeLimitInMs;
    private int maxDepth;
    private int maxNodes;

    public LeelaState(string playerName, bool isWhite) : base(playerName, isWhite)
    {
        string leelaPath = "C:/Users/cruzmart/Daniel/lc0-v0.31.2-windows-cpu-dnnl/lc0";
        lcZeroProcess = new Process();
        lcZeroProcess.StartInfo.FileName = leelaPath; // Path to LCZero executable
        lcZeroProcess.StartInfo.UseShellExecute = false;
        lcZeroProcess.StartInfo.RedirectStandardInput = true;
        lcZeroProcess.StartInfo.RedirectStandardOutput = true;
        lcZeroProcess.StartInfo.CreateNoWindow = true;

        lcZeroProcess.Start();
        writer = lcZeroProcess.StandardInput;
        reader = lcZeroProcess.StandardOutput;

        // Initialize LCZero with UCI
        SendUciCommand("uci");
        string output = ReadUciOutput(); // Read response to confirm initialization
        UnityEngine.Debug.Log("Initialize: " + output);
    }

    public LeelaState(LeelaState original) : base(original) { }

    public override PlayerState Clone() => new LeelaState(this);

    public override Vector2Int GetMove()
    {
        // Set dynamic calculation parameters based on the game state (e.g., CurrentGame.MoveCount)
        SetCalculationParameters(CurrentGame.MoveCount);

        // Get the board position from the current game state (FEN)
        string fen = CurrentGame.FEN();  // You'll need to ensure this method gives you the FEN string
        SetPosition(fen);

        // Get the best move from LCZero
        string bestMove = GetBestMove();

        UnityEngine.Debug.Log("Move To play: " + bestMove);
        

        // bestMove = bestMove.Substring(0, 4); // Remove the promotion info (e.g., "e7e8q" -> "e7e8")
        Vector2Int move = ConvertMoveToVector(bestMove); // Convert the best move (e.g., e2e4) to your internal format (e.g., 0, 16)

        // Check if the move is a promotion
        if (bestMove.Length == 5) // A promotion move will have 5 characters like "e7e8q"
        {
            // Handle promotion move, strip the promotion character
            // set Leela's choice of promotion
            PromoteTo = Char.ToUpper(bestMove[bestMove.Length-1]);
            
        }
        UnityEngine.Debug.Log("To vectore:");
        
        UnityEngine.Debug.Log(move);
        return move;
    }

    private void SetPosition(string fen)
    {
        // Send the position (FEN string) to LCZero
        SendUciCommand($"position fen {fen}");
    }

    private string GetBestMove(){
        // Send LCZero command to start calculating the best move with dynamic parameters
        SendUciCommand($"go movetime {timeLimitInMs} depth {maxDepth} nodes {maxNodes}");

        string output = ReadUciOutput();

        // Loop to read the output until we find the best move
        while (output != null && !output.StartsWith("bestmove"))
        {
            output = ReadUciOutput();  // Read next line if it doesn't start with 'bestmove'
        }

        // If we find a line that starts with "bestmove", extract the move
        if (output != null && output.StartsWith("bestmove")){
            // Extract the best move from the output (it should be the second word after "bestmove")
            string bestMove = output.Replace("bestmove ", "").Split(' ')[0].Trim();

            return bestMove;
        }

        // In case the output is malformed or no best move is found, return a default value (optional)
        return string.Empty;
    }


    private void SetCalculationParameters(int moveCount)
    {
        // Dynamic heuristic based on the move count

        if (moveCount <= 10) // Early game (e.g., opening)
        {
            timeLimitInMs = 2000; // 2 seconds per move
            maxDepth = 15;        // Deeper search to explore opening positions
            maxNodes = 1000000;   // Search through a decent amount of nodes for openings
        }
        else if (moveCount <= 40) // Midgame (more complex positions)
        {
            timeLimitInMs = 3000; // 3 seconds per move
            maxDepth = 12;        // Moderate depth for tactical analysis
            maxNodes = 500000;    // Fewer nodes than opening but still solid
        }
        else // Endgame (fewer pieces, more accurate calculation needed)
        {
            timeLimitInMs = 1500; // 1.5 seconds per move
            maxDepth = 10;        // Lower depth as the game is simpler
            maxNodes = 300000;    // Reduce node count to speed up the endgame calculation
        }

        // Optional: Print debug information for tuning
        UnityEngine.Debug.Log($"Set Calculation Parameters -> Time: {timeLimitInMs} ms, Depth: {maxDepth}, Nodes: {maxNodes}");
    }

    private void SendUciCommand(string command)
    {
        writer.WriteLine(command);
        writer.Flush();
    }

    private string ReadUciOutput() => reader.ReadLine();

    private Vector2Int ConvertMoveToVector(string move)
    {
        // Convert UCI move notation (e.g., e2e4) into a Vector2Int (e.g., (0, 16))
        int from = ConvertUciToIndex(move.Substring(0, 2));
        int to = ConvertUciToIndex(move.Substring(2, 2));
        return new Vector2Int(from, to);
    }

    private int ConvertUciToIndex(string uciPosition)
    {
        // Convert UCI notation like "e2" or "d4" to the internal index representation
        char file = uciPosition[0]; // e.g., 'e'
        char rank = uciPosition[1]; // e.g., '2'

        int fileIndex = file - 'a';  // Convert 'a'-'h' to 0-7
        int rankIndex = rank - '1';  // Convert '1'-'8' to 7-0
        // UnityEngine.Debug.Log(fileIndex);
        // UnityEngine.Debug.Log(rankIndex);

        return rankIndex * 8 + fileIndex;  // Convert to 0-63 index
    }

    public override void Close()
    {
        SendUciCommand("quit");
        writer.Close();
        reader.Close();
        lcZeroProcess.Kill();
    }
}






