using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager inputManager; // singleton
    public Action<string> OnCommand; // to PlayerController.HandleMovementInput()
    public Action OnTimeRewindModeToggle; // to PlayerController.ToggleTimeRewind()
    public Action<string> OnTimeRewindControl; // to PlayerController.HandleTimeRewindInput()

    private void Awake()
    {
        if (inputManager == null) { inputManager = this; }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            OnCommand?.Invoke("w");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            OnCommand?.Invoke("s");
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            OnCommand?.Invoke("a");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            OnCommand?.Invoke("d");
        }
        else if (Input.GetKeyDown(KeyCode.R)) 
        {
            OnCommand?.Invoke("r");
        }
        else if (Input.GetKeyDown(KeyCode.Space)) 
        {
            OnTimeRewindModeToggle?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Q)) 
        {
            OnTimeRewindControl?.Invoke("q");
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            OnTimeRewindControl?.Invoke("e");
        }
    }
}