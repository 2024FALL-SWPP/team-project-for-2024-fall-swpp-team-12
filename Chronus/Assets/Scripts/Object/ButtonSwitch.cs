
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSwitch : MonoBehaviour
{
    public GameObject[] platforms; 
    private Vector3 plateOffPosition;
    private Vector3 plateOnPosition;
    public bool isPressed = false;

    public int resetTurnCount = 4;
    private int turnActivated = 0;

    private void Start()
    {
        plateOffPosition = this.transform.GetChild(1).transform.position;
        plateOnPosition = plateOffPosition + new Vector3(0,-0.1f,0);

        foreach (GameObject platform in platforms)
        {
            platform.SetActive(false);
        }
    }

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
    
    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed && (other.CompareTag("Player") || other.CompareTag("Box"))) //update when firstCollisionCheck
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
        this.transform.GetChild(1).transform.position = plateOnPosition;

        turnActivated = TurnManager.turnManager.turn + 1; //get value before turn update, so +1 (indicates next turn)
        isPressed = true;
        TurnManager.turnManager.dicTurnCheck["Button"] = true;
    }

    private void ResetButton()
    {
        foreach (GameObject platform in platforms)
        {
            platform.SetActive(false);
        }
        this.transform.GetChild(1).transform.position = plateOffPosition;

        turnActivated = 0;
        isPressed = false;
        TurnManager.turnManager.dicTurnCheck["Button"] = true;
    }
}