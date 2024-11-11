using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public abstract class CharacterBase : MonoBehaviour
{
    public Animator animator;
    public Vector3 playerCurPos;
    public Quaternion playerCurRot;

    // Movement-related properties
    public float moveSpeedHor;
    public float moveSpeedVer;
    public float turnSpeed;
    public float curHopDir = 1.0f;
    public float curTurnAngle = 0.0f;
    public float curSpeed { get; set; }
    public float curHopSpeed { get; set; }
    public float curRotSpeed { get; set; }

    //for state decision 'branch' from spatial condition check
    protected bool isUnderJump = false;

    // Raycasting for spatial checks
    protected RaycastHit hitWall, hitFloor, hitOverFloor, hitUnderFloor;
    protected float rayDistance = 1.0f;
    protected float rayJumpInterval = 1.0f;
    protected int layerMask = 1 << 0;

    // State management
    protected List<IState<CharacterBase>> listCurTurn, listStay, listTurn, listMoveForward, listMoveSideRear, listHopForward, listHopSideRear;
    protected StateMachine<CharacterBase> sm;
    // To make it controllable from the inspector, separated same scripts into 2 variables...
    public CharacterIdle cIdle; public CharacterMove cMove; public CharacterTurn cTurn; public CharacterHop cHop;
    protected IState<CharacterBase> idle, move, turn, hop;
    // index of state list
    protected int seq = 0;
    // task of a state ended, need to jump to next state (next index of the state list)
    //can be changed from state's DoneAction func, through sender
    public bool doneAction = false;

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    protected virtual void Start()
    {
        //Raycast for spatial condition check
        hitWall = new RaycastHit();
        hitFloor = new RaycastHit();
        hitOverFloor = new RaycastHit();
        hitUnderFloor = new RaycastHit();
        rayDistance = 1.0f;
        rayJumpInterval = 1.0f;
        layerMask = 1 << 0;

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

    /*protected virtual void Update()
    {
        sm.DoOperateUpdate();
        if (doneAction)
        {
            seq++;
            if (seq < listCurTurn.Count)
            {
                sm.SetState(listCurTurn[seq]);
                doneAction = false;
            }
            else
            {
                sm.SetState(listStay[0]);
                doneAction = false;
            }
        }
    }*/

    protected virtual void HandleMovementInput(string command)
    {
        switch (command)
        {
            case "w":
                HandleDirection(Vector3.forward, new float[] { 0.0f, -90.0f, 180.0f, 90.0f }, new Vector3(0, 0, 2.0f));
                break;
            case "s":
                HandleDirection(Vector3.back, new float[] { 180.0f, 90.0f, 0.0f, -90.0f }, new Vector3(0, 0, -2.0f));
                break;
            case "a":
                HandleDirection(Vector3.left, new float[] { -90.0f, 180.0f, 90.0f, 0.0f }, new Vector3(-2.0f, 0, 0));
                break;
            case "d":
                HandleDirection(Vector3.right, new float[] { 90.0f, 0.0f, -90.0f, 180.0f }, new Vector3(2.0f, 0, 0));
                break;
            case "r": // keep idle and pass turn
                listCurTurn = listStay;
                StartAction();
                break;
        }
    }

    protected virtual void StartAction()
    {
        seq = 0; 
        sm.SetState(listCurTurn[seq]); 
    }

    // below this, functions for spatial check //

    protected void HandleDirection(Vector3 direction, float[] angles, Vector3 rayOffset) //from HandleMovementInput() with local direction Array
    {
        int angleIndex = Mathf.RoundToInt(this.transform.eulerAngles.y / 90) % 4;
        curTurnAngle = angles[angleIndex]; //relative orientation!!

        Debug.DrawRay(playerCurPos + rayOffset, transform.up * -rayDistance, Color.red, 0.8f);
        if (Physics.Raycast(playerCurPos + rayOffset + new Vector3(0, 0.1f, 0), -transform.up, out hitUnderFloor, rayDistance + rayJumpInterval + 0.1f, layerMask)) //void check
        {
            isUnderJump = !Physics.Raycast(playerCurPos + rayOffset + new Vector3(0, 0.1f, 0), -transform.up, out hitFloor, rayDistance + 0.1f, layerMask); //can jump under?

            HandleActionBasedOnAngle(); //need to check more things.
        }
        else //if void
        {
            if (curTurnAngle != 0.0f) //if void is not in front of player, just turn(rotate) in its place (turn update happens.)
            {
                listCurTurn = listTurn; //we should set animation trigger on Enter and Exit of PlayerTurn state.
                StartAction(); //cycle starts!!!
            }
            //if void is in front of player, no action and no turn update.
        }
    }
    protected void HandleActionBasedOnAngle()  //local direction and check whether 'wall or jumpable stair(over 1.0)' is in front of player's oriented direction
    {
        Vector3 rayDirection = Vector3.zero;
        if (curTurnAngle == 0.0f)
            rayDirection = transform.forward;
        else if (curTurnAngle == 180.0f)
            rayDirection = -transform.forward;
        else if (curTurnAngle == -90.0f)
            rayDirection = -transform.right;
        else if (curTurnAngle == 90.0f)
            rayDirection = transform.right;

        Debug.DrawRay(playerCurPos, rayDirection * rayDistance, Color.blue, 0.8f);
        if (Physics.Raycast(playerCurPos, rayDirection, out hitWall, rayDistance, layerMask)) //wall check
        {
            if (/*tag is Lever  AND  can push lever by this side*/false) //lever.
            {
                ChooseAction(listStay, listTurn); //we should set animation trigger on Enter and Exit of PlayerIdle state.
                StartAction(); //cycle starts!!!
            }
            else if (/*tag is box  AND  can push box by this side*/false) //box.
            {
                ChooseAction(listMoveForward, listMoveSideRear); //we should set animation trigger on Enter and Exit of PlayerMove state.
                StartAction(); //cycle starts!!!
            }
            else if (!Physics.Raycast(playerCurPos + new Vector3(0, 0.5f, 0), rayDirection, out hitOverFloor, rayDistance, layerMask)) //available to jump over
            {
                curHopDir = 1.0f; //Jump Over
                ChooseAction(listHopForward, listHopSideRear);
                StartAction(); //cycle starts!!!
            }
            else //if not
            {
                if (curTurnAngle != 0.0f) //if wall is not in front of player, just turn(rotate) in its place (turn update happens.)
                {
                    listCurTurn = listTurn; //we should set animation trigger on Enter and Exit of PlayerTurn state.
                    StartAction(); //cycle starts!!!
                }
                //if wall is in front of player, no action and no turn update.
            }
        }
        else
        {
            if (isUnderJump)
            {
                curHopDir = -1.0f; //Jump Under
                ChooseAction(listHopForward, listHopSideRear);
                isUnderJump = false;
            }
            else
            {
                ChooseAction(listMoveForward, listMoveSideRear); //Horizontal Move
            }
            StartAction(); //cycle starts!!!
        }
    }

    protected void ChooseAction(List<IState<CharacterBase>> statelist1, List<IState<CharacterBase>> statelist2)
    {
        listCurTurn = curTurnAngle == 0.0f ? statelist1 : statelist2;
    }

}
