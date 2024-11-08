using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager turnManager; //public accessibility using Singleton

    public int turn; //The turn, starting from savePoint!!! //also index of "player command list"

    /* List of Objects for Turn Check (dunno how to get public enum by singleton so let's just handwrite all lol)
        Player,
        Phantom,
        MirrorPhantom,
        Button,
        Lever,
        Platform,
        Box
    */

    public Dictionary<string, bool> dicTurnCheck;

    public Vector3 savePoint_pos;
    public Quaternion savePoint_rot;
    // Start is called before the first frame update

    private void Awake() //Singleton
    {
        if(TurnManager.turnManager == null)
        {
            TurnManager.turnManager = this;
        }
    }

    void Start()
    {

        turn = 0; //0 means player is at the savePoint

        dicTurnCheck = new Dictionary<string, bool>();
        dicTurnCheck.Add("Player", false);
        dicTurnCheck.Add("Phantom", true);
        dicTurnCheck.Add("MirrorPhantom", true);
        dicTurnCheck.Add("Button", true);
        dicTurnCheck.Add("Lever", true);
        dicTurnCheck.Add("Platform", true);
        dicTurnCheck.Add("Box", true);

        savePoint_pos = new Vector3(1, 1, 1);
        savePoint_rot = Quaternion.Euler(new Vector3(0,0,0));
    }

    // Update is called once per frame
    void Update()
    {
        //print(turn);
        if (dicTurnCheck["Player"]) //Player Leads Turn Control.
        {
            bool ph = dicTurnCheck["Phantom"];
            bool mph = dicTurnCheck["MirrorPhantom"];
            bool bt = dicTurnCheck["Button"];
            bool l = dicTurnCheck["Lever"];
            bool pf = dicTurnCheck["Platform"];
            bool bx = dicTurnCheck["Box"];
            if (ph && mph && bt && l && pf && bx)
            {
                turn += 1; //this turn ended, wait for the next turn!
                PlayerController.playerController.turnClock = false;
                dicTurnCheck["Player"] = false;
            }
        }
    }
}