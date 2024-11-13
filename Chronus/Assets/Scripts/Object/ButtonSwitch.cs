
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSwitch : MonoBehaviour
{
    public GameObject[] platforms;
    private bool[] boolPalette;
    private int idx;
    private Vector3 plateOffPosition;
    private Vector3 plateOnPosition;
    public bool isPressed = false;

    public int resetTurnCount = 4;
    private int turnActivated = 0;
    private int lastLoggedCountdown = -1;  // Tracks the last countdown logged

    // Time rewind logs
    public List<string> listButtonCommandLog;
    public List<(Vector3, bool, int)> listButtonStateLog; // (position, isPressed, turnActivated)

    private void Start()
    {
        plateOffPosition = this.transform.GetChild(1).transform.position;
        plateOnPosition = plateOffPosition + new Vector3(0,-0.1f,0);

        boolPalette = new bool[1];
        boolPalette[0] = true;

        idx = 0;
        foreach (GameObject platform in platforms)
        {
            platform.SetActive(boolPalette[idx] ^ true);
            idx++;
        }

        listButtonCommandLog = new List<string>();
        listButtonStateLog = new List<(Vector3, bool, int)>();
    }

    /*
    private void Update()
    {
        if (TurnManager.turnManager.firstCollisionCheck) //update when firstCollisionCheck
        {
            if (isPressed  &&  TurnManager.turnManager.turn >= turnActivated + resetTurnCount - 1)
            {
                ResetButton();
            }
            else
            {
                TurnManager.turnManager.dicTurnCheck["Button"] = true;
            }
        }
    }
    */

    private void Update()
    {
        if (TurnManager.turnManager.firstCollisionCheck) //update when firstCollisionCheck
        {
            if (isPressed)
            {
                int remainingTurns = (turnActivated + resetTurnCount) - TurnManager.turnManager.turn - 1;

                // Log the countdown if it's decreasing and has not been logged yet
                if (remainingTurns > 0 && remainingTurns != lastLoggedCountdown)
                {
                    listButtonCommandLog.Add($"{remainingTurns}");
                    lastLoggedCountdown = remainingTurns;
                }

                // If countdown reaches 0, reset the button
                if (TurnManager.turnManager.turn >= turnActivated + resetTurnCount - 1)
                {
                    ResetButton();
                }
                else
                {
                    TurnManager.turnManager.dicTurnCheck["Button"] = true;
                }
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed && (other.CompareTag("Player") || other.CompareTag("Box"))) //update when firstCollisionCheck
        {
            PressButton();
        }
    }

    private void PressButton()
    {
        idx = 0;
        foreach (GameObject platform in platforms)
        {
            platform.SetActive(boolPalette[idx] ^ false);
            idx++;
        }
        this.transform.GetChild(1).transform.position = plateOnPosition;

        turnActivated = TurnManager.turnManager.turn + 1; //get value before turn update, so +1 (indicates next turn)
        isPressed = true;
        TurnManager.turnManager.dicTurnCheck["Button"] = true;

        // Log button press state
        listButtonCommandLog.Add("Activated");
        lastLoggedCountdown = resetTurnCount;
    }

    private void ResetButton()
    {
        idx = 0;
        foreach (GameObject platform in platforms)
        {
            platform.SetActive(boolPalette[idx] ^ true);
            idx++;
        }
        this.transform.GetChild(1).transform.position = plateOffPosition;

        turnActivated = 0;
        isPressed = false;
        TurnManager.turnManager.dicTurnCheck["Button"] = true;

        // Log button reset state
        listButtonCommandLog.Add("Deactivated");
        lastLoggedCountdown = -1; // Reset countdown tracking
    }

    private void SaveCurrentState(string command)
    {
        // Log the command and current state
        listButtonCommandLog.Add(command);
        listButtonStateLog.Add((transform.position, isPressed, turnActivated));
    }

    public void RestoreState(int turnIndex)
    {
        if (turnIndex < listButtonStateLog.Count)
        {
            var state = listButtonStateLog[turnIndex];
            transform.position = state.Item1;
            isPressed = state.Item2;
            turnActivated = state.Item3;

            // Update button appearance based on the restored state
            this.transform.GetChild(1).transform.position = isPressed ? plateOnPosition : plateOffPosition;

            idx = 0;
            foreach (GameObject platform in platforms)
            {
                platform.SetActive(boolPalette[idx] ^ !isPressed);
                idx++;
            }
        }
    }
}