using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSwitch : MonoBehaviour
{
    public GameObject[] platforms;
    private bool[] boolPalette;
    private int idx;
    private Vector3 plateOffPosition;
    private Vector3 plateOnPosition;

    private bool doPressButton = false;
    public bool isPressed = false;

    public int resetTurnCount = 9; //treated as constant (hyperparameter)
    public int turnActivated = -1; //variable.

    // Time rewind logs
    public List<(Vector3, bool, int)> listButtonStateLog; // (extrude/intrude, isPressed, turnActivated)
    public List<string> listButtonCommandLog; //visualizing detail for us, developers.
    private int maxTurn = 0; //Time Rewinding Mode
    private int curTurn = 0; //for simultaneous(parallel) Time Rewinding Mode managing, local(private) turn variable is need for each object.
    private bool isTimeRewinding = false; //for simultaneous(parallel) Time Rewinding Mode managing, local(private) turn variable is need for each object.

    private void Start()
    {
        InputManager.inputManager.OnTimeRewindModeToggle += ToggleTimeRewindModeForButton;
        InputManager.inputManager.OnTimeRewindControl += HandleTimeRewindInputForButton;

        plateOffPosition = this.transform.GetChild(1).transform.position;
        plateOnPosition = plateOffPosition + new Vector3(0,-0.1f,0);

        boolPalette = new bool[1];
        boolPalette[0] = true;

        idx = 0;
        foreach (GameObject platform in platforms)
        {
            platform.SetActive(boolPalette[idx] ^ true);
            idx++;
        }

        listButtonCommandLog = new List<string>();
        listButtonStateLog = new List<(Vector3, bool, int)>() { (this.transform.GetChild(1).transform.position, isPressed, turnActivated) };
    }

  

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Box")) //update when firstCollisionCheck
        {
            doPressButton = true; //start pressing the button
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Box")) //update when firstCollisionCheck
        {
            doPressButton = true; //if player stands still on the button -> keep pressing
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Box")) //update when firstCollisionCheck
        {
            doPressButton = false; //left the button
        }
    }

    private void Update()
    {
        if (TurnManager.turnManager.firstCollisionCheck) //update when firstCollisionCheck
        {
            if (doPressButton)
            {
                doPressButton = false;
                PressButton(); //turnActivated update.
            }
            else
            {
                if (!TurnManager.turnManager.dicTurnCheck["Button"])
                {
                    if (isPressed)
                    {
                        int remainingTurns = (turnActivated + resetTurnCount) - TurnManager.turnManager.turn - 1;
                        if (remainingTurns <= 0)
                        {
                            ResetButton();
                        }
                        else
                        {
                            SaveCurrentState($"{remainingTurns}");
                            TurnManager.turnManager.dicTurnCheck["Button"] = true;
                        }
                    }
                    else
                    {
                        SaveCurrentState("No Update");
                        TurnManager.turnManager.dicTurnCheck["Button"] = true;
                    }
                }     
            } 
        }

    }

    private void PressButton()
    {
        idx = 0;
        foreach (GameObject platform in platforms)
        {
            platform.SetActive(boolPalette[idx] ^ false);
            idx++;
        }
        this.transform.GetChild(1).transform.position = plateOnPosition;

        turnActivated = TurnManager.turnManager.turn + 1; //get value before turn update, so +1 (indicates next turn)

        if (!isPressed) isPressed = true;

        // Log button press state
        SaveCurrentState("Activated");

        TurnManager.turnManager.dicTurnCheck["Button"] = true;
    }

    private void ResetButton()
    {
        idx = 0;
        foreach (GameObject platform in platforms)
        {
            platform.SetActive(boolPalette[idx] ^ true);
            idx++;
        }
        this.transform.GetChild(1).transform.position = plateOffPosition;

        turnActivated = -1; //idle state of turnActivated.

        isPressed = false;

        // Log button reset state
        SaveCurrentState("Deactivated");

        TurnManager.turnManager.dicTurnCheck["Button"] = true;
    }

    private void SaveCurrentState(string command)
    {
        // Log the command and current state
        listButtonCommandLog.Add(command);
        listButtonStateLog.Add((this.transform.GetChild(1).transform.position,  isPressed, turnActivated));
    }

    public void RestoreState(int turnIndex)
    {
        var state = listButtonStateLog[turnIndex];
        this.transform.GetChild(1).transform.position = state.Item1;
        isPressed = state.Item2;
        turnActivated = state.Item3;

        idx = 0;
        foreach (GameObject platform in platforms)
        {
            platform.SetActive(boolPalette[idx] ^ !isPressed);
            idx++;
        }

    }
    public void RemoveLog(int startIndex) {
        listButtonCommandLog.RemoveRange(startIndex, listButtonCommandLog.Count - startIndex); // turn 0: no element
        listButtonStateLog.RemoveRange(startIndex + 1, listButtonStateLog.Count - startIndex - 1); // turn 0: one initial element
    }


    // Functions for Time Rewind //
    private void ToggleTimeRewindModeForButton()
    {
        if (TurnManager.turnManager.CLOCK) return; //***** active when Not executing Actions.

        if (!isTimeRewinding) //when OFF
        {
            maxTurn = TurnManager.turnManager.turn;
            curTurn = maxTurn;
            isTimeRewinding = true; //toggle ON
        }
        else //when ON
        {
            if (curTurn < maxTurn)
            {
                RemoveLog(curTurn);
            }
            isTimeRewinding = false; //toggle OFF
        }
    }



    private void HandleTimeRewindInputForButton(string command)
    {
        if (TurnManager.turnManager.CLOCK || !isTimeRewinding) return; //***** active when Time Rewinding Mode.
        switch (command)
        {
            case "q": // go to the Past (turn -1)
                if (curTurn >= 1)
                {
                    GoToThePastOrFuture(-1);
                }
                break;

            case "e": // go to the Future (turn +1)
                if (curTurn <= maxTurn - 1)
                {
                    GoToThePastOrFuture(1);
                }
                break;
        }
    }
    private void GoToThePastOrFuture(int turnDelta)
    {
        curTurn += turnDelta;
        //position tracking log -> preview the current position(and rotation)
        RestoreState(curTurn);
    }
}