using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CharacterBase
{
    public static PlayerController playerController; // singleton
    private string curKey = "r"; //Command Log, it will be written on.
    public bool isTimeRewinding = false; //Time Rewinding Mode Toggle
    private int maxTurn = 0; //Time Rewinding Mode
    
    public List<string> listCommandLog; //command log for time reverse(objects) and phantom action(copy commands)
    public List<(Vector3, Quaternion)> listPosLog; //position tracking log for time reverse(reset position)
    protected override void Awake() // Singleton
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

        listCommandLog = new List<string>(); //command log for time rewind(objects) and phantom action(copy commands)
        listPosLog = new List<(Vector3, Quaternion)> { (playerCurPos, playerCurRot) }; //position tracking log for time rewind and reset position
        // always: listPosLog.Count = 1 + listCommandLog.Count ***
    }

    // [Update() of InputManager.cs]: Input&Condition Check
    // <-> [Update() of PlayerController.cs]: Action of this Turn(State Change)
    // <-> [Update() of InputManager.cs]: Time Rewind
    void Update() //Action of this Turn(State Change)
    {
        if (TurnManager.turnManager.turnClock) //when turn clock in ON.
        {
            sm.IsDoneAction(); 
            if (doneAction) //next state in the state list
            {
                seq++; //next index
                doneAction = false;
                if (seq < listCurTurn.Count)
                {
                    sm.SetState(listCurTurn[seq]);
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

    protected override void StartAction() // when this is called, the global cycle(the turn) starts.
    {
        base.StartAction();
        listCommandLog.Add(curKey); // command log update for the phantom.
        TurnManager.turnManager.StartTurn();
    }

    // Functions for Time Rewind mode //
    private void ToggleTimeRewindMode()
    {
        if (TurnManager.turnManager.turnClock) return; // assuring that every action should be ended (during the turn)

        if (!isTimeRewinding) // entering time rewind mode: make an empty phantom(actually, the player) in current position 
        {
            PhantomController.phantomController.listCommandOrder.Clear();
            PhantomController.phantomController.gameObject.SetActive(false);
            PhantomController.phantomController.transform.position = this.listPosLog[0].Item1 + new Vector3(0, -4, 0);
            PhantomController.phantomController.transform.rotation = this.listPosLog[0].Item2;
            PhantomController.phantomController.playerCurPos = PhantomController.phantomController.transform.position;
            PhantomController.phantomController.playerCurRot = PhantomController.phantomController.transform.rotation;
            //Destroy(phantomInstance);
            PhantomController.phantomController.isPhantomExisting = false;
            maxTurn = TurnManager.turnManager.turn;
            isTimeRewinding = true; 
        }
        else // leaving time rewind mode: make a phantom based on the input
        {
            if (TurnManager.turnManager.turn < maxTurn)
            {
                int startIndex = TurnManager.turnManager.turn;
                PhantomController.phantomController.transform.position = this.listPosLog[startIndex].Item1;
                PhantomController.phantomController.transform.rotation = this.listPosLog[startIndex].Item2;
                PhantomController.phantomController.playerCurPos = PhantomController.phantomController.transform.position;
                PhantomController.phantomController.playerCurRot = PhantomController.phantomController.transform.rotation;
                PhantomController.phantomController.gameObject.SetActive(true);
                //phantomInstance = Instantiate(phantomPrefab, listPosLog[startIndex].Item1, listPosLog[startIndex].Item2);
                //phantomScript = phantomInstance.GetComponent<PhantomController>();
                PhantomController.phantomController.isPhantomExisting = true;
                // Make Copy list of Command Orders for phantom
                PhantomController.phantomController.listCommandOrder.AddRange(listCommandLog.GetRange(startIndex, listCommandLog.Count - startIndex)); //copy!!!
                PhantomController.phantomController.order = 0;
                //phantomScript.listCommandOrder.AddRange(listCommandLog.GetRange(startIndex, listCommandLog.Count - startIndex)); //copy!!!
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
        //print(TurnManager.turnManager.turn);
        //position tracking log -> preview the current position(and rotation)
        this.transform.position = listPosLog[TurnManager.turnManager.turn].Item1;
        this.transform.rotation = listPosLog[TurnManager.turnManager.turn].Item2;
        playerCurPos = this.transform.position;
        playerCurRot = this.transform.rotation;
    }
}