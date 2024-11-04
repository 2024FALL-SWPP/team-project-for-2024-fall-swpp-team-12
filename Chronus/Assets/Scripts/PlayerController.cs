using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Move,
        Turn,
        Hop
    }

    private bool doSelectAction = false;


    private bool turnClock = false;
    //when turnClock false -> true  state list reading~~
    //next time: when turnClock false -> true: turn count +1!!!

    // index of state list
    private int seq = 0;
    // task of a state ended, need to jump to next state (next index of the state list)
    public bool doneAction = false;


    //current transformation write.
    public Vector3 playerCurPos;
    public Quaternion playerCurRot;

    public float curHopDir;
    public float curTurnAngle;

    public float moveSpeedHor;
    public float moveSpeedVer;
    public float turnSpeed;
    public float curSpeed { get; set; }
    public float curHopSpeed { get; set; }
    public float curRotSpeed { get; set; }


    private Dictionary<PlayerState, IState<PlayerController>> dicState = new Dictionary<PlayerState, IState<PlayerController>>();
    
    //state list write.
    private List<IState<PlayerController>> listCurTurn = new List<IState<PlayerController>>();


    private List<IState<PlayerController>> listStay = new List<IState<PlayerController>>();

    private List<IState<PlayerController>> listMoveForward = new List<IState<PlayerController>>();
    private List<IState<PlayerController>> listMoveSideRear = new List<IState<PlayerController>>();

    private List<IState<PlayerController>> listHopForward = new List<IState<PlayerController>>();
    private List<IState<PlayerController>> listHopSideRear = new List<IState<PlayerController>>();

    private StateMachine<PlayerController> sm;

    private RaycastHit hitWall;
    private RaycastHit hitFloor;
    private RaycastHit hitOverFloor;
    private RaycastHit hitUnderFloor;
    private float rayDistance;
    private float rayJumpInterval;
    private int layerMask;

    private bool isUnderJump = false;

    // Start is called before the first frame update
    void Start()
    {
        playerCurPos = this.transform.position;
        playerCurRot = this.transform.rotation;

        moveSpeedHor = 6.0f;
        moveSpeedVer = 0.5f * moveSpeedHor;
        turnSpeed = 480.0f;


        //state names!
        IState<PlayerController> idle = new PlayerIdle();
        IState<PlayerController> move = new PlayerMove();
        IState<PlayerController> turn = new PlayerTurn();
        IState<PlayerController> hop = new PlayerHop();

        //dictionary. (not useful this time yet lol)
        dicState.Add(PlayerState.Idle, idle);
        dicState.Add(PlayerState.Move, move);
        dicState.Add(PlayerState.Turn, turn);
        dicState.Add(PlayerState.Hop, hop);

        //State List!!

        //idle -> idle
        listStay.Add(idle);
        //idle -> move(forward.) -> idle
        listMoveForward.Add(move);
        //idle -> turn(x) -> move -> idle    (x=f(i)) (i:input, f: how much to rotate)
        listMoveSideRear.Add(turn);
        listMoveSideRear.Add(move);
        //idle -> hop(over or under) -> idle
        listHopForward.Add(hop);
        //idle -> turn(x) -> hop(over or under) -> idle    (x=f(i)) (i:input, f: how much to rotate)
        listHopSideRear.Add(turn);
        listHopSideRear.Add(hop);

        //StateMachine we handle.
        sm = new StateMachine<PlayerController>(this, dicState[PlayerState.Idle]);

        hitWall = new RaycastHit();
        hitFloor = new RaycastHit();
        hitOverFloor = new RaycastHit();
        hitUnderFloor = new RaycastHit();
        rayDistance = 1.0f;
        rayJumpInterval = 1.0f;
        layerMask = 1 << 0;
    }

    void Update()
    {
        if (!turnClock) {
            HandleMovementInput();

            // select action which is towarding the orientation (curTurnAngle: Relative orientation)
            if (doSelectAction)
            {
                HandleActionBasedOnAngle();
                doSelectAction = false;
            }
        }
        else //if (turnClock)
        {
            //update when turnClock is ON
            sm.IsDoneAction();

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
                    sm.SetState(dicState[PlayerState.Idle]);
                    doneAction = false;
                    turnClock = false;
                    //seq = 0;
                }
            }
        }
        print(curTurnAngle);

        //update always ~
        sm.DoOperateUpdate();
    }

    private void HandleMovementInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) 
        {
            HandleDirection(Vector3.forward, new float[] { 0.0f, -90.0f, 180.0f, 90.0f }, new Vector3(0, 0, 2.0f));
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            HandleDirection(Vector3.back, new float[] { 180.0f, 90.0f, 0.0f, -90.0f }, new Vector3(0, 0, -2.0f));
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            HandleDirection(Vector3.left, new float[] { -90.0f, 180.0f, 90.0f, 0.0f }, new Vector3(-2.0f, 0, 0));
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            HandleDirection(Vector3.right, new float[] { 90.0f, 0.0f, -90.0f, 180.0f }, new Vector3(2.0f, 0, 0));
        }
    }

    private void HandleDirection(Vector3 direction, float[] angles, Vector3 rayOffset)
    {
        playerCurPos = this.transform.position;

        Debug.DrawRay(playerCurPos + rayOffset, transform.up * -rayDistance, Color.red, 0.8f);
        if (Physics.Raycast(playerCurPos + rayOffset + new Vector3(0, 0.1f, 0), -transform.up, out hitUnderFloor, rayDistance + rayJumpInterval + 0.1f, layerMask))
        {
            isUnderJump = !Physics.Raycast(playerCurPos + rayOffset + new Vector3(0, 0.1f, 0), -transform.up, out hitFloor, rayDistance + 0.1f, layerMask);

            int angleIndex = Mathf.RoundToInt(this.transform.eulerAngles.y / 90) % 4;
            curTurnAngle = angles[angleIndex]; //orientation!!
            
            doSelectAction = true;
        }
    }

    private void HandleActionBasedOnAngle()
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
        if (Physics.Raycast(playerCurPos, rayDirection, out hitWall, rayDistance, layerMask))
        {
            if (!Physics.Raycast(playerCurPos + new Vector3(0, 0.5f, 0), rayDirection, out hitOverFloor, rayDistance, layerMask))
            {
                curHopDir = 1.0f;
                listCurTurn = listHopSideRear;
                turnClock = true;
                seq = 0;
                sm.SetState(listCurTurn[seq]);
            }
        }
        else
        {
            if (isUnderJump)
            {
                curHopDir = -1.0f;
                listCurTurn = listHopSideRear;
                isUnderJump = false;
            }
            else
            {
                listCurTurn = curTurnAngle == 0.0f ? listMoveForward : listMoveSideRear;
            }
            turnClock = true;
            seq = 0;
            sm.SetState(listCurTurn[seq]);
        }
    }
}
