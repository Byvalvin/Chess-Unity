using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class Komodo : Bot
{
    // This will be your Komodo bot class.
}

public class KomodoState : BotState
{
    private Process komodoProcess;
    private StreamWriter writer;
    private StreamReader reader;

    // Define internal variables to hold calculation parameters
    private int timeLimitInMs;
    private int maxDepth;
    private int maxNodes;

    public KomodoState(string playerName, bool isWhite) : base(playerName, isWhite)
    {
        // Path to your Komodo executable (adjust the path as needed)
        string komodoPath = "C:/Users/cruzmart/Daniel/dragon/dragon_05e2a7/Windows/dragon-64bit-avx2"; // Change this to the actual Komodo executable path

        // Initialize the Komodo process
        komodoProcess = new Process();
        komodoProcess.StartInfo.FileName = komodoPath;
        komodoProcess.StartInfo.UseShellExecute = false;
        komodoProcess.StartInfo.RedirectStandardInput = true;
        komodoProcess.StartInfo.RedirectStandardOutput = true;
        komodoProcess.StartInfo.CreateNoWindow = true;

        komodoProcess.Start();
        writer = komodoProcess.StandardInput;
        reader = komodoProcess.StandardOutput;

        // Initialize Komodo with UCI command
        SendUciCommand("uci");
        string output = ReadUciOutput(); // Read the response to confirm initialization
        UnityEngine.Debug.Log("Komodo Initialized: " + output);
    }

    public KomodoState(KomodoState original) : base(original) { }

    public override PlayerState Clone() => new KomodoState(this);

    public override Vector2Int GetMove()
    {
        // Set dynamic calculation parameters based on the game state (e.g., CurrentGame.MoveCount)
        SetCalculationParameters(CurrentGame.MoveCount);

        // Get the board position from the current game state (FEN)
        string fen = CurrentGame.FEN(); // Ensure this method gives you the FEN string
        SetPosition(fen);

        // Get the best move from Komodo
        string bestMove = GetBestMove();

        UnityEngine.Debug.Log("Move to play: " + bestMove);

        // Convert the best move (e.g., e2e4) to your internal format (e.g., 0, 16)
        Vector2Int move = ConvertMoveToVector(bestMove);

        // Check if the move is a promotion (e.g., "e7e8q")
        if (bestMove.Length == 5) // A promotion move will have 5 characters like "e7e8q"
        {
            PromoteTo = Char.ToUpper(bestMove[bestMove.Length - 1]);
        }

        UnityEngine.Debug.Log("To vector:");
        UnityEngine.Debug.Log(move);
        return move;
    }

    private void SetPosition(string fen)
    {
        // Send the position (FEN string) to Komodo
        SendUciCommand($"position fen {fen}");
    }

    private string GetBestMove()
    {
        // Send Komodo command to start calculating the best move
        SendUciCommand($"go movetime {timeLimitInMs} depth {maxDepth} nodes {maxNodes}");

        string output = ReadUciOutput();

        // Loop to read the output until we find the best move
        while (output != null && !output.StartsWith("bestmove"))
        {
            output = ReadUciOutput();  // Read the next line if it doesn't start with 'bestmove'
        }

        // If we find a line that starts with "bestmove", extract the move
        if (output != null && output.StartsWith("bestmove"))
        {
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
            maxDepth = 15;        // Deeper search for openings
            maxNodes = 1000000;   // Search through more nodes in the opening
        }
        else if (moveCount <= 40) // Midgame
        {
            timeLimitInMs = 3000; // 3 seconds per move
            maxDepth = 12;        // Moderate depth for tactical analysis
            maxNodes = 500000;
        }
        else // Endgame
        {
            timeLimitInMs = 1500; // 1.5 seconds per move
            maxDepth = 10;
            maxNodes = 300000;
        }

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
        int from = ConvertUciToIndex(move.Substring(0, 2));
        int to = ConvertUciToIndex(move.Substring(2, 2));
        return new Vector2Int(from, to);
    }

    private int ConvertUciToIndex(string uciPosition)
    {
        char file = uciPosition[0];
        char rank = uciPosition[1];

        int fileIndex = file - 'a'; 
        int rankIndex = rank - '1'; 

        return rankIndex * 8 + fileIndex;
    }

    public override void Close()
    {
        SendUciCommand("quit");
        writer.Close();
        reader.Close();
        komodoProcess.Kill();
    }
}
