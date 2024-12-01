using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    public Vector3 targetTranslation { get; set; }
    public Vector3 targetDirection { get; set; }

    public Animator animator;
    private Rigidbody rb;
    public Vector3 playerCurPos;
    public Quaternion playerCurRot;

    // Movement-related properties
    public float moveSpeedHor, moveSpeedVer, turnSpeed;
    public float curHopDir = 1.0f;
    public float curTurnAngle = 0.0f;
    public float curSpeed { get; set; }
    public float curHopSpeed { get; set; }
    public float curRotSpeed { get; set; }

    public Vector3 pushDirection = Vector3.zero; // to check if being pushed
    public float pushSpeed = 0;
    public bool isRidingBox = false;
    private const float BLOCK_SIZE = 2.0f;

    // State management
    protected List<IState<CharacterBase>> listCurTurn, listStay, listTurn, listMoveForward, listMoveSideRear, listHopForward, listHopSideRear;
    protected StateMachine<CharacterBase> sm;
    // To make it controllable from the inspector, separated same scripts into 2 variables...
    public CharacterIdle cIdle; public CharacterMove cMove; public CharacterTurn cTurn; public CharacterHop cHop;
    protected IState<CharacterBase> idle, move, turn, hop;
    // index of state list
    protected int listSeq = 0;

    protected float rayDistance = 1.0f;
    protected float rayJumpInterval = 1.0f;
    protected float maxFallHeight = 10.0f;
    protected int layerMask = (1 << 0) | (1 << 6); //default, lever

    // task of a state ended, need to jump to next state (next index of the state list)
    // can be changed from state's DoneAction func, through sender
    public bool doneAction = false; // for state machine

    public bool isMoveComplete = false; // for turn mechanism
    public bool isFallComplete = true;
    public bool willDropDeath = false;
    public bool willLaserKillCharacter = false;
    

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    protected virtual void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        moveSpeedHor = 6.0f;
        moveSpeedVer = 0.5f * moveSpeedHor;
        turnSpeed = 480.0f;

        playerCurPos = transform.position;
        playerCurRot = transform.rotation;

        idle = cIdle; move = cMove; turn = cTurn; hop = cHop;

        //idle -> idle
        listStay = new List<IState<CharacterBase>> { idle };
        //idle -> turn -> idle
        listTurn = new List<IState<CharacterBase>> { turn };
        //idle -> move(forward.) -> idle
        listMoveForward = new List<IState<CharacterBase>> { move };
        //idle -> turn(x) -> move -> idle    (x=f(i)) (i:input, f: how much to rotate)
        listMoveSideRear = new List<IState<CharacterBase>> { turn, move };
        //idle -> hop(over or under) -> idle
        listHopForward = new List<IState<CharacterBase>> { hop };
        //idle -> turn(x) -> hop(over or under) -> idle    (x=f(i)) (i:input, f: how much to rotate)
        listHopSideRear = new List<IState<CharacterBase>> { turn, hop };

        sm = new StateMachine<CharacterBase>(this, idle);
    }

    protected virtual void Update()
    {
        if (TurnManager.turnManager.CLOCK)
        {
            if (!isMoveComplete && isFallComplete) // when turn is on progress
            {
                sm.IsDoneAction();
                if (doneAction) // next state in the state list
                {
                    doneAction = false;
                    if (listSeq >= 0)
                    {
                        listSeq++; // next index
                        if (listSeq < listCurTurn.Count)
                        {
                            sm.SetState(listCurTurn[listSeq]);
                        }
                        else
                        {
                            listSeq = -1; // no list update.
                            sm.SetState(idle);
                        }
                    }
                }
            }
            sm.DoOperateUpdate();
        }
        if ((TurnManager.turnManager.CLOCK || willDropDeath) && !isFallComplete)
        {
            if (willLaserKillCharacter) //check after fall
            {
                willLaserKillCharacter = false;
                this.KillCharacter();
                return;
            }

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayDistance, layerMask))
            {
                // Tile detected, keep Y position constraint to keep the box stable
                rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
                transform.position = new Vector3(transform.position.x, Mathf.Ceil(transform.position.y), transform.position.z);

                if (willDropDeath) //hit the ground hard -> Drop Death
                {
                    KillCharacter();
                }
                else
                {
                    playerCurPos = transform.position; //update position after fall.
                    isFallComplete = true;
                    isMoveComplete = true; //all tasks done at this turn
                }
            }
            else
            {
                if (willDropDeath)
                {
                    if (transform.position.y < -maxFallHeight) //fall to the void -> Drop Death
                    {
                        KillCharacter();
                    }
                }
            }
        }
    }

    public virtual void KillCharacter()  //gameover, initialize
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        willDropDeath = false; //already dead.
        isFallComplete = true; //ended fall.
    }

    public virtual void AdvanceFall()
    {
        if (willLaserKillCharacter) //check before fall (after all obstacles move)
        {
            willLaserKillCharacter = false;
            this.KillCharacter();
            return;
        }
        if (!willDropDeath)
        {
            // If no tile is detected, allow the box to fall
            if (Physics.Raycast(playerCurPos, Vector3.down, out RaycastHit hit1, rayDistance, layerMask))
            {
                isMoveComplete = true; //bypass fall.
            }
            else
            {
                //check if the character can Survive from this height
                if (!Physics.Raycast(playerCurPos, Vector3.down, out RaycastHit hit2, rayDistance + rayJumpInterval * maxFallHeight, layerMask))
                {
                    //cannot survive D:
                    willDropDeath = true; //itself blocks input.
                    isMoveComplete = true; //just pass turn and fall itself alone. (of course blocks input by willDropDeath)
                }
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                isFallComplete = false;
            }
        }
        else
        {
            isMoveComplete = true;
        }
    }

    protected virtual void HandleMovementInput(string command)
    {
        switch (command)
        {
            case "w":
                targetDirection = Vector3.forward;
                break;
            case "s":
                targetDirection = Vector3.back;
                break;
            case "a":
                targetDirection = Vector3.left;
                break;
            case "d":
                targetDirection = Vector3.right;
                break;
            case "r":
                listCurTurn = listStay;
                if (!isRidingBox) targetTranslation = playerCurPos;
                StartAction();
                return;
        }

        TryToMoveToDirection();
    }

    protected virtual void StartAction()
    {
        listSeq = 0;
        sm.SetState(listCurTurn[listSeq]);
    }


    // below this, functions for spatial check //

    protected void RotateInPlace()
    {
        if (curTurnAngle != 0.0f)
        {
            listCurTurn = listTurn;
            targetTranslation = playerCurPos; // staying at the previous grid // because stuck at the wall
            StartAction();
        }
        //do nothing, no turn update.
    }

    protected void TryJumpOver() //targetDirection, targetTranslation: global by aspect of this block
    {
        if (!Physics.Raycast(playerCurPos + new Vector3(0, 0.5f, 0), targetDirection, out RaycastHit hitOverFloor, BLOCK_SIZE, layerMask))
        // can jump over
        {
            curHopDir = BLOCK_SIZE / 2;
            targetTranslation += new Vector3(0, curHopDir, 0);
            ChooseAndStartAction(listHopForward, listHopSideRear);
        }
        else
        {
            RotateInPlace();
        }
    }

    protected void TryToMoveToDirection()
    {
        Vector3 rayOffset = targetDirection * BLOCK_SIZE;
        targetTranslation = playerCurPos + rayOffset;
        float angleDifference = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);
        curTurnAngle = Mathf.Round(angleDifference / 90) * 90;

        // First, check if there's an wall-like object to that target direction. 
        if (Physics.Raycast(transform.position - new Vector3(0,0.1f,0), targetDirection, out RaycastHit hit, BLOCK_SIZE, layerMask))
        {
            switch (hit.collider.tag)
            {
                case "Lever":
                    {
                        Lever lever = hit.collider.gameObject.GetComponent<Lever>();
                        if (lever.canToggleDirection == targetDirection)
                        {
                            lever.doPushLever = true;
                            ChooseAndStartAction(listStay, listTurn);
                        }
                        else
                        {
                            RotateInPlace();
                        }
                        break;
                    }

                case "Box":
                    {
                        Box box = hit.collider.gameObject.GetComponent<Box>();
                        if (box.willDropDeath) //box is heading to death
                        {
                            RotateInPlace();
                            break;
                        }
                        if (box.TryMove(targetDirection))
                        {
                            ChooseAndStartAction(listMoveForward, listMoveSideRear);
                        }
                        else
                        {
                            TryJumpOver();
                        }
                        break;
                    }

                default:
                    {
                        TryJumpOver();
                        break;
                    }
            }
        }
        else // if there is no wall: then check if target position has floor
        {
            Vector3 rayStart = transform.position + rayOffset;
            Debug.DrawRay(rayStart, -transform.up * BLOCK_SIZE, Color.blue, 1.0f);
            if (Physics.Raycast(rayStart, -transform.up, out RaycastHit hitGround, rayDistance + rayJumpInterval*maxFallHeight, layerMask))
            {
                if (hitGround.collider.CompareTag("Box")) //box is heading to death
                {
                    Box box = hitGround.collider.gameObject.GetComponent<Box>();
                    if (box.willDropDeath)
                    {
                        RotateInPlace();
                        return;
                    }
                }
                GameObject floor = hitGround.collider.gameObject;

                Collider playerCollider = GetComponent<Collider>();
                Collider floorCollider = floor.GetComponent<Collider>();
                float playerBottomY = Mathf.Round(playerCollider.bounds.center.y - playerCollider.bounds.extents.y);
                float floorTopY = Mathf.Round(floorCollider.bounds.center.y + floorCollider.bounds.extents.y);

                if (Mathf.Approximately(playerBottomY, floorTopY)/* || (playerBottomY - floorTopY > (BLOCK_SIZE / 2))*/) // flat move
                {
                    ChooseAndStartAction(listMoveForward, listMoveSideRear);
                }
                else if (Mathf.Approximately(playerBottomY, floorTopY + (BLOCK_SIZE / 2)) || (playerBottomY - floorTopY > (BLOCK_SIZE / 2))) // jump down
                {
                    curHopDir = floorTopY - playerBottomY;
                    //curHopDir = -(BLOCK_SIZE / 2);
                    targetTranslation += new Vector3(0, curHopDir, 0);
                    ChooseAndStartAction(listHopForward, listHopSideRear);
                }
            }
            else
            {
                RotateInPlace();
            }
        }
    }

    protected void ChooseAndStartAction(List<IState<CharacterBase>> statelist1, List<IState<CharacterBase>> statelist2)
    {
        listCurTurn = curTurnAngle == 0.0f ? statelist1 : statelist2;
        StartAction();
    }

}
