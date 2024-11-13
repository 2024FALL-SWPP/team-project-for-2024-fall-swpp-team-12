using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBox : MonoBehaviour
{
    public float moveDistance = 2.0f;
    private Rigidbody rb;
    public float checkDistance = 0.5f;
    private Vector3 lastSavedPosition;
     private bool wasOnGround = false;

    // Time rewind logs
    public List<string> listBoxCommandLog;
    public List<(Vector3, bool)> listBoxStateLog; // (position, isOnGround)

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        // Initialize time rewind logs
        listBoxCommandLog = new List<string>();
        listBoxStateLog = new List<(Vector3, bool)>();

        lastSavedPosition = transform.position;
        wasOnGround = true;
        SaveCurrentState("Initialize"); // Save initial state
    }

    private void Update()
    {
        bool isOnGround = false;
        
        // Raycast downward to check if there's a "Ground Floor" or "First Floor" tile beneath the box
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, checkDistance))
        {
            if (hit.collider.CompareTag("GroundFloor") || hit.collider.CompareTag("FirstFloor"))
            {
                // Tile detected, keep Y position constraint to keep the box stable
                rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
                isOnGround = true;
            }
        }
        else
        {
            // If no tile is detected or the tile is not GroundFloor/FirstFloor, allow the box to fall
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        
        // Save the state if the position has changed or if there is a ground state change
        if (isOnGround && (transform.position != lastSavedPosition || !wasOnGround))
        {
            SaveCurrentState(isOnGround ? "Landed" : "Location Update");
        }
        
        // Update ground status
        wasOnGround = isOnGround;
    }

    private void SaveCurrentState(string command)
    {
        // Save state only if position or stability status changes
        if (transform.position != lastSavedPosition || listBoxCommandLog.Count == 0 || listBoxCommandLog[^1] != command)
        {
            // Log the command and current position
            string logEntry = $"{command} {transform.position}";
            listBoxCommandLog.Add(logEntry);

            bool isOnGround = (rb.constraints & RigidbodyConstraints.FreezePositionY) != 0;
            listBoxStateLog.Add((transform.position, isOnGround));
            
            // Update the last saved position
            lastSavedPosition = transform.position;
        }
    }

    public void RestoreState(int turnIndex)
    {
        if (turnIndex < listBoxStateLog.Count)
        {
            var state = listBoxStateLog[turnIndex];
            transform.position = state.Item1;
            bool isOnGround = state.Item2;

            // Restore Y constraint based on whether the box was on the ground
            rb.constraints = isOnGround
                ? RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY
                : RigidbodyConstraints.FreezeRotation;
        }
    }
}