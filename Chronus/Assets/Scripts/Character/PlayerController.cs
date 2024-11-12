using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CharacterBase
{
    public static PlayerController playerController; 
    private string curKey = "r"; // command log(wasdr)
    public bool isTimeRewinding = false;     
    private int maxTurn = 0; // used for time rewind mode
    
    public List<string> listCommandLog; // command log for time rewind (objects) and phantom action (copy commands)
    public List<(Vector3, Quaternion)> listPosLog; // position tracking log for time rewind (reset position)
    protected override void Awake() // singleton
    {
        base.Awake();
        if (PlayerController.playerController == null) { PlayerController.playerController = this; }
    }
    
    protected override void Start()
    {
        base.Start();

        // getting input from input manager: producing is done below.
        InputManager.inputManager.OnCommand += HandleMovementInput;
        InputManager.inputManager.OnTimeRewindModeToggle += ToggleTimeRewindMode;
        InputManager.inputManager.OnTimeRewindControl += HandleTimeRewindInput;

        listCommandLog = new List<string>(); // command log for time rewind(objects) and phantom action(copy commands)
        listPosLog = new List<(Vector3, Quaternion)> { (playerCurPos, playerCurRot) }; // position tracking log for time rewind and reset position
        // always: listPosLog.Count = 1 + listCommandLog.Count 
    }

    void Update() 
    {
        if (TurnManager.turnManager.turnClock) // when turn is on progress
        {
            sm.IsDoneAction(); 
            if (doneAction) // next state in the state list
            {
                listSeq++; // next index
                doneAction = false;
                if (listSeq < listCurTurn.Count)
                {
                    sm.SetState(listCurTurn[listSeq]);
                }
                else
                {
                    sm.SetState(idle);                    
                    TurnManager.turnManager.dicTurnCheck["Player"] = true;
                }
            }
        }

        sm.DoOperateUpdate();
    }

    protected override void HandleMovementInput(string command) 
    {
        if (TurnManager.turnManager.turnClock || isTimeRewinding) return; // active when the turn is entirely ended, and not in time rewind mode
        curKey = command;
        base.HandleMovementInput(command);
    }

    protected override void StartAction() // when this is called, the global cycle(the turn) starts
    {
        base.StartAction();
        listCommandLog.Add(curKey); // command log update for the phantom
        TurnManager.turnManager.StartTurn();
    }

    // Functions for Time Rewind mode //
    private void ToggleTimeRewindMode()
    {
        if (TurnManager.turnManager.turnClock) return; // assuring that every action should be ended (during the turn)

        if (!isTimeRewinding) // entering time rewind mode: make an empty phantom(actually, the player) in current position 
        {
            maxTurn = TurnManager.turnManager.turn;
            isTimeRewinding = true; 

            // delete already existing phantom, if not ended
            PhantomController.phantomController.isPhantomExisting = false;
            PhantomController.phantomController.gameObject.SetActive(false);
        }
        else // leaving time rewind mode: make a phantom based on the input
        {
            if (TurnManager.turnManager.turn < maxTurn)
            {
                int startIndex = TurnManager.turnManager.turn;
                // actually, this can be implemented in another way: using Instantiate
                // but, there's a problem with it: it has to be "referred" from TurnManager and PlayerController
                // so, at each turn, 2 scripts have to iterate through all objects on the scene and find a phantom.
                // rather, using a singleton object makes it easier to keep track.
                
                // modifying the location
                PhantomController.phantomController.transform.position = listPosLog[startIndex].Item1;
                PhantomController.phantomController.transform.rotation = listPosLog[startIndex].Item2;
                PhantomController.phantomController.playerCurPos = listPosLog[startIndex].Item1;
                PhantomController.phantomController.playerCurRot = listPosLog[startIndex].Item2;
                
                // actually initializing the phantom
                PhantomController.phantomController.gameObject.SetActive(true);
                PhantomController.phantomController.isPhantomExisting = true;
                PhantomController.phantomController.listCommandOrder.Clear();
                PhantomController.phantomController.listCommandOrder.AddRange(listCommandLog.GetRange(startIndex, listCommandLog.Count - startIndex)); //copy!!!
                PhantomController.phantomController.order = 0;

                // deleting list commands from the player, which has been moved to the phantom
                listCommandLog.RemoveRange(startIndex, listCommandLog.Count - startIndex); // turn 0: no element in listCommandLog
                listPosLog.RemoveRange(startIndex + 1, listPosLog.Count - startIndex - 1); // turn 0: one initial element in listPosLog
            }
            isTimeRewinding = false; 
        }
    }

    private void HandleTimeRewindInput(string command)
    {
        if (TurnManager.turnManager.turnClock || !isTimeRewinding) return; // active only in time rewind mode
        switch (command)
        {
            case "q": // go to the 1 turn past
                if (TurnManager.turnManager.turn >= 1)
                {
                    GoToThePastOrFuture(-1);
                }
                else
                {
                    print("Cannot go further to the Past!!!");
                }
                break;

            case "e": // go to the 1 turn future
                if (TurnManager.turnManager.turn <= maxTurn - 1)
                {
                    GoToThePastOrFuture(1);
                }
                else
                {
                    print("Cannot go further to the Future!!!");
                }
                break;
        }
    }
    private void GoToThePastOrFuture(int turnDelta)
    {
        TurnManager.turnManager.turn += turnDelta;
        // position tracking log -> preview the current position(and rotation)
        transform.position = listPosLog[TurnManager.turnManager.turn].Item1;
        transform.rotation = listPosLog[TurnManager.turnManager.turn].Item2;
        playerCurPos = transform.position;
        playerCurRot = transform.rotation;
    }
}