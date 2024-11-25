using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    public float moveDistance = 2.0f;
    public float checkDistance = 0.5f;
    private Rigidbody rb;
    
    public TurnLogIterator<Vector3> positionIterator;
    public bool isMoveComplete = false;
    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        InitializeLog();
    }

    public void InitializeLog()
    {
        var initialPositionLog = new List<Vector3> { transform.position };
        positionIterator = new TurnLogIterator<Vector3>(initialPositionLog);
    }

    private void Update()
    {
        // Demo falling code. need fix.
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, checkDistance))
        {
            if (hit.collider.CompareTag("GroundFloor") || hit.collider.CompareTag("FirstFloor"))
            {
                // Tile detected, keep Y position constraint to keep the box stable
                rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            }
        }
        else
        {
            // If no tile is detected, allow the box to fall
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // There should be a code indicating:
        // if it has touched the ground, then isMoveComplete = true. 
    }
    public void AdvanceTurn()
    {
        // I guess it's not okay to just simply put this here, but it's temporary.
        // There MUST be a problem because of this.
        isMoveComplete = true;
    }
    public bool TryMove(Vector3 direction)
    {
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, moveDistance))
        {
            if (hit.collider.CompareTag("Obstacle") || // need "Obstacle" tag for all walls.
                hit.collider.CompareTag("Lever") ||
                hit.collider.CompareTag("MovingObstacle")) 
            {
                return false;
            }
        }

        StartCoroutine(SmoothMove(direction * moveDistance));
        return true;
    }

    private IEnumerator SmoothMove(Vector3 direction)
    {
        float moveSpeed = 3.0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + direction;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        //isMoveComplete = true;

        // Also, need to implement this: but has to be changed

        /*
        if (Physics.Raycast(box.transform.position, direction, out RaycastHit obstacleHit, pushDistance, layerMask)) //cannot push box
                {
                    Debug.Log("cannotpush");
                    //StartCoroutine(JumpOntoBox(box));
                    canRideBox = false;
                    canPushBox = false;
                }
                else //can push box
                {
                    Rigidbody rb = box.GetComponent<Rigidbody>();
                    //smoothly move
                    if (Physics.Raycast(box.transform.position, transform.up, out RaycastHit playerHit, pushDistance, layerMask))
                    {
                        if (playerHit.collider.CompareTag("Player"))
                        {
                            canRideBox = true;
                            playerHit.collider.gameObject.GetComponent<Transform>().transform.position += direction * pushDistance;
                            PlayerController.playerController.playerCurPos = playerHit.collider.gameObject.GetComponent<Transform>().transform.position;
                        }
                        else if (playerHit.collider.CompareTag("Phantom")) 
                        {
                            canRideBox = false;
                            // kill phantom.
                        }
                        else
                        {
                            canRideBox = false;
                        }
                    }
                    else
                    {
                        canRideBox = false;
                    }
                    rb.MovePosition(box.transform.position + direction * pushDistance); //statemachine replace is neeeeded
                    canPushBox = true;
        */
    }
    public void SaveCurrentPos()
    {
        positionIterator.Add(transform.position);
    }
    public void RemoveLog(int k)
    {
        positionIterator.RemoveLastK(k);
    }
    public void RestoreState() // updating for time rewind
    {
        transform.position = positionIterator.Current;
    }
}