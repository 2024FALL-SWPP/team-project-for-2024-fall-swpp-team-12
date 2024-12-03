using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    public Vector3 targetTranslation;
    public float moveDistance = 2.0f;
    public float checkDistance;
    private Rigidbody rb;

    public TurnLogIterator<(Vector3, bool)> positionIterator;
    public bool isMoveComplete = false;
    public bool isFallComplete = true;

    private bool isBeingPushed = false;
    public bool willDropDeath = false;
    private bool isWaitingToCheckFall = false;

    private float rayJumpInterval = 1.0f;
    private float maxFallHeight = 10.0f;
    private int layerMask = (1 << 0) | (1 << 3) | (1 << 6) | (1 << 8); //detect default(0), player(3), lever(6) and box(8) layer.
    private int layerMaskFall = (1 << 0) | (1 << 3) | (1 << 6); //don't detect other boxes.
    public float boxLayer = 0.0f;
    private void Start()
    {
        targetTranslation = transform.position;

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
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, checkDistance + boxLayer, layerMaskFall))
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
    }
    public void AdvanceTurn()
    {
        if (!isBeingPushed)
        {
            if (gameObject.activeSelf) isWaitingToCheckFall = true;  //wait.
            else isMoveComplete = true;  //just pass
        }
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
        //Check overlapping target position - Phantom
        if (PhantomController.phantomController.isPhantomExisting && isBeingPushed)
        {
            Vector3 phantomTargetPosition = PhantomController.phantomController.targetTranslation;
            if (phantomTargetPosition.x == targetTranslation.x && phantomTargetPosition.z == targetTranslation.z &&
                phantomTargetPosition.y == Mathf.Ceil(targetTranslation.y)) //for size max 100. cannot check for taller box.
            {
                PhantomController.phantomController.willBoxKillPhantom = true;
            }
        }
        if (!willDropDeath)
        {
            int layermask;
            float fallHeightCheck;
            if (isBeingPushed) //don't detect other boxes
            {
                layermask = layerMaskFall;
                fallHeightCheck = maxFallHeight + boxLayer;
            }
            else //normal raycast.
            {
                layermask = layerMask;
                fallHeightCheck = maxFallHeight;
            }
            // If no tile is detected, allow the box to fall
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit1, checkDistance + (fallHeightCheck - maxFallHeight), layermask))
            {
                if (isBeingPushed)
                {
                    isMoveComplete = true; //bypass fall.
                }
                else
                {
                    if (hit1.collider.CompareTag("Box")) //on the box.
                    {
                        //wait for the box below to check this.
                    }
                    else //at floor. - no fall.
                    {
                        CheckChainFall(false, boxLayer + checkDistance * 2);
                        isMoveComplete = true; //bypass fall.
                    }
                }
            }
            else
            {
                if (!isBeingPushed) //the lowest box of the group, and it is about to fall.
                {
                    CheckChainFall(true, boxLayer + checkDistance * 2);
                }
                canSurviveFall(fallHeightCheck, layermask);
            }
        }
        else
        {
            isMoveComplete = true; //keep falling to death
        }
        isBeingPushed = false; //Unlock.
    }

    public void canSurviveFall(float fallHeightCheck, int layermask)
    {
        //check if the character can Survive from this height
        if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit2, checkDistance + fallHeightCheck, layermask))
        {
            willDropDeath = true;
            isMoveComplete = true; //just pass turn and fall itself alone.
        }
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        isFallComplete = false; //fall start!
    }

    public void CheckChainFall(bool doFall, float layer) //Recursion.
    {
        if (isBeingPushed) return; //if locked - no CheckChainFall.(first box, calls this function only when !isBeingPushed, no problem ///but next boxes, can be Locked by isBeingPushed)
        if (Physics.Raycast(transform.position, Vector3.up, out RaycastHit hitUp, checkDistance + moveDistance / 4, 1 << 8))
        {
            Box box = hitUp.collider.gameObject.GetComponent<Box>();
            box.boxLayer = layer;
            if (doFall)
            {
                box.canSurviveFall(maxFallHeight + box.boxLayer, layerMaskFall);
            }
            else
            {
                box.isMoveComplete = true; //no fall.
            }
            box.CheckChainFall(doFall, box.boxLayer + box.checkDistance * 2);
        }
    }

    public bool TryMove(Vector3 direction, float layer)
    {
        if (isBeingPushed) return false; //if locked - no TryMove(). // to prevent player & phantom simultaneous push
        boxLayer = layer;

        //Check Wall Forward
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, moveDistance, layerMask) || Physics.Raycast(transform.position + new Vector3(0, checkDistance * 0.8f, 0), direction, out RaycastHit hit1, moveDistance, layerMask))
        {
            targetTranslation = transform.position;
            return false;
        }

        targetTranslation = transform.position + direction * moveDistance;

        //Check overlapping target position - Player
        Vector3 playerTargetPosition = PlayerController.playerController.targetTranslation;
        if (playerTargetPosition.x == targetTranslation.x && playerTargetPosition.z == targetTranslation.z &&
            playerTargetPosition.y == Mathf.Ceil(targetTranslation.y)) //for size max 100. cannot check for taller box.
        {
            targetTranslation = transform.position; //roll back
            return false;
        }

        //Check overlapping target position - Box
        if (PlayerController.playerController.checkOverlappingBox) //only check when 2nd trymove.
        {
            Vector3 boxTargetPosition = PlayerController.playerController.tempTagetPositionOfBox;
            if (boxTargetPosition.x == targetTranslation.x && boxTargetPosition.z == targetTranslation.z &&
                Mathf.Ceil(boxTargetPosition.y) == Mathf.Ceil(targetTranslation.y)) //for size max 100. cannot check for taller box.
            {
                targetTranslation = transform.position; //roll back
                return false;
            }
        }
        else
        {
            PlayerController.playerController.tempTagetPositionOfBox = targetTranslation;
            PlayerController.playerController.checkOverlappingBox = true;
        }

        //Check Rider
        if (Physics.Raycast(transform.position, Vector3.up, out RaycastHit hitUp, checkDistance + moveDistance / 4, layerMask)) //look up.
        {
            switch (hitUp.collider.tag)
            {
                case "Box": //box stack yeah
                    {
                        Box box = hitUp.collider.gameObject.GetComponent<Box>();
                        box.TryMove(direction, boxLayer + checkDistance * 2); //Recursion.
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

        isBeingPushed = true; //Lock.
        StartCoroutine(SmoothMove(direction, PlayerController.playerController.moveSpeedHor));
        return true;
    }

    private IEnumerator SmoothMove(Vector3 direction, float moveSpeed)
    {
        Vector3 startPosition = transform.position;

        while (Vector3.Distance(transform.position, targetTranslation) > 0.01f && Vector3.Distance(transform.position, startPosition) < 2.0f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetTranslation, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetTranslation;

        isWaitingToCheckFall = true;
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