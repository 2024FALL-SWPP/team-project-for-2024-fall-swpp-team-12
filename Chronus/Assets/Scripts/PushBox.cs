using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBox : MonoBehaviour
{
    public float moveDistance = 2.0f;
    private Rigidbody rb;
    public float checkDistance = 1.5f;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        TickManager.OnTick += OnTickEvent;
    }

    private void OnDestroy()
    {
        TickManager.OnTick -= OnTickEvent;
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
            // Keep Y constraint to prevent the box from falling
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            TurnManager.turnManager.dicTurnCheck["Box"] = true;
        }
        else
        {
            // Release Y constraint if no ground detected to allow falling
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.useGravity = true;
        }
    }

    private void OnTickEvent()
    {
        CheckIfFloating();
    }
}
