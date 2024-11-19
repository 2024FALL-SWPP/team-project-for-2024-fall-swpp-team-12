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
    private int remainingTurns = 0;

    public bool isMoveComplete = false;

    public TurnLogIterator<(Vector3, bool, int)> stateIterator;
    public TurnLogIterator<string> commandIterator;
    private void Start()
    {
        plateOffPosition = transform.GetChild(1).transform.position;
        plateOnPosition = plateOffPosition + new Vector3(0, -0.1f, 0);

        // Initialize platforms based on their initial state
        targetStates.ForEach(state => state.target.SetActive(state.isInitiallyActive)); 

        var initialState = new List<(Vector3, bool, int)> { (transform.GetChild(1).transform.position, isPressed, remainingTurns) };
        var initialCommands = new List<string>();

        stateIterator = new TurnLogIterator<(Vector3, bool, int)>(initialState);
        commandIterator = new TurnLogIterator<string>(initialCommands);
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
                    remainingTurns--;
                    Debug.Log(remainingTurns);

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

        remainingTurns = resetTurnCount - 1;

        if (!isPressed) isPressed = true;

        // Log button press state
        SaveCurrentState("Activated");

        isMoveComplete = true;
    }

    private void ResetButton()
    {
        targetStates.ForEach(state => state.target.SetActive(state.isInitiallyActive)); // Revert to initial state
        transform.GetChild(1).transform.position = plateOffPosition;

        remainingTurns = 0; //idle state of turnActivated.

        isPressed = false;

        // Log button reset state
        SaveCurrentState("Deactivated");

        isMoveComplete = true;
    }

    private void SaveCurrentState(string command)
    {
        // Log the command and current state
        commandIterator.Add(command);
        stateIterator.Add((transform.GetChild(1).transform.position, isPressed, remainingTurns));
    }

    public void RestoreState()
    {
        var state = stateIterator.Current;
        transform.GetChild(1).transform.position = state.Item1;
        isPressed = state.Item2;
        remainingTurns = state.Item3;

        targetStates.ForEach(state => state.target.SetActive(isPressed ? !state.isInitiallyActive : state.isInitiallyActive));

    }
    public void RemoveLog(int k)
    {
        for (int i = 0; i < k; i++)
        {
            commandIterator.RemoveLast();
            stateIterator.RemoveLast();
        }
    }
}