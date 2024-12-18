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
    private Vector3 plateOffPosition, plateOnPosition;
    public bool isPressed = false;
    public int resetTurnCount = 1; //treated as constant (hyperparameter)
    private int remainingTurns = 0;
    public bool isMoveComplete = false;
    private bool willKeepPress = false;
    private Collider tempCollider;
    public TurnLogIterator<(Vector3, bool, int)> stateIterator;
    private void Start()
    {
        plateOffPosition = transform.GetChild(1).transform.position;
        plateOnPosition = plateOffPosition + new Vector3(0, -0.15f, 0);

        InitializeLog();
    }

    public void InitializeLog()
    {
        targetStates.ForEach(state => state.target.SetActive(state.isInitiallyActive));

        var initialState = new List<(Vector3, bool, int)> { (transform.GetChild(1).transform.position, isPressed, remainingTurns) };

        stateIterator = new TurnLogIterator<(Vector3, bool, int)>(initialState);
    }
    public void ResetToStart()
    {
        if (stateIterator == null)
        {
            InitializeLog();
        }
        stateIterator.ResetToStart();
        RestoreState();
    }

    private void BePressedByObjects(Collider other, bool willKeepPress = false)
    {
        if (!TurnManager.turnManager.CLOCK) return;
        if (other.CompareTag("Player") || other.CompareTag("Box"))
        {
            if (other.name == "Phantom")
            {
                tempCollider = other;
                Debug.Log("temp!");
            }
            if (willKeepPress) this.willKeepPress = true;
            PressButton();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        BePressedByObjects(other);
    }
    private void OnTriggerStay(Collider other)
    {
        BePressedByObjects(other, true);
    }
    private void OnTriggerExit(Collider other)
    {
        willKeepPress = false;
        remainingTurns--; //reason: (because of stay(->turncount reset), turncount don't decrease by itself, so when exit decrease.)
    }

    public void AdvanceTurn()
    {
        if (!gameObject.activeSelf) { isMoveComplete = true; return; }
        if (isPressed)
        {
            if (tempCollider != null && !tempCollider.gameObject.GetComponent<PhantomController>().commandIterator.InBoundary())
            {
                willKeepPress = false;
                tempCollider = null;
            }
            else
            {
                if (remainingTurns == resetTurnCount &&
                PlayerController.playerController.curKey == "r" &&
            (!PhantomController.phantomController.isPhantomExisting ||
            (PhantomController.phantomController.isPhantomExisting && (!PhantomController.phantomController.commandIterator.InBoundary() ||
            PhantomController.phantomController.commandIterator.Current == "r"))))
                {
                    willKeepPress = true;
                }
            }
            
            if (remainingTurns <= 0) ResetButton();
            if (!willKeepPress) remainingTurns--;
            else willKeepPress = false;
            isMoveComplete = true;
        }
        else
        {
            StartCoroutine(WaitForOthersMoveCompleteAndAdvance());
        }
    }

    private IEnumerator WaitForOthersMoveCompleteAndAdvance()
    {
        // Wait until Others' isMoveComplete becomes true (including fall)
        yield return new WaitUntil(
            () =>
            isPressed ||
            TurnManager.turnManager.CheckCharactersActionComplete()
        );
        willKeepPress = false;
        isMoveComplete = true;
    }

    private void PressButton()
    {
        remainingTurns = resetTurnCount; //reset count (when enter, stay)
        if (!isPressed)
        {
            targetStates.ForEach(state => state.target.SetActive(!state.isInitiallyActive)); // Toggle state
            targetStates.ForEach(state => PlaySummonSound(state.target));
            transform.GetChild(1).transform.position = plateOnPosition;
            SoundManager.soundManager.PlaySound3D("button_press", this.transform, 0.15f);

            isPressed = true;
        }
    }

    private void ResetButton()
    {
        targetStates.ForEach(state => state.target.SetActive(state.isInitiallyActive)); // Revert to initial state
        targetStates.ForEach(state => PlaySummonSound(state.target));
        transform.GetChild(1).transform.position = plateOffPosition;
        SoundManager.soundManager.PlaySound3D("button_pop", this.transform, 0.13f);

        isPressed = false;
    }

    private void PlaySummonSound(GameObject target)
    {
        if (target.CompareTag("Box")) SoundManager.soundManager.PlaySound3D("box_summon", target.transform, 0.09f);
        else if (target.CompareTag("Laser") || target.name == "StartPoint")
        {
            if (target.activeSelf) SoundManager.soundManager.PlaySound3D("laser_on", target.transform, 0.09f);
            else SoundManager.soundManager.PlaySound3D("laser_off", target.transform, 0.09f);
        }
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