using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public GameObject gameInstructionCanvas;
    public GameObject leverInstructionCanvas;
    public GameObject timeRewindInstructionCanvas;

    public void ShowTutorialForLevel(string levelName)
    {
        gameInstructionCanvas.SetActive(false);
        leverInstructionCanvas.SetActive(false);
        timeRewindInstructionCanvas.SetActive(false);

        switch (levelName)
        {
            case "L-001-1":
                gameInstructionCanvas.SetActive(true);
                break;
            case "L-001-2":
                leverInstructionCanvas.SetActive(true);
                break;
            case "L-001-3":
                timeRewindInstructionCanvas.SetActive(true);
                break;
            default:
                break;
        }
    }
    
    private void Update()
    {
        // Check for player input and hide the canvas if the tutorial is active
        if (DetectPlayerInput())
        {
            HideActiveTutorialCanvas();
        }
    }

    private bool DetectPlayerInput()
    {
        return Input.anyKeyDown;
    }

    private void HideActiveTutorialCanvas()
    {    
        gameInstructionCanvas.SetActive(false);
        leverInstructionCanvas.SetActive(false);
        timeRewindInstructionCanvas.SetActive(false);
    }
}