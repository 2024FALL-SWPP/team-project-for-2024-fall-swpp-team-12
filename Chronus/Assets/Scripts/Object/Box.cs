using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    public float moveDistance = 2.0f;
    private float checkDistance;
    private Rigidbody rb;
    
    public TurnLogIterator<(Vector3, bool)> positionIterator;
    public bool isMoveComplete = false;
    public bool isFallComplete = true;

    private bool isBeingPushed = false;
    public bool willDropDeath = false;
    private bool isWaitingToCheckFall = false;

    private float rayJumpInterval = 1.0f;
    private float maxFallHeight = 10.0f;
    private int layerMask = (1 << 0) | (1 << 3) | (1 << 6); //detect default(0), player(3), and lever(6) layer.
    private void Start()
    {
        checkDistance = transform.localScale.z / 100;
        rb = gameObject.GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        InitializeLog();
    }

    public void InitializeLog()
    {
        var initialPositionLog = new List<(Vector3, bool)> { (transform.position, true) };
        positionIterator = new TurnLogIterator<(Vector3, bool)>(initialPositionLog);
    }

    public void ResetToStart()
    {
        positionIterator.ResetToStart();
        RestoreState();
    }

    private void Update()
    {
        if (isWaitingToCheckFall)
        {
            if (TurnManager.turnManager.CheckMovingObstaclesMoveComplete()) AdvanceFall(); //wait for movingobstacles to move completely
        }

        if (willDropDeath && PlayerController.playerController.isTimeRewinding) //intercept by timerewinding
        {
            DropKillBox();
            return;
        }
        //intercept by gameover: at PlayerController - KillCharacter
        if ((TurnManager.turnManager.CLOCK || willDropDeath) && !isFallComplete) //update fall also when willDropDeath, independantly
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, checkDistance, layerMask))
            {
                if (hit.collider.CompareTag("Player") && !willDropDeath) //Stamp Kill
                {
                    if (hit.collider.name == "Player")
                    {
                        PlayerController.playerController.KillCharacter();
                        return;
                    }
                    if (hit.collider.name == "Phantom")
                    {
                        PhantomController.phantomController.KillCharacter();
                        return;
                    }
                }

                // Tile detected, keep Y position constraint to keep the box stable
                rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
                transform.position = new Vector3(transform.position.x, Mathf.Ceil(transform.position.y) - 1.0f + checkDistance, transform.position.z);

                if (willDropDeath) //hit the ground hard -> Drop Death
                {
                    DropKillBox();
                }
                else
                {
                    isFallComplete = true;
                    isMoveComplete = true; //all tasks done at this turn //toggle when Not willDropDeath.
                }
            }
            else
            {
                if (willDropDeath)
                {
                    if (transform.position.y < -maxFallHeight) //fall to the void -> Drop Death
                    {
                        DropKillBox();
                    }
                }
            }
        }
        
        // There should be a code indicating:
        // if it has touched the ground, then isMoveComplete = true. 
    }
    public void AdvanceTurn()
    {
        if (!isBeingPushed)
        {
            //AdvanceFall();
            if (gameObject.activeSelf) isWaitingToCheckFall = true;  //wait.
            else isMoveComplete = true;  //just pass
        }
        isBeingPushed = false;
    }

    public void DropKillBox()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        willDropDeath = false;
        isFallComplete = true;

        gameObject.SetActive(false);
        if (TurnManager.turnManager.CLOCK && isWaitingToCheckFall) isMoveComplete = true;
        //some particle effect and sound effect
    }
    public void AdvanceFall() //can refactor with characterbase.advancefall()
    {
        isWaitingToCheckFall = false;

        if (!willDropDeath)
        {
            // If no tile is detected, allow the box to fall
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit1, checkDistance, layerMask))
            {
                isMoveComplete = true; //bypass fall.
            }
            else
            {
                //check if the character can Survive from this height
                if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit2, checkDistance + rayJumpInterval * maxFallHeight, layerMask))
                {
                    willDropDeath = true;
                    isMoveComplete = true; //just pass turn and fall itself alone.
                }
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                isFallComplete = false; //fall start!
            }
        }
        else
        {
            isMoveComplete = true;
        }
    }
    public bool TryMove(Vector3 direction)
    {
        if (isBeingPushed) return false; // preventing player & phantom simultaneous push 

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, moveDistance, layerMask) || Physics.Raycast(transform.position + new Vector3(0, checkDistance*0.8f, 0), direction, out RaycastHit hit1, moveDistance, layerMask))
        { 
            isBeingPushed = false;
            return false;
        }
        
        if (Physics.Raycast(transform.position, Vector3.up, out RaycastHit hitUp, moveDistance/2, layerMask)) //look up.
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
        //isMoveComplete = true;
        //isFallComplete = false;

        //AdvanceFall();
        isWaitingToCheckFall = true; //wait.
    }
    public void SaveCurrentPos()
    {
        if (willDropDeath || !gameObject.activeSelf) //save before kill.
        {
            positionIterator.Add((Vector3.zero, false));
        }
        else
        {
            positionIterator.Add((transform.position, true));
        }
    }
    public void RemoveLog(int k)
    {
        positionIterator.RemoveLastK(k);
    }
    public void RestoreState() // updating for time rewind
    {
        var current = positionIterator.Current;
        if (current.Item2)
        {
            transform.position = current.Item1;
        }
        gameObject.SetActive(current.Item2);
    }
}