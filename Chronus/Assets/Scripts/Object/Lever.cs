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

    public List<(Quaternion, bool, Vector3)> listLeverStateLog; //(stick bias, isActivated, canToggleDirection)
    public List<string> listLeverCommandLog;

    private void Start()
    {
        // Initial lever rotation
        transform.GetChild(1).transform.localRotation = backwardRotation;
        canToggleDirection = transform.localRotation * Vector3.forward;

        // Initialize platforms based on their initial state
        targetStates.ForEach(state => state.target.SetActive(state.isInitiallyActive)); 

        listLeverCommandLog = new List<string>();
        listLeverStateLog = new List<(Quaternion, bool, Vector3)>() { (transform.GetChild(1).transform.localRotation, isActivated, canToggleDirection) };
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
        listLeverCommandLog.Add(command);
        listLeverStateLog.Add((transform.GetChild(1).transform.localRotation, isActivated, canToggleDirection));
    }

    public void RestoreState(int turnIndex)
    {
        if (turnIndex < listLeverStateLog.Count)
        {
            var state = listLeverStateLog[turnIndex];
            transform.GetChild(1).transform.localRotation = state.Item1;
            isActivated = state.Item2;
            canToggleDirection = state.Item3;

            targetStates.ForEach(state => state.target.SetActive(state.isInitiallyActive ^ isActivated)); 
        }
    }
    public void RemoveLog(int startIndex)
    {
        listLeverCommandLog.RemoveRange(startIndex, listLeverCommandLog.Count - startIndex); // turn 0: no element
        listLeverStateLog.RemoveRange(startIndex + 1, listLeverStateLog.Count - startIndex - 1); // turn 0: one initial element
    }
}
