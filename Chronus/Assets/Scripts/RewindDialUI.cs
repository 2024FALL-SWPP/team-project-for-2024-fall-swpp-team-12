using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewindDialUI : MonoBehaviour
{
    [SerializeField] private List<Image> uiSlots;
    [SerializeField] private Sprite upArrow; 
    [SerializeField] private Sprite downArrow; 
    [SerializeField] private Sprite leftArrow; 
    [SerializeField] private Sprite rightArrow; 
    [SerializeField] private Sprite idleCircle; 
    [SerializeField] private Sprite emptySlot;
    [SerializeField] private RectTransform slidingSquare;
    private int currentLogIndex = 0; 
    private int visibleStartIndex = 0; 
    private const int maxVisibleSlots = 5; 
    private int slidingSquareIndex = 5;
    private bool isFixedAtSlot3 = false;

    private List<string> commandLog;

    private void Start()
    {
        slidingSquare.position = uiSlots[slidingSquareIndex].transform.position; // Start at Slot 6
    }

    public void Initialize(List<string> log)
    {
        commandLog = log;
        currentLogIndex = Mathf.Max(1, commandLog.Count - 1); 
        visibleStartIndex = Mathf.Max(1, commandLog.Count - maxVisibleSlots);
        UpdateRewindUI();
    }

    public void UpdateRewindUI()
    {
        int logCount = commandLog?.Count ?? 0;

        // Fill Slots 1–5 with commands or clock sprites if no commands exist
        for (int i = 0; i < maxVisibleSlots; i++)
        {
            int logIndex = visibleStartIndex + i;

            if (logIndex < logCount && logIndex > 0)
            {
                uiSlots[i].sprite = GetCommandSprite(commandLog[logIndex]);
            }
            else
            {
                uiSlots[i].sprite = emptySlot;
            }
        }

        // Slot 6 always remain empty
        uiSlots[5].sprite = emptySlot;

        // Update sliding square position
        slidingSquare.position = uiSlots[slidingSquareIndex].transform.position;
    }
    
    private Sprite GetCommandSprite(string command)
    {
        switch (command)
        {
            case "w": return upArrow;
            case "s": return downArrow;
            case "a": return leftArrow;
            case "d": return rightArrow;
            case "r": return idleCircle;
            default: return emptySlot;
        }
    }

    public void MoveSlidingSquare(int direction)
    {
        if (commandLog == null || commandLog.Count == 0) return;

        int logCount = commandLog.Count;

        if (direction < 0) // Moving left
        {
            if (isFixedAtSlot3)
            {
                visibleStartIndex = Mathf.Clamp(visibleStartIndex - 1, 0, logCount - maxVisibleSlots);
            }
            else
            {
                if (slidingSquareIndex > 0)
                {
                    slidingSquareIndex--;
                    currentLogIndex = Mathf.Clamp(currentLogIndex - 1, 0, logCount - 1);
                }
                else if (logCount > maxVisibleSlots && slidingSquareIndex == 0)
                {
                    isFixedAtSlot3 = true;
                    slidingSquareIndex = 2; // Fix at Slot 3
                    visibleStartIndex = Mathf.Clamp(currentLogIndex - 2, 0, logCount - maxVisibleSlots);
                }
            }
        }
        else if (direction > 0) // Moving right
        {
            if (isFixedAtSlot3)
            {
                visibleStartIndex = Mathf.Clamp(visibleStartIndex + 1, 0, logCount - maxVisibleSlots);
            }
            else
            {
                // For the first 5 steps, move the sliding square
                if (slidingSquareIndex < maxVisibleSlots - 1)
                {
                    slidingSquareIndex++;
                    currentLogIndex = Mathf.Clamp(currentLogIndex + 1, 0, logCount - 1);
                }
            }
        }

        UpdateRewindUI();
    }
    
    public void EnterRewindMode()
    {
        if (commandLog == null || commandLog.Count == 1)
        {
            // Fill all slots with empty sprite if no commands exist
            for (int i = 0; i < uiSlots.Count; i++)
            {
                uiSlots[i].sprite = emptySlot;
            }
            slidingSquareIndex = 5; // Reset sliding square to Slot 6
            slidingSquare.position = uiSlots[slidingSquareIndex].transform.position;
            gameObject.SetActive(true);
            return;
        }

        int logCount = commandLog.Count;

        // Initialize indices and UI for rewind mode
        currentLogIndex = Mathf.Max(1, logCount - 1);
        visibleStartIndex = Mathf.Max(1, logCount - maxVisibleSlots);
        slidingSquareIndex = Mathf.Min(logCount-1, 5); // Start at Slot 5 or less

        UpdateRewindUI();
        gameObject.SetActive(true);
    }
    
    public void ExitRewindMode()
    {
        gameObject.SetActive(false);
    }
}
