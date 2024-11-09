using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager turnManager; 
    public int turn = 0; 
    public bool turnClock;
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
    }

    void Update()
    {
        if (dicTurnCheck["Player"])// && dicTurnCheck["Phantom"]) 
        // temporary... need to check if all actions are done.
        {
            EndTurn();
            dicTurnCheck["Player"] = false;
            dicTurnCheck["Phantom"] = false;
        }
    }

    public void StartTurn() // This is called at PlayerController
    {
        if (!turnClock) { turnClock = true; }
    }

    private void EndTurn()
    {
        if (turnClock)
        {
            turnClock = false;
            turn++;
        }
    }
}