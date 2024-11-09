using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager inputManager; // singleton
    public Action<string> OnInput; // to PlayerController.HandleInput()
    public Action<string> OnTimeRewindInput; // to PlayerController.HandleTimeRewindInput()
    public Action OnSpace; // to PlayerController.ToggleTimeRewind()

    private void Awake()
    {
        if (inputManager == null) { inputManager = this; }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            OnInput?.Invoke("w");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            OnInput?.Invoke("s");
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            OnInput?.Invoke("a");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            OnInput?.Invoke("d");
        }
        else if (Input.GetKeyDown(KeyCode.R)) 
        {
            OnInput?.Invoke("r");
        }
        else if (Input.GetKeyDown(KeyCode.Space)) 
        {
            OnSpace?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Q)) 
        {
            OnTimeRewindInput?.Invoke("q");
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            OnTimeRewindInput?.Invoke("e");
        }
    }
}