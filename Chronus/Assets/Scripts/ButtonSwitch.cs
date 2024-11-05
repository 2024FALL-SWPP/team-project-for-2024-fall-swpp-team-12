using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSwitch : MonoBehaviour
{
    public GameObject platform; 
    public Transform plate; 
    private Vector3 initialPlatePosition;
    private bool isResetting = false;
    private bool isPressing = false;

    private void Start()
    {
        platform.SetActive(false); 
        initialPlatePosition = plate.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(DelayedPressButton());
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !platform.activeSelf && !isPressing)
        {
            StartCoroutine(DelayedPressButton());
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isResetting)
        {
            StartCoroutine(ResetPlatePositionAfterDelay());
        }
    }

    private IEnumerator DelayedPressButton()
    {
        isPressing = true;
        PressButton();
        isPressing = false;
        yield break;
    }

    private void PressButton()
    {
        platform.SetActive(true);
        
        // Move the plate down slightly to simulate pressing
        plate.position = initialPlatePosition - new Vector3(0, 0.1f, 0);

        // Cancel any reset process if the player is still standing on the button
        if (isResetting)
        {
            StopCoroutine(ResetPlatePositionAfterDelay());
            isResetting = false;
        }
    }

    private IEnumerator ResetPlatePositionAfterDelay()
    {
        isResetting = true; // Mark that reset is in progress
        yield return new WaitForSeconds(3f);
        
        plate.position = initialPlatePosition;
        platform.SetActive(false);
        isResetting = false;
    }
}
