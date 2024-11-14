using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager turnManager;

    public int turn = 0;

    public bool CLOCK = false;

    public bool idleClock = true;
    public bool turnClock = false;
    public bool firstCollisionCheck = false;
    public bool lateTurnClock = false;
    public bool secondCollisionCheck = false;
    public bool fallClock = false;
    public bool lastCollisionCheck = false;

    // removed object control for simplicity: this will be done in other branch.

    public Dictionary<string, bool> dicTurnCheck;
    private void Awake() // Singleton
    {
        if (TurnManager.turnManager == null) { TurnManager.turnManager = this; }
    }

    void Start()
    {
        dicTurnCheck = new Dictionary<string, bool>();
        dicTurnCheck.Add("Player", false);
        dicTurnCheck.Add("Phantom", false);
        dicTurnCheck.Add("Box", false);
        dicTurnCheck.Add("Button", false);
        //+ mirror phantom, button, lever, box, ...... so on
    }

    void Update()
    {
        if (CLOCK) //whole.
        {
            if (idleClock)
            {
                NextSubClock(ref idleClock, ref turnClock);
            }
            if (turnClock && dicTurnCheck["Player"] && (dicTurnCheck["Phantom"] || !PlayerController.playerController.isPhantomExists)/* && and so on...*/) //action execution (player, phantom, switches, objects, etc)
            {
                NextSubClock(ref turnClock, ref firstCollisionCheck);
                dicTurnCheck["Player"] = false;
                dicTurnCheck["Phantom"] = false;
            }
            if (firstCollisionCheck && dicTurnCheck["Button"]/* && something*/) //collision check between player and harmful objects (first) (if collide, kill)
            {
                NextSubClock(ref firstCollisionCheck, ref lateTurnClock);
                dicTurnCheck["Button"] = false;
                //dicTurnCheck ....... = false;
            }
            if (lateTurnClock/* && something*/) //late action execution (moving wall, block, spear, etc)
            {
                NextSubClock(ref lateTurnClock, ref secondCollisionCheck);
                //dicTurnCheck ....... = false;
            }
            if (secondCollisionCheck/* && something*/) //collision check between player and harmful objects (second) (if collide, kill)
            {
                NextSubClock(ref secondCollisionCheck, ref fallClock);
                //dicTurnCheck ....... = false;
            }
            if (fallClock/* && something*/) //make them fall (land, or kill)
            {
                NextSubClock(ref fallClock, ref lastCollisionCheck);
                //dicTurnCheck ....... = false;
            }
            if (lastCollisionCheck/* && something*/) //collision check between player and harmful objects (last) (if collide, kill)
            {
                NextSubClock(ref lastCollisionCheck, ref idleClock);
                //dicTurnCheck ....... = false;
                CLOCK = false;

                turn++;
            }
        }
    }

    private void NextSubClock(ref bool previousClock, ref bool nextClock)
    {
        previousClock = false;
        nextClock = true;
    }
}