using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverSwitch : MonoBehaviour
{
    public GameObject platform; 
    public Transform lever;
    private bool isActivated = false;
    private Quaternion initialLeverRotation;

    private void Start()
    {
        platform.SetActive(false); 
        initialLeverRotation = lever.rotation; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            ActivateLever();
        }
    }

    private void ActivateLever()
    {
        isActivated = true;
        platform.SetActive(true); 
        
        // Rotate the lever 45 degrees forward 
        lever.rotation = Quaternion.Euler(initialLeverRotation.eulerAngles + new Vector3(-45, 0, 0));

        lever.GetComponent<Collider>().enabled = false;
    }
}