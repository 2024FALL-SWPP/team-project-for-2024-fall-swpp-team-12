using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject pauseUI;
    public GameObject resetUI; // maybe adding "Are you sure to reset?"

    void Start()
    {
        InputManager.inputManager.OnPauseToggle += TogglePause;
    }

    void TogglePause()
    {
        InputManager.inputManager.isPaused = !InputManager.inputManager.isPaused;

        if (InputManager.inputManager.isPaused)
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;
            if (pauseUI != null) pauseUI.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            if (pauseUI != null) pauseUI.SetActive(false);
        }
    }
}
