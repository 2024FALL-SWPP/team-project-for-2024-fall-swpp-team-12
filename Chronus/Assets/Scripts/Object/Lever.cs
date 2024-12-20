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

    private void Start()
    {
        if (stateIterator == null) InitializeLog();
    }

    public void InitializeLog()
    {
        // Reset state
        transform.GetChild(1).transform.localRotation = backwardRotation;
        canToggleDirection = transform.localRotation * Vector3.forward;
        doPushLever = false;
        isActivated = false;

        targetStates.ForEach(state => state.target.SetActive(state.isInitiallyActive));
        var initialState = new List<(Quaternion, bool, Vector3)>
        {
            (transform.GetChild(1).transform.rotation, isActivated, canToggleDirection)
        };
        stateIterator = new TurnLogIterator<(Quaternion, bool, Vector3)>(initialState);
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
                isMoveComplete = true;
            }
        }
    }

    public void ToggleLever() // Log lever toggle state
    {
        isActivated = !isActivated;
        transform.GetChild(1).transform.localRotation = isActivated ? forwardRotation : backwardRotation;
        canToggleDirection = -canToggleDirection;

        targetStates.ForEach(state => state.target.SetActive(state.isInitiallyActive ^ isActivated));
        targetStates.ForEach(state => PlaySummonSound(state.target));

        SoundManager.soundManager.PlaySound3D("lever_push", this.transform, 0.18f);
        isMoveComplete = true;
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
        stateIterator.RemoveLastK(k);
    }
}
