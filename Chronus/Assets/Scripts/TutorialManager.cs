using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public GameObject gameInstructionCanvas;
    public GameObject leverInstructionCanvas;
    public GameObject timeRewindInstructionCanvas;
    public GameObject boxInstructionCanvas;

    public Transform player;
    public Transform lever;
    public Transform box;
    public float radius = 4.0f;
    public int maxInputCount = 3;
    
    private GameObject currentCanvas;
    private int inputCount = 0;

    public void ShowTutorialForLevel(string levelName)
    {
        HideActiveTutorialCanvas();
        inputCount = 0;

        switch (levelName)
        {
            case "L-001-1":
                currentCanvas = gameInstructionCanvas;
                currentCanvas.SetActive(true);
                break;
            case "L-001-3":
                currentCanvas = timeRewindInstructionCanvas;
                currentCanvas.SetActive(true);
                break;
            case "L-001-2":
                currentCanvas = leverInstructionCanvas;
                break;
            case "L-001-5":
                currentCanvas = boxInstructionCanvas;
                break;
            default:
                currentCanvas = null;
                break;
        }
    }
    
    private void Update()
    {
        // Handle canvases that disappear after 3 inputs
        if (currentCanvas == gameInstructionCanvas || currentCanvas == timeRewindInstructionCanvas)
        {
            HandleInputForCanvas();
        }

        // Handle canvases that reappear near specific tiles
        if (currentCanvas == leverInstructionCanvas && lever != null)
        {
            HandleTileProximityForCanvas();
        }

        if (currentCanvas == boxInstructionCanvas && box != null)
        {
            HandleTileProximityForCanvas();
        }
    }

    private void HandleInputForCanvas()
    {
        if (currentCanvas != null && currentCanvas.activeSelf && Input.anyKeyDown)
        {
            inputCount++;
            if (inputCount >= maxInputCount)
            {
                currentCanvas.SetActive(false);
            }
        }
    }
    
    private void HandleTileProximityForCanvas()
    {
        if (currentCanvas != null)
        {
            Transform targetTile = (currentCanvas == leverInstructionCanvas) ? lever : box;

            // Check if the player is within the 3x3 area
            if (IsPlayerWithinRadius(targetTile))
            {   
                currentCanvas.SetActive(true);
            }
            else
            {
                currentCanvas.SetActive(false);
            }
        }
    }

    private bool IsPlayerWithinRadius(Transform tileCenter)
    {
        if (tileCenter == null) return false;

        // Calculate the distance between the player and the target tile center
        float distance = Vector3.Distance(player.position, tileCenter.position);
        return distance <= radius;
    }

    public void SetLever(Transform lever)
    {
        this.lever = lever;
    }

    public void SetBox(Transform box)
    {
        this.box = box;
    }

    public void ClearLever()
    {
        lever = null;
    }

    public void ClearBox()
    {
        box = null;
    }

    
    private void HideActiveTutorialCanvas()
    {
        gameInstructionCanvas.SetActive(false);
        leverInstructionCanvas.SetActive(false);
        timeRewindInstructionCanvas.SetActive(false);
        boxInstructionCanvas.SetActive(false);
    }
}