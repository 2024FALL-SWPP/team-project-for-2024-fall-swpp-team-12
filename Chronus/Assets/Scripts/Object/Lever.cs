using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public List<TargetObjectState> targetStates; //pair platforms with their initial states
    private bool isActivated = false;
    private Quaternion forwardRotation = Quaternion.Euler(130, 0, 0);
    private Quaternion backwardRotation = Quaternion.Euler(50, 0, 0);
    public Vector3 canToggleDirection;

    public bool doPushLever = false;
    public bool isMoveComplete = false;

    public TurnLogIterator<(Quaternion, bool, Vector3)> stateIterator;
    public TurnLogIterator<string> commandIterator;

    private void Start()
    {
        // Initial lever rotation
        transform.GetChild(1).transform.localRotation = backwardRotation;
        canToggleDirection = transform.localRotation * Vector3.forward;

        // Initialize platforms based on their initial state
        targetStates.ForEach(state => state.target.SetActive(state.isInitiallyActive)); 

        var initialState = new List<(Quaternion, bool, Vector3)>
        {
            (transform.GetChild(1).transform.rotation, isActivated, canToggleDirection)
        };
        var initialCommands = new List<string>{""};

        stateIterator = new TurnLogIterator<(Quaternion, bool, Vector3)>(initialState);
        commandIterator = new TurnLogIterator<string>(initialCommands);
    }

    public void AdvanceTurn()
    {
        if (doPushLever)
        {
            doPushLever = false;
            ToggleLever();
        }
        else
        {
            if (!isMoveComplete)
            {
                SaveCurrentState("No Update");
                isMoveComplete = true;
            }
        }
    }

    public void ToggleLever()
    {
        isActivated = !isActivated;
        transform.GetChild(1).transform.localRotation = isActivated ? forwardRotation : backwardRotation;
        canToggleDirection = -canToggleDirection;

        targetStates.ForEach(state => state.target.SetActive(state.isInitiallyActive ^ isActivated)); 

        // Log lever toggle state
        SaveCurrentState(isActivated ? "Activate" : "Deactivate");

        isMoveComplete = true;
    }

    private void SaveCurrentState(string command)
    {
        // Log the command and current state
        commandIterator.Add(command);
        stateIterator.Add((transform.GetChild(1).transform.rotation, isActivated, canToggleDirection));
    }

    public void RestoreState()
    {
        var state = stateIterator.Current;
        transform.GetChild(1).transform.rotation = state.Item1;
        isActivated = state.Item2;
        canToggleDirection = state.Item3;

        targetStates.ForEach(state =>
        state.target.SetActive(state.isInitiallyActive ^ isActivated));
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
