using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBox : MonoBehaviour
{
    public float moveDistance = 2.0f;
    private Rigidbody rb;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
    }

    private void Update()
    {
        // Raycast downward to check if there's a "Ground Floor" or "First Floor" tile beneath the box
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.0f))
        {
            if (hit.collider.CompareTag("GroundFloor") || hit.collider.CompareTag("FirstFloor"))
            {
                // Tile detected, keep Y position constraint to keep the box stable
                rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
                return;
            }
        }
        
        // If no tile is detected or the tile is not GroundFloor/FirstFloor, allow the box to fall
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
}