
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSwitch : MonoBehaviour
{
    public GameObject[] platforms; 
    public Transform plate; 
    private Vector3 initialPlatePosition;
    private bool isPressed = false;
    private int resetTurnCount = 4; 
    private int turnActivated;

    private void Start()
    {
        foreach (GameObject platform in platforms)
        {
            platform.SetActive(false);
        }
        initialPlatePosition = plate.position;
    }

    private void Update()
    {
        if (isPressed && TurnManager.turnManager.turn > turnActivated + resetTurnCount)
        {
            ResetButton();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("Box")) && !isPressed)
        {
            PressButton();
        }
    }

    private void PressButton()
    {
        foreach (GameObject platform in platforms)
        {
            platform.SetActive(true);
        }
        plate.position = initialPlatePosition - new Vector3(0, 0.1f, 0); 
        isPressed = true;

        turnActivated = TurnManager.turnManager.turn;
    }

    private void ResetButton()
    {
        foreach (GameObject platform in platforms)
        {
            platform.SetActive(false);
        }
        plate.position = initialPlatePosition; 
        isPressed = false; 
    }
}