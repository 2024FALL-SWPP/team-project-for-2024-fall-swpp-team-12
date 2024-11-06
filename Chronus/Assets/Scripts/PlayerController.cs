using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController playerController; //public accessibility using Singleton

    public enum PlayerState
    {
        Idle,
        Move,
        Turn,
        Hop
    }

    public Animator animator;
    //2 separated parts of 'input and spatial condition check'
    private bool doSelectAction = false;

    //when turnClock false -> true  state list reading (DoneAction <-> next state ...)
    public bool turnClock = false;
    //when turnClock true -> false: turn check for player: true -> for turn count +1

    // index of state list
    private int seq = 0;

    // task of a state ended, need to jump to next state (next index of the state list)
        //can be changed from state's DoneAction func, through sender
    public bool doneAction = false;

    public List<string> listCommandLog; //command log for time reverse and phantom action


    //Current transformation write.
    public Vector3 playerCurPos;
    public Quaternion playerCurRot;

    //condition -> rotate or hop direction
    public float curHopDir;
    public float curTurnAngle;

    //act speed.
    public float moveSpeedHor;
    public float moveSpeedVer;
    public float turnSpeed;
    //sender get & set. (speed)
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

    private Dictionary<string, List<IState<PlayerController>>> dicCommand = new Dictionary<string, List<IState<PlayerController>>>();

    //the state machine of player.
    private StateMachine<PlayerController> sm;

    //for spatial condition check
    private RaycastHit hitWall;
    private RaycastHit hitFloor;
    private RaycastHit hitOverFloor;
    private RaycastHit hitUnderFloor;
    private float rayDistance;
    private float rayJumpInterval;
    private int layerMask;

    //for state decision 'branch' from spatial condition check
    private bool isUnderJump = false;


    private void Awake() //Singleton
    {
        if (PlayerController.playerController == null)
        {
            PlayerController.playerController = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        List<List<IState<PlayerController>>> listCommandLog = new List<List<IState<PlayerController>>>();

        animator = transform.Find("MainCharacter").GetComponent<Animator>();
        
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

        
        dicCommand.Add("Stay", listStay);
        dicCommand.Add("MoveForward", listMoveForward);
        dicCommand.Add("MoveSideRear", listMoveSideRear);
        dicCommand.Add("HopForward", listHopForward);
        dicCommand.Add("HopSideRear", listHopSideRear);
        

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

    void Update() //Input&Condition Check <-> State Change
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

            if (doneAction) //next state in the state list
            {
                seq++; //next index
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
                    TurnManager.turnManager.dicTurnCheck["Player"] = true;
                }
            }
        }
        //print(curTurnAngle);

        //update always ~
        sm.DoOperateUpdate();
    }

    private void HandleMovementInput() //global direction and check whether 'void or floor(plain or below 1.0)' is in front of player
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
        else if (Input.GetKeyDown(KeyCode.R))
        {
            KeepIdleAndPassTurn();
        }
    }

    private void KeepIdleAndPassTurn()
    {
        listCurTurn = listStay;
        listCommandLog.Add("Stay");
        turnClock = true; // start current turn!!!
        seq = 0;
        sm.SetState(listCurTurn[seq]);
    }

    private void HandleDirection(Vector3 direction, float[] angles, Vector3 rayOffset) //from HandleMovementInput() with local direction Array
    {
        playerCurPos = this.transform.position;

        Debug.DrawRay(playerCurPos + rayOffset, transform.up * -rayDistance, Color.red, 0.8f);
        if (Physics.Raycast(playerCurPos + rayOffset + new Vector3(0, 0.1f, 0), -transform.up, out hitUnderFloor, rayDistance + rayJumpInterval + 0.1f, layerMask))
        {
            isUnderJump = !Physics.Raycast(playerCurPos + rayOffset + new Vector3(0, 0.1f, 0), -transform.up, out hitFloor, rayDistance + 0.1f, layerMask);

            int angleIndex = Mathf.RoundToInt(this.transform.eulerAngles.y / 90) % 4;
            curTurnAngle = angles[angleIndex]; //orientation!!
            
            doSelectAction = true; //need to check more things. go to HandleActionBasedOnAngle()
        }
    }

    private void HandleActionBasedOnAngle()  //local direction and check whether 'wall or jumpable stair(over 1.0)' is in front of player's oriented direction
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
                listCurTurn = curTurnAngle == 0.0f ? listHopForward : listHopSideRear;
                string s = curTurnAngle == 0.0f ? "HopForward" : "HopSideRear";
                listCommandLog.Add(s);
                turnClock = true; // start current turn!!!
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
                listCommandLog.Add("HopSideRear");
                isUnderJump = false;
            }
            else
            {
                if (curTurnAngle == 0.0f)
                {
                    listCurTurn = listMoveForward;
                    listCommandLog.Add("MoveForward");
                }
                else
                {
                    listCurTurn = listMoveSideRear;
                    listCommandLog.Add("MoveSideRear");
                }

            }
            turnClock = true; // start current turn!!!
            seq = 0;
            sm.SetState(listCurTurn[seq]);
        }
    }
}
