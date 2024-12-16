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
    public Transform targetObject;
    public float radius = 4.0f;

    private GameObject currentCanvas;

    public void ShowTutorialForLevel(string levelName)
    {
        HideActiveTutorialCanvas();

        switch (levelName)
        {
            case "L-001-1":
                currentCanvas = gameInstructionCanvas;
                break;
            case "L-001-3":
                currentCanvas = timeRewindInstructionCanvas;
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
        // Handle canvases that reappear near specific tiles
        if (currentCanvas == gameInstructionCanvas || currentCanvas == timeRewindInstructionCanvas ||
            currentCanvas == leverInstructionCanvas || currentCanvas == boxInstructionCanvas)
        {
            HandleTileProximityForCanvas();
        }
        if (currentCanvas.activeSelf && (ScenarioManager.scenarioManager.isReadingMonologue || PlayerController.playerController.isTimeRewinding)) currentCanvas.SetActive(false);
    }

    private void HandleTileProximityForCanvas()
    {
        if (currentCanvas != null)
        {
            GameObject targetTile = FindTargetTile();

            if (targetTile != null && IsPlayerWithinRadius(targetTile.transform))
            {
                if (!ScenarioManager.scenarioManager.isReadingMonologue && !PlayerController.playerController.isTimeRewinding) currentCanvas.SetActive(true);
            }
            else
            {
                currentCanvas.SetActive(false);
            }
        }
    }

    private GameObject FindTargetTile()
    {
        if (currentCanvas == gameInstructionCanvas || currentCanvas == timeRewindInstructionCanvas)
        {
            return GameObject.FindWithTag("TutorialTile");
        }
        else if (currentCanvas == leverInstructionCanvas)
        {
            return GameObject.FindWithTag("Lever");
        }
        else if (currentCanvas == boxInstructionCanvas)
        {
            return GameObject.FindWithTag("Box");
        }
        return null;
    }

    private bool IsPlayerWithinRadius(Transform tileCenter)
    {
        if (tileCenter == null) return false;

        // Calculate the distance between the player and the target tile center
        float distance = Vector3.Distance(player.position, tileCenter.position);
        return distance <= radius;
    }

    public void SetTargetObject(Transform target)
    {
        targetObject = target;
    }

    public void ClearTargetObject()
    {
        targetObject = null;
    }

    private void HideActiveTutorialCanvas()
    {
        gameInstructionCanvas.SetActive(false);
        leverInstructionCanvas.SetActive(false);
        timeRewindInstructionCanvas.SetActive(false);
        boxInstructionCanvas.SetActive(false);
    }
}