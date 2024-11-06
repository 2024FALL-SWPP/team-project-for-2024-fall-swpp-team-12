using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager turnManager; //public accessibility using Singleton

    public int turn; //The turn, starting from savePoint!!! //also index of "player command list"

    /* List of Objects for Turn Check yeah (dunno how to make enum public so let's just handwrite all lol)
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

        savePoint_pos = new Vector3(1, 1, 1);
        savePoint_rot = Quaternion.Euler(new Vector3(0,0,0));
    }

    // Update is called once per frame
    void Update()
    {
        print(turn);
        if (dicTurnCheck["Player"])
        {
            turn += 1; //this turn ended, wait for the next turn!
            dicTurnCheck["Player"] = false;
        }
    }
}
