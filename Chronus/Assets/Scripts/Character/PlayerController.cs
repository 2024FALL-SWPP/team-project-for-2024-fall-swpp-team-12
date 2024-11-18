using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CharacterBase
{
    public static PlayerController playerController; 
    private string curKey = "r"; // command log(wasdr)
    public bool isTimeRewinding = false;     
    public int maxTurn = 0; // used for time rewind mode
    
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

        InitializePositionLog();
    }

    public void InitializePositionLog()
    {
        listCommandLog = new List<string>(); // command log for time rewind(objects) and phantom action(copy commands)
        listPosLog = new List<(Vector3, Quaternion)> { (playerCurPos, playerCurRot) }; // position tracking log for time rewind and reset position
        // always: listPosLog.Count = 1 + listCommandLog.Count
    }

    protected override void Update() 
    {
        base.Update();
    }

    protected override void HandleMovementInput(string command) 
    {
        if (TurnManager.turnManager.CLOCK || isTimeRewinding) return; // active when the turn is entirely ended, and not in time rewind mode
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
        if (TurnManager.turnManager.CLOCK) return; // assuring that every action should be ended (during the turn)

        if (!isTimeRewinding) 
        {
            TurnManager.turnManager.EnterTimeRewind();
        }
        else 
        {
            TurnManager.turnManager.LeaveTimeRewind();
        }
    }

    private void HandleTimeRewindInput(string command)
    {
        if (TurnManager.turnManager.CLOCK || !isTimeRewinding) return; // active only in time rewind mode
        switch (command)
        {
            case "q": // go to the 1 turn past
                if (TurnManager.turnManager.turn >= 1)
                {
                    TurnManager.turnManager.GoToThePastOrFuture(-1);
                }
                else
                {
                    print("Cannot go further to the Past!!!");
                }
                break;

            case "e": // go to the 1 turn future
                if (TurnManager.turnManager.turn <= maxTurn - 1)
                {
                    TurnManager.turnManager.GoToThePastOrFuture(1);
                }
                else
                {
                    print("Cannot go further to the Future!!!");
                }
                break;
        }
    }
}