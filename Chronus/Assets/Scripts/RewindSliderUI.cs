using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewindSliderUI : MonoBehaviour
{
    [SerializeField] private List<Image> slots;
    [SerializeField] private Sprite upArrow;
    [SerializeField] private Sprite downArrow;
    [SerializeField] private Sprite leftArrow;
    [SerializeField] private Sprite rightArrow;
    [SerializeField] private Sprite idleCircle;
    [SerializeField] private Sprite rewindPoint;
    [SerializeField] private RectTransform slidingSquare;

    private List<string> commandList;
    private int commandIndex = 0;
    private int slidingSquareIndex = 4;
    private const int maxVisibleSlots = 5;

    public void EnterRewindMode(List<string> commands)
    {
        gameObject.SetActive(true);
        slidingSquare.gameObject.SetActive(true);

        commandList = commands;
        commandList.RemoveAt(0); // remove null 
        commandList.Insert(0, "t");

        commandIndex = Mathf.Max(0, commands.Count - 1);
        slidingSquareIndex = maxVisibleSlots - 1;
        UpdateUI();
        UpdateSlidingSquarePosition();
    }

    public void LeaveRewindMode()
    {
        gameObject.SetActive(false);
        slidingSquare.gameObject.SetActive(false);
    }

    private void UpdateUI()
    {
        for (int i = 0; i < maxVisibleSlots; i++)
        {
            int logIndex = commandIndex - (slidingSquareIndex - i);

            if (logIndex >= 0 && logIndex < commandList.Count)
            {
                string command = commandList[logIndex];
                slots[i].sprite = GetCommandSprite(command);
            }
            else
            {
                slots[i].sprite = null;
            }
        }
    }

    private void UpdateSlidingSquarePosition()
    {
        if (slidingSquareIndex >= 0 && slidingSquareIndex < slots.Count)
        {
            RectTransform targetSlot = slots[slidingSquareIndex].rectTransform;
            slidingSquare.position = targetSlot.position;
        }
    }

    public void MoveSlider(int direction)
    {
        int newCommandIndex = commandIndex + direction;
        if (newCommandIndex < 0 || newCommandIndex >= commandList.Count) return;
        commandIndex = newCommandIndex;

        if (commandList.Count < maxVisibleSlots || commandIndex >= commandList.Count - (maxVisibleSlots / 2))
        {
            // if length of the commandList is less than 5: always start from the right
            // OR, rightmost (index = length-2, length-1) -> slidingSquare moves
            slidingSquareIndex = maxVisibleSlots - (commandList.Count - commandIndex);
        }
        else if (commandIndex <= (maxVisibleSlots / 2) - 1)
        {
            // leftmost (index = 0, 1) -> slidingSquare moves
            slidingSquareIndex = commandIndex;
        }
        else
        {
            // else, stuck in the middle
            slidingSquareIndex = maxVisibleSlots / 2;
        }

        UpdateUI();
        UpdateSlidingSquarePosition();
    }

    private Sprite GetCommandSprite(string command)
    {
        return command switch
        {
            "w" => upArrow,
            "s" => downArrow,
            "a" => leftArrow,
            "d" => rightArrow,
            "r" => idleCircle,
            "t" => rewindPoint,
            _ => null,
        };
    }
}
