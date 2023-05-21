using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataObj
{
    public string Level;
    public string wins;
    public string Losses;

    public string Queue;
    public string BoardType;
 
}

public class GlobalUserData
{
    public string UserID;
 
}


public class ChessDataObj {
    //Player Account
    public string flag;
    public string description;


    // Player Level and ratings
    public string ChessElo;
    public string ChessLevel;
    public string ChessSkill;

    //game results
    public string chesswin;
    public string chessloses;
    public string chessDraw;
    public int AILeveling;

}
