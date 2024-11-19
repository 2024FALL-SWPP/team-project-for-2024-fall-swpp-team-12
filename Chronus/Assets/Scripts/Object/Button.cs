using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TargetObjectState
{
    public GameObject target;
    public bool isInitiallyActive; //true means the platform starts inactive
}

public class Button : MonoBehaviour
{
    public List<TargetObjectState> targetStates; //pair platforms with their initial states
    private Vector3 plateOffPosition;
    private Vector3 plateOnPosition;

    private bool doPressButton = false;
    public bool isPressed = false;

    public int resetTurnCount = 1; //treated as constant (hyperparameter)
    public int turnActivated = -1; //variable.

    public bool isMoveComplete = false;
    public List<(Vector3, bool, int)> listButtonStateLog; // (extrude/intrude, isPressed, turnActivated)
    public List<string> listButtonCommandLog; //visualizing detail for us, developers.
    private void Start()
    {
        plateOffPosition = transform.GetChild(1).transform.position;
        plateOnPosition = plateOffPosition + new Vector3(0, -0.1f, 0);

        // Initialize platforms based on their initial state
        targetStates.ForEach(state => state.target.SetActive(state.isInitiallyActive)); 

        listButtonCommandLog = new List<string>();
        listButtonStateLog = new List<(Vector3, bool, int)>() { (transform.GetChild(1).transform.position, isPressed, turnActivated) };
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Box")) //update when firstCollisionCheck
        {
            PressButton();
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Box")) //update when firstCollisionCheck
        {
            PressButton();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Box")) //update when firstCollisionCheck
        {
            doPressButton = false; //left the button
        }
    }

    public void AdvanceTurn()
    {
        if (doPressButton)
        {
            doPressButton = false;
            isMoveComplete = true;
        }
        else
        {
            if (!isMoveComplete)
            {
                if (isPressed)
                {
                    int remainingTurns = (turnActivated + resetTurnCount) - TurnManager.turnManager.rewindTurnCount - 1;
                    if (remainingTurns <= 0)
                    {
                        ResetButton();
                    }
                    else
                    {
                        SaveCurrentState($"{remainingTurns}");
                        isMoveComplete = true;
                    }
                }
                else
                {
                    SaveCurrentState("No Update");
                    isMoveComplete = true;
                }
            }
        }
    }

    private void PressButton()
    {
        doPressButton = true;
        targetStates.ForEach(state => state.target.SetActive(!state.isInitiallyActive)); // Toggle state
        transform.GetChild(1).transform.position = plateOnPosition;

        turnActivated = TurnManager.turnManager.rewindTurnCount; //get value before turn update, so +1 (indicates next turn)

        if (!isPressed) isPressed = true;

        // Log button press state
        SaveCurrentState("Activated");

        isMoveComplete = true;
    }

    private void ResetButton()
    {
        targetStates.ForEach(state => state.target.SetActive(state.isInitiallyActive)); // Revert to initial state
        transform.GetChild(1).transform.position = plateOffPosition;

        turnActivated = -1; //idle state of turnActivated.

        isPressed = false;

        // Log button reset state
        SaveCurrentState("Deactivated");

        isMoveComplete = true;
    }

    private void SaveCurrentState(string command)
    {
        // Log the command and current state
        listButtonCommandLog.Add(command);
        listButtonStateLog.Add((transform.GetChild(1).transform.position, isPressed, turnActivated));
    }

    public void RestoreState(int turnIndex)
    {
        var state = listButtonStateLog[turnIndex];
        transform.GetChild(1).transform.position = state.Item1;
        isPressed = state.Item2;
        turnActivated = state.Item3;

        targetStates.ForEach(state => state.target.SetActive(isPressed ? !state.isInitiallyActive : state.isInitiallyActive));

    }
    public void RemoveLog(int startIndex)
    {
        listButtonCommandLog.RemoveRange(startIndex, listButtonCommandLog.Count - startIndex); // turn 0: no element
        listButtonStateLog.RemoveRange(startIndex + 1, listButtonStateLog.Count - startIndex - 1); // turn 0: one initial element
    }
}