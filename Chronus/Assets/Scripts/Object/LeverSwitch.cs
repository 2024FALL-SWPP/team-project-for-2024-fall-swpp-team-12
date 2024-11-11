using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverSwitch : MonoBehaviour
{
    public GameObject[] platforms;
    private bool[] boolPalette;
    private int idx;

    private bool isActivated = false;
    private Quaternion forwardRotation; 
    private Quaternion backwardRotation;
    public Vector3 canToggleDirection;
    //private bool playerAdjacent = false; 

    //private GameObject player;

    private void Start()
    {
        forwardRotation = Quaternion.Euler(130, 0, 0);
        backwardRotation = Quaternion.Euler(50, 0, 0);

        // Initial lever rotation
        this.transform.GetChild(1).transform.rotation = backwardRotation;
        canToggleDirection = this.transform.forward;

        boolPalette = new bool[1];
        boolPalette[0] = true;

        idx = 0;
        foreach (GameObject platform in platforms)
        {
            platform.SetActive(boolPalette[idx] ^ !isActivated);
            idx++;
        }
    }

    /*
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerAdjacent = true;
            player = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerAdjacent = false;
            player = null;
        }
    }
    */

    private void Update()
    {
        /*
        if (playerAdjacent && PlayerIsTryingToMoveTowardsLever())
        {
            ToggleLever();
        }
        */
    }

    /*
    private bool PlayerIsTryingToMoveTowardsLever()
    {
        if (player == null) return false;

        // Calculate the direction from the player to the lever
        Vector3 directionToLever = (lever.position - player.transform.position).normalized;

        // Get the player's movement input direction
        Vector3 playerInputDirection = Vector3.zero;

        // Capture player input continuously
        if (Input.GetKey(KeyCode.W)) playerInputDirection = Vector3.forward;
        else if (Input.GetKey(KeyCode.S)) playerInputDirection = Vector3.back;
        else if (Input.GetKey(KeyCode.A)) playerInputDirection = Vector3.left;
        else if (Input.GetKey(KeyCode.D)) playerInputDirection = Vector3.right;

        // Check if the player is pressing in the direction of the lever
        return Vector3.Dot(directionToLever, playerInputDirection) > 0.5f;
    }
    */

    public void ToggleLever()
    {
        isActivated = !isActivated;
        this.transform.GetChild(1).transform.rotation = isActivated ? forwardRotation : backwardRotation;
        canToggleDirection = -canToggleDirection;
        idx = 0;
        foreach (GameObject platform in platforms)
        {
            platform.SetActive(boolPalette[idx] ^ !isActivated);
            idx++;
        }
    }
}
