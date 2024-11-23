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
    public bool isFallComplete = false;
    private bool isBeingPushed = false;
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
        if (TurnManager.turnManager.fallCLOCK && !isFallComplete)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, checkDistance))
            {
                if (hit.collider.CompareTag("GroundFloor") || hit.collider.CompareTag("FirstFloor") || hit.collider.CompareTag("Box"))
                {
                    // Tile detected, keep Y position constraint to keep the box stable
                    rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
                    transform.position = new Vector3(transform.position.x, Mathf.Ceil(transform.position.y) - 0.5f, transform.position.z);
                    isFallComplete = true;
                }
            }
            else
            {
                // If no tile is detected, allow the box to fall
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }
        
        // There should be a code indicating:
        // if it has touched the ground, then isMoveComplete = true. 
    }
    public void AdvanceTurn()
    {   
        if (!isBeingPushed) isMoveComplete = true;
        isBeingPushed = false;
    }
    public void AdvanceFall()
    {
        //check if void or not
    }
    public bool TryMove(Vector3 direction)
    {
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, moveDistance))
        {
            if (hit.collider.CompareTag("Obstacle") || // need "Obstacle" tag for all walls.
                hit.collider.CompareTag("Lever") ||
                hit.collider.CompareTag("MovingObstacle") ||
                hit.collider.CompareTag("Wall") ||
                hit.collider.CompareTag("Player"))
            {
                isBeingPushed = false;
                return false;
            }
        }
        
        if (Physics.Raycast(transform.position, Vector3.up, out RaycastHit hitUp, moveDistance/2)) //look up.
        {
            switch (hitUp.collider.tag)
            {
                case "Box": //box stack yeah
                    {
                        Box box = hitUp.collider.gameObject.GetComponent<Box>();
                        box.TryMove(direction); //kind of recursion yeah
                        break;
                    }
                case "Player": //player or phantom
                    {
                        if (hitUp.collider.name == "Player")
                        {
                            if (PlayerController.playerController.curKey == "r") //player would ride the box when it has nothing to do.
                            {
                                if (!Physics.Raycast(PlayerController.playerController.transform.position, direction, out RaycastHit hitWall, moveDistance)) // and no obstacles forward.
                                {
                                    //player will have 'idle' state and do operation update in that state.
                                    PlayerController.playerController.isRidingBox = true;
                                    PlayerController.playerController.targetTranslation = PlayerController.playerController.playerCurPos + direction * 2.0f;
                                }
                            }
                        }
                        else if (hitUp.collider.name == "Phantom")
                        {
                            if (PhantomController.phantomController.commandIterator.GetNext() == "r") //phantom would ride the box when it has nothing to do.
                            {
                                if (!Physics.Raycast(PhantomController.phantomController.transform.position, direction, out RaycastHit hitWall1, moveDistance)) // and no obstacles forward.
                                {
                                    //phantom will have 'idle' state and do operation update in that state.
                                    PhantomController.phantomController.isRidingBox = true;
                                    PhantomController.phantomController.targetTranslation = PhantomController.phantomController.playerCurPos + direction * 2.0f;
                                }
                            }
                        }

                        break;
                    }
            }
        }
        
        isBeingPushed = true;
        StartCoroutine(SmoothMove(direction * moveDistance));
        return true;
    }

    private IEnumerator SmoothMove(Vector3 direction)
    {
        float moveSpeed = PlayerController.playerController.moveSpeedHor;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + direction;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f && Vector3.Distance(transform.position, startPosition) < 2.0f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        isBeingPushed = false;
        isMoveComplete = true;

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