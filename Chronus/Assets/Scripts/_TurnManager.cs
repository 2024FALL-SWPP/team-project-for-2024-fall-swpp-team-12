using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _TurnManager : MonoBehaviour
{
    public static _TurnManager turnManager;

    public int turn = 0;

    public bool CLOCK = false; //god father clock for 1 single turn yeah
    public bool isTimeRewinding = false; //Time Rewinding Mode Toggle

    public bool idleClock = true; //waiting for inputs
    public bool turnClock = false; //action execution (player, phantom, switches, objects, etc)
    public bool firstCollisionCheck = false; //collision check between player and harmful objects (first) (if collide, kill) //plus button state change by condition
    public bool lateTurnClock = false; //late action execution (moving wall(or players pushed by it), block, spear, etc)
    public bool secondCollisionCheck = false; //collision check (second)
    public bool fallClock = false; //make them fall (land, or kill) //like koyote time
    public bool lastCollisionCheck = false; //collision check (last)

    // removed object control for simplicity: this will be done in other branch.

    public Dictionary<string, bool> dicTurnCheck;
    private void Awake() // Singleton
    {
        if (_TurnManager.turnManager == null) { _TurnManager.turnManager = this; }
    }

    void Start()
    {
        dicTurnCheck = new Dictionary<string, bool>();
        dicTurnCheck.Add("Player", false);
        dicTurnCheck.Add("Phantom", false);
        dicTurnCheck.Add("Box", false);
        dicTurnCheck.Add("Button", false);
        dicTurnCheck.Add("Lever", false);
        //+ ...... so on
    }

    void Update()
    {
        if (CLOCK) //whole.
        {
            if (idleClock)
            {
                NextSubClock(ref idleClock, ref turnClock);
            }
            if (turnClock && dicTurnCheck["Player"] && (dicTurnCheck["Phantom"] || !PhantomController.phantomController.isPhantomExisting)
                && dicTurnCheck["Lever"] && dicTurnCheck["Box"]/* && and so on...*/) //action execution (player, phantom, switches, objects, etc)
            {
                NextSubClock(ref turnClock, ref firstCollisionCheck);
                dicTurnCheck["Player"] = false;
                dicTurnCheck["Phantom"] = false;
                dicTurnCheck["Lever"] = false;
                dicTurnCheck["Box"] = false;
                //dicTurnCheck ....... = false;
            }
            if (firstCollisionCheck && dicTurnCheck["Button"]
                /* && something*/) //collision check between player and harmful objects (first) (if collide, kill) //plus button state change by condition
            {
                NextSubClock(ref firstCollisionCheck, ref lateTurnClock);
                dicTurnCheck["Button"] = false;
                //dicTurnCheck ....... = false;
            }
            if (lateTurnClock/* && something*/) //late action execution (moving wall(or players pushed by it), block, spear, etc)
            {
                NextSubClock(ref lateTurnClock, ref secondCollisionCheck);
                //dicTurnCheck ....... = false;
            }
            if (secondCollisionCheck/* && something*/) //collision check (second)
            {
                NextSubClock(ref secondCollisionCheck, ref fallClock);
                //dicTurnCheck ....... = false;
            }
            if (fallClock/* && something*/) //make them fall (land, or kill) //like koyote time
            {
                NextSubClock(ref fallClock, ref lastCollisionCheck);
                //dicTurnCheck ....... = false;
            }
            if (lastCollisionCheck/* && something*/) //collision check (last)
            {
                NextSubClock(ref lastCollisionCheck, ref idleClock);
                //dicTurnCheck ....... = false;
                CLOCK = false;

                turn++;
            }
        }
        else
        {
            //????
        }
    }

    private void NextSubClock(ref bool previousClock, ref bool nextClock)
    {
        previousClock = false;
        nextClock = true;
    }
}