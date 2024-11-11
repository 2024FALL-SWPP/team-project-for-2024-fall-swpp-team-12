using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBox : MonoBehaviour
{
    public float moveDistance = 2.0f;
    private Rigidbody rb;
    public float checkDistance = 0.5f;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

    }
    /*
    private void Update()
    {
        CheckIfFloating();
    }
    */

    public void CheckIfFloating()
    {
        bool isGroundDetected = Physics.Raycast(transform.position, Vector3.down, out RaycastHit groundHit, checkDistance);

        if (isGroundDetected && (groundHit.collider.CompareTag("GroundFloor") || groundHit.collider.CompareTag("FirstFloor")))
        {
            if (hit.collider.CompareTag("GroundFloor") || hit.collider.CompareTag("FirstFloor"))
            {
                // Tile detected, keep Y position constraint to keep the box stable
                rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            }
        }
        else
        {
            // If no tile is detected or the tile is not GroundFloor/FirstFloor, allow the box to fall
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

}
