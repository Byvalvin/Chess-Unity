using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class PresetState : BotState
{
    static NotationParser presetNotationParser = null;
    
    private int moveIndex = 0;
    private List<string> MoveList; // List to store move strings

    public PresetState(string _playerName, bool _colour, string filePath) : base(_playerName, _colour)
    {
        MoveList = new List<string>();
        LoadMovesFromFile(filePath);
        if(presetNotationParser==null) presetNotationParser = new NotationParser(this);
    }

    public PresetState(PresetState original) : base(original)
    {
        moveIndex = original.moveIndex;
        MoveList = new List<string>(original.MoveList);
    }
    public override PlayerState Clone() => new PresetState(this);

    public override Vector2Int[] GetMove() // uses no evaluation
    {
        if (moveIndex < MoveList.Count)
        {
            string moveString = MoveList[moveIndex]; // Get the next move string

            // set promotion choice for promotion
            bool isPromotion = moveString.Contains('=');
            if(isPromotion) PromoteTo=NotationParser.pieceTypeNotationMap[moveString[moveString.Length-1]];
            moveIndex++;
            return ParseMove(moveString); // Parse it into Vector2Int[]
        }
        else
        {
            Debug.Log("No more moves available.");
            return null; // or return a default move
        }
    }

    private void LoadMovesFromFile(string filePath)
    {
        // Load the file and read moves line by line
        string[] lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                var columns = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                string moveString = this.Colour ? columns[0] : columns[1];
                MoveList.Add(moveString.Trim()); // Add the move string directly
            }
        }
    }

    private Vector2Int[] ParseMove(string move)=>presetNotationParser.ParseMove(move);

}

public class Preset : Bot
{
    protected override void Awake()
    {
        //state = new PresetState();
    }
    
    // Start is called before the first frame update
    protected override void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }
}