using System;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager inputManager;
    public bool isPaused = false;
    public bool isPlayingScript = false;
    public Action<string> OnMovementControl; // to PlayerController.HandleMovementInput()
    public Action<string> OnTimeRewindControl; // to PlayerController.HandleTimeRewindInput()
    public Action OnTimeRewindModeToggle; // to PlayerController.ToggleTimeRewind()
    public Action OnPauseToggle, OnUndo, OnReset; // to UIManager / TurnManager / LevelManager
    private Dictionary<KeyCode, Action> inputActions;

    private void Awake()
    {
        if (inputManager == null) { inputManager = this; }
    }

    void Start()
    {
        inputActions = new Dictionary<KeyCode, Action>
        {
            { KeyCode.W, () => OnMovementControl?.Invoke("w") },
            { KeyCode.S, () => OnMovementControl?.Invoke("s") },
            { KeyCode.A, () => OnMovementControl?.Invoke("a") },
            { KeyCode.D, () => OnMovementControl?.Invoke("d") },
            { KeyCode.R, () => OnMovementControl?.Invoke("r") },
            { KeyCode.Q, () => OnTimeRewindControl?.Invoke("q") },
            { KeyCode.E, () => OnTimeRewindControl?.Invoke("e") },
            { KeyCode.Space, () => OnTimeRewindModeToggle?.Invoke() },
            { KeyCode.Backspace, () => OnUndo?.Invoke() }
        };
    }

    private void InvokeAction()
    {
        foreach (var keyAction in inputActions)
        {
            if (Input.GetKeyDown(keyAction.Key))
            {
                if (ScenarioManager.scenarioManager.isReadingMonologue)
                {
                    if (ScenarioManager.scenarioManager.isLockedToRead) ScenarioManager.scenarioManager.isLockedToRead = false; //skip
                    ScenarioManager.scenarioManager.isReadingMonologue = false;
                }
                keyAction.Value.Invoke();
            }
        }
    }

    void Update()
    {
        if (!isPaused && (InputManagerTesting.instance == null || !InputManagerTesting.instance.isActiveAndEnabled) && !isPlayingScript)
        {
            InvokeAction();
        }

        if (Input.GetKeyDown(KeyCode.Return) && !TurnManager.turnManager.CLOCK && !PlayerController.playerController.isBlinking && !isPlayingScript)
        {
            SoundManager.soundManager.PlaySound2D("ui_reset", 0.2f);
            OnReset?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SoundManager.soundManager.PlaySound2D("ui_pause", 0.5f);
            OnPauseToggle?.Invoke();
        }
    }
}