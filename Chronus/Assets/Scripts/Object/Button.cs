using System;
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
    public bool isPressed = false;

    public int resetTurnCount = 1; //treated as constant (hyperparameter)
    private int remainingTurns = 0;

    public bool isMoveComplete = false;

    public TurnLogIterator<(Vector3, bool, int)> stateIterator;
    private void Start()
    {
        plateOffPosition = transform.GetChild(1).transform.position;
        plateOnPosition = plateOffPosition + new Vector3(0, -0.15f, 0);

        InitializeLog();
    }

    private void Update()
    {
        
    }

    public void InitializeLog()
    {
        targetStates.ForEach(state => state.target.SetActive(state.isInitiallyActive)); 

        var initialState = new List<(Vector3, bool, int)> { (transform.GetChild(1).transform.position, isPressed, remainingTurns) };

        stateIterator = new TurnLogIterator<(Vector3, bool, int)>(initialState);
    }
    public void ResetToStart()
    {
        stateIterator.ResetToStart();
        RestoreState();
    }

    private void BePressedByObjects(Collider other)
    {
        if (!TurnManager.turnManager.CLOCK) return;
        if (other.CompareTag("Player") || other.CompareTag("Box"))
        {
            PressButton();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        BePressedByObjects(other);
    }
    private void OnTriggerStay(Collider other)
    {
        BePressedByObjects(other);
    }
    private void OnTriggerExit(Collider other)
    {
        remainingTurns--; //reason: (because of stay(->turncount reset), turncount don't decrease by itself, so when exit decrease.)
    }

    public void AdvanceTurn()
    {
        if (isPressed)
        {
            Debug.Log("remainingTurns:" + remainingTurns);

            if (remainingTurns <= 0) ResetButton();
            remainingTurns--;
        }
        StartCoroutine(WaitForOthersMoveCompleteAndAdvance());
    }

    private IEnumerator WaitForOthersMoveCompleteAndAdvance()
    {
        // Wait until Others' isMoveComplete becomes true (including fall)
        yield return new WaitUntil(() => TurnManager.turnManager.CheckMoveCompleteExceptButton());

        isMoveComplete = true;
    }

    private void PressButton()
    {
        remainingTurns = resetTurnCount; //reset count (when enter, stay)

        if (!isPressed) {
            targetStates.ForEach(state => state.target.SetActive(!state.isInitiallyActive)); // Toggle state
            transform.GetChild(1).transform.position = plateOnPosition;

            isPressed = true;
            Debug.Log("pressed");
        }
    }

    private void ResetButton()
    {
        targetStates.ForEach(state => state.target.SetActive(state.isInitiallyActive)); // Revert to initial state
        transform.GetChild(1).transform.position = plateOffPosition;

        //need particle

        isPressed = false;
    }

    public void SaveCurrentState()
    {
        // Log the current state
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
        stateIterator.RemoveLastK(k);
    }
}