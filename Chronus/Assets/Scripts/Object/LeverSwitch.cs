using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverSwitch : MonoBehaviour
{
    public GameObject[] platforms;
    private bool[] boolPalette;
    private int idx;

    private bool isActivated = false;
    private Quaternion forwardRotation; 
    private Quaternion backwardRotation;
    public Vector3 canToggleDirection;

    public bool doPushLever = false;

    // Time rewind logs
    public List<(Quaternion, bool, Vector3)> listLeverStateLog; //(stick bias, isActivated, canToggleDirection)
    public List<string> listLeverCommandLog;
    private int maxTurn = 0; //Time Rewinding Mode
    private int curTurn = 0; //for simultaneous(parallel) Time Rewinding Mode managing, local(private) turn variable is need for each object.
    private bool isTimeRewinding = false; //for simultaneous(parallel) Time Rewinding Mode managing, local(private) turn variable is need for each object.

    private void Start()
    {
        InputManager.inputManager.OnTimeRewindModeToggle += ToggleTimeRewindModeForLever;
        InputManager.inputManager.OnTimeRewindControl += HandleTimeRewindInputForLever;

        forwardRotation = Quaternion.Euler(130, 0, 0);
        backwardRotation = Quaternion.Euler(50, 0, 0);

        // Initial lever rotation
        this.transform.GetChild(1).transform.rotation = backwardRotation;
        canToggleDirection = this.transform.forward;

        boolPalette = new bool[1];
        boolPalette[0] = true;

        idx = 0;
        foreach (GameObject platform in platforms)
        {
            platform.SetActive(boolPalette[idx] ^ !isActivated);
            idx++;
        }


        listLeverCommandLog = new List<string>();
        listLeverStateLog = new List<(Quaternion, bool, Vector3)>() { (transform.GetChild(1).transform.rotation, isActivated, canToggleDirection) };
    }

    private void Update()
    {
        if (TurnManager.turnManager.turnClock)
        {
            if (doPushLever)
            {
                doPushLever = false;
                ToggleLever();
            }
            else
            {
                if (!TurnManager.turnManager.dicTurnCheck["Lever"])
                {
                    SaveCurrentState("No Update");
                    TurnManager.turnManager.dicTurnCheck["Lever"] = true;
                }
            }
        }
    }

    public void ToggleLever()
    {
        isActivated = !isActivated;
        this.transform.GetChild(1).transform.rotation = isActivated ? forwardRotation : backwardRotation;
        canToggleDirection = -canToggleDirection;
        idx = 0;
        foreach (GameObject platform in platforms)
        {
            platform.SetActive(boolPalette[idx] ^ !isActivated);
            idx++;
        }

        // Log lever toggle state
        SaveCurrentState(isActivated ? "Activate" : "Deactivate");

        TurnManager.turnManager.dicTurnCheck["Lever"] = true;
    }

    private void SaveCurrentState(string command)
    {
        // Log the command and current state
        listLeverCommandLog.Add(command);
        listLeverStateLog.Add((transform.GetChild(1).transform.rotation, isActivated, canToggleDirection));
    }

    public void RestoreState(int turnIndex)
    {
        if (turnIndex < listLeverStateLog.Count)
        {
            var state = listLeverStateLog[turnIndex];
            transform.GetChild(1).transform.rotation = state.Item1;
            isActivated = state.Item2;
            canToggleDirection = state.Item3;

            idx = 0;
            foreach (GameObject platform in platforms)
            {
                platform.SetActive(boolPalette[idx] ^ !isActivated);
                idx++;
            }
        }
    }
    public void RemoveLog(int startIndex)
    {
        listLeverCommandLog.RemoveRange(startIndex, listLeverCommandLog.Count - startIndex); // turn 0: no element
        listLeverStateLog.RemoveRange(startIndex + 1, listLeverStateLog.Count - startIndex - 1); // turn 0: one initial element
    }

    // Functions for Time Rewind //
    private void ToggleTimeRewindModeForLever()
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



    private void HandleTimeRewindInputForLever(string command)
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
