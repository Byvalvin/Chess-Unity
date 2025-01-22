using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;
using System.IO;

public class Leela : Bot{

}

public class LeelaState : BotState{
    private Process lcZeroProcess;
    private StreamWriter writer;
    private StreamReader reader;

    public LeelaState(string playerName, bool isWhite) : base(playerName, isWhite) {
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
        UnityEngine.Debug.Log("Initialize: "+output);
    }

    public LeelaState(LeelaState original) : base(original) { }

    public override PlayerState Clone() => new LeelaState(this);

    public override Vector2Int GetMove(){
        // Get the board position from the current game state (FEN)
        string fen = CurrentGame.FEN();  // You'll need to ensure this method gives you the FEN string
        SetPosition(fen);

        // Get the best move from LCZero
        string bestMove = GetBestMove();

        UnityEngine.Debug.Log("Move To play: "+bestMove);
        // Convert the best move (e.g., e2e4) to your internal format (e.g., 0, 16)
        Vector2Int move = ConvertMoveToVector(bestMove);
        return move;
    }

    private void SetPosition(string fen)
    {
        // Send the position (FEN string) to LCZero
        SendUciCommand($"position fen {fen}");
    }

    // private string GetBestMove(){
    //     // Ask LCZero for the best move
    //     SendUciCommand("go");
    //     string output = ReadUciOutput();
    //     // Expected output: bestmove e2e4
    //     string bestMove = output.Replace("bestmove ", "").Trim();
    //     return bestMove;
    // }

    private string GetBestMove(){
        // Ask LCZero for the best move
        SendUciCommand("go");
        string output = ReadUciOutput();

        // Loop to read the output until we find the best move
        while (!output.StartsWith("bestmove"))
        {
            output = ReadUciOutput();  // Read next line if it doesn't start with 'bestmove'
        }

        // Extract the best move from the output (it should be the second word after "bestmove")
        string bestMove = output.Replace("bestmove ", "").Trim();

        // Return the best move (e.g., e2e4)
        return bestMove;
    }


    private void SendUciCommand(string command){
        writer.WriteLine(command);
        writer.Flush();
    }

    private string ReadUciOutput()=>reader.ReadLine();

    private Vector2Int ConvertMoveToVector(string move){
        // Convert UCI move notation (e.g., e2e4) into a Vector2Int (e.g., (0, 16))
        // Example: e2e4 -> (12, 28)
        int from = ConvertUciToIndex(move.Substring(0, 2));
        int to = ConvertUciToIndex(move.Substring(2, 2));
        return new Vector2Int(from, to);
    }

    private int ConvertUciToIndex(string uciPosition){
        // Convert UCI notation like "e2" or "d4" to the internal index representation
        char file = uciPosition[0]; // e.g., 'e'
        char rank = uciPosition[1]; // e.g., '2'

        int fileIndex = file - 'a';  // Convert 'a'-'h' to 0-7
        int rankIndex = 8 - (rank - '0');  // Convert '1'-'8' to 7-0

        return rankIndex * 8 + fileIndex;  // Convert to 0-63 index
    }

    public void Close(){
        SendUciCommand("quit");
        writer.Close();
        reader.Close();
        lcZeroProcess.Kill();
    }

}
/*


public class ChessBot
{
    private Process lcZeroProcess;
    private StreamWriter writer;
    private StreamReader reader;

    public ChessBot()
    {
        lcZeroProcess = new Process();
        lcZeroProcess.StartInfo.FileName = "path/to/lc0"; // Path to LCZero executable
        lcZeroProcess.StartInfo.UseShellExecute = false;
        lcZeroProcess.StartInfo.RedirectStandardInput = true;
        lcZeroProcess.StartInfo.RedirectStandardOutput = true;
        lcZeroProcess.StartInfo.CreateNoWindow = true;

        lcZeroProcess.Start();
        writer = lcZeroProcess.StandardInput;
        reader = lcZeroProcess.StandardOutput;
    }

    public string SendUCICommand(string command)
    {
        writer.WriteLine(command);
        writer.Flush();
        return reader.ReadLine();
    }

    public string GetBestMove(string boardPosition)
    {
        // Set the position for the engine
        SendUCICommand("position " + boardPosition);
        SendUCICommand("go");

        // Get the best move
        string bestMove = reader.ReadLine();
        return bestMove?.Replace("bestmove ", "");
    }

    public void Close()
    {
        writer.Close();
        reader.Close();
        lcZeroProcess.Kill();
    }
}








































































using System.Diagnostics;
using System.IO;

public class LeelaChessEngine
{
    private Process engineProcess;
    private StreamWriter inputWriter;
    private StreamReader outputReader;

    public void InitializeEngine(string enginePath)
    {
        engineProcess = new Process();
        engineProcess.StartInfo.FileName = enginePath; // Path to LCZero executable
        engineProcess.StartInfo.UseShellExecute = false;
        engineProcess.StartInfo.RedirectStandardInput = true;
        engineProcess.StartInfo.RedirectStandardOutput = true;
        engineProcess.StartInfo.CreateNoWindow = true;
        engineProcess.Start();

        inputWriter = engineProcess.StandardInput;
        outputReader = engineProcess.StandardOutput;
    }

    public void SendUciCommand(string command)
    {
        inputWriter.WriteLine(command);
        inputWriter.Flush();
    }

    public string ReadUciOutput()
    {
        return outputReader.ReadLine();
    }

    public void SetPosition(string fen)
    {
        SendUciCommand("position fen " + fen);
    }

    public void GetBestMove()
    {
        SendUciCommand("go");
        string bestMove = ReadUciOutput();
        // Parse the output for the best move
    }

    public void QuitEngine()
    {
        SendUciCommand("quit");
        engineProcess.Close();
    }
}

*/