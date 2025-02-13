using UnityEngine;
using Unity.Barracuda;
using System.IO;
using System.Collections.Generic;

public class Pride_Sorrow : Trained{}

public class Pride_SorrowState : TrainedState
{
    private const string morphy_path = "Morphy_space1";
    
    // Constructor for the Pride_SorrowState class
    public Pride_SorrowState(string playerName, bool isWhite) : base(morphy_path, playerName, isWhite){}
    // Copy constructor for cloning
    public Pride_SorrowState(Pride_SorrowState original) : base(original){}

    // Override the Clone method
    public override PlayerState Clone()=>new Pride_SorrowState(this);


}
