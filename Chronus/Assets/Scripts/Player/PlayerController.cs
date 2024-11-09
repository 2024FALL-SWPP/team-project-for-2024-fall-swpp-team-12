using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController playerController; // singleton

    public enum PlayerState { Idle, Move, Turn, Hop }

    public Animator animator;

    private string curKey = "r";

    //2 separated parts of 'input and spatial condition check' this bool variable for switching between two sides.
    private bool doSelectAction = false;
    public bool isTimeRewinding = false;

    private int maxTurn = 0;

    // index of state list
    private int seq = 0;

    // task of a state ended, need to jump to next state (next index of the state list)
    //can be changed from state's DoneAction func, through sender
    public bool doneAction = false;

    public List<string> listCommandLog; //command log for time reverse(objects) and phantom action(copy commands)
    public List<(Vector3, Quaternion)> listPosLog; //position tracking log for time reverse(reset position)

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

    public GameObject playerStates;
    private PlayerIdle playerIdle;
    private PlayerMove playerMove;
    private PlayerTurn playerTurn;
    private PlayerHop playerHop;


    private Dictionary<PlayerState, IState<PlayerController>> dicState = new Dictionary<PlayerState, IState<PlayerController>>();

    //State List!!!
    private List<IState<PlayerController>> listCurTurn;
    private List<IState<PlayerController>> listStay;
    private List<IState<PlayerController>> listTurn;
    private List<IState<PlayerController>> listMoveForward;
    private List<IState<PlayerController>> listMoveSideRear;
    private List<IState<PlayerController>> listHopForward;
    private List<IState<PlayerController>> listHopSideRear;

    //don't need for Player, need for Phantom (currently I just wrote this on this .cs file sorry)
    //command order list for phantom action(copy commands)
    public List<string> listCommandOrderForPhantom;
    //for matching String(impormation of State List & HopDir & TurnAngle) -> State List
    private Dictionary<string, List<IState<PlayerController>>> dicCommand = new Dictionary<string, List<IState<PlayerController>>>();

    //the state machine of player.
    private StateMachine<PlayerController> sm;

    //Raycast for spatial condition check
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
        if (PlayerController.playerController == null) { PlayerController.playerController = this; }
    }
    void Start()
    {
        listCommandLog = new List<string>(); //command log for time reverse(objects) and phantom action(copy commands)
        listPosLog = new List<(Vector3, Quaternion)>(); //position tracking log for time reverse(reset position)

        animator = transform.Find("MainCharacter").GetComponent<Animator>();

        playerCurPos = this.transform.position;
        playerCurRot = this.transform.rotation;

        listPosLog.Add((playerCurPos, playerCurRot)); //position tracking log update!
        // always: listPosLog.Count = 1 + listCommandLog.Count ***

        moveSpeedHor = 6.0f;
        moveSpeedVer = 0.5f * moveSpeedHor;
        turnSpeed = 480.0f;

        playerIdle = playerStates.GetComponent<PlayerIdle>();
        playerMove = playerStates.GetComponent<PlayerMove>();
        playerTurn = playerStates.GetComponent<PlayerTurn>();
        playerHop = playerStates.GetComponent<PlayerHop>();

        //state names!
        IState<PlayerController> idle = playerIdle;
        IState<PlayerController> move = playerMove;
        IState<PlayerController> turn = playerTurn;
        IState<PlayerController> hop = playerHop;

        //dictionary. (not useful this time yet lol)
        dicState.Add(PlayerState.Idle, idle);
        dicState.Add(PlayerState.Move, move);
        dicState.Add(PlayerState.Turn, turn);
        dicState.Add(PlayerState.Hop, hop);

        //State List write!!
        //idle -> idle
        listStay = new List<IState<PlayerController>> { playerIdle };
        //idle -> turn -> idle
        listTurn = new List<IState<PlayerController>> { playerTurn };
        //idle -> move(forward.) -> idle
        listMoveForward = new List<IState<PlayerController>> { playerMove };
        //idle -> turn(x) -> move -> idle    (x=f(i)) (i:input, f: how much to rotate)
        listMoveSideRear = new List<IState<PlayerController>> { playerTurn, playerMove };
        //idle -> hop(over or under) -> idle
        listHopForward = new List<IState<PlayerController>> { playerHop };
        //idle -> turn(x) -> hop(over or under) -> idle    (x=f(i)) (i:input, f: how much to rotate)
        listHopSideRear = new List<IState<PlayerController>> { playerTurn, playerHop };

        //don't need for Player, need for Phantom (currently I just wrote this on this .cs file sorry)
        //command order list for phantom action(copy commands)
        listCommandOrderForPhantom = new List<string>();
        //w, s, a, d, r

        //StateMachine for Player, which we handle all the time.
        sm = new StateMachine<PlayerController>(this, dicState[PlayerState.Idle]);

        //Raycast for spatial condition check
        hitWall = new RaycastHit();
        hitFloor = new RaycastHit();
        hitOverFloor = new RaycastHit();
        hitUnderFloor = new RaycastHit();
        rayDistance = 1.0f;
        rayJumpInterval = 1.0f;
        layerMask = 1 << 0;

        InputManager.inputManager.OnInput += HandleInput; 
        InputManager.inputManager.OnTimeRewindInput += HandleTimeRewindInput;
        InputManager.inputManager.OnSpace += ToggleTimeRewind;
    }

    //Input&Condition Check <-> Action of this Turn(State Change) <-> Time Reverse ((not yet) <-> save-load)
    void Update() //it's good to think that Update function is an StateMachine too. (if-else grammer)
    {
        if (!TurnManager.turnManager.turnClock)
        {
            if (!isTimeRewinding)
            {
                if (doSelectAction)
                {
                    HandleActionBasedOnAngle();
                    doSelectAction = false;
                }
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

                    listPosLog.Add((playerCurPos, playerCurRot)); //position tracking log update!
                    TurnManager.turnManager.dicTurnCheck["Player"] = true;
                }
            }
        }

        //update always ~
        sm.DoOperateUpdate();
    }

    private void HandleInput(string command)
    {
        if (TurnManager.turnManager.turnClock) return;
        curKey = command;
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

            case "r":
                KeepIdleAndPassTurn();
                break;
        }
    }

    private void KeepIdleAndPassTurn()
    {
        listCurTurn = listStay;
        listCommandLog.Add(curKey);
        TurnManager.turnManager.turnClock = true; // start current turn!!!
        seq = 0;
        sm.SetState(listCurTurn[seq]);
    }

    private void HandleDirection(Vector3 direction, float[] angles, Vector3 rayOffset) //from HandleMovementInput() with local direction Array
    {
        int angleIndex = Mathf.RoundToInt(this.transform.eulerAngles.y / 90) % 4;
        curTurnAngle = angles[angleIndex]; //relative orientation!!

        Debug.DrawRay(playerCurPos + rayOffset, transform.up * -rayDistance, Color.red, 0.8f);
        if (Physics.Raycast(playerCurPos + rayOffset + new Vector3(0, 0.1f, 0), -transform.up, out hitUnderFloor, rayDistance + rayJumpInterval + 0.1f, layerMask)) //void check
        {
            isUnderJump = !Physics.Raycast(playerCurPos + rayOffset + new Vector3(0, 0.1f, 0), -transform.up, out hitFloor, rayDistance + 0.1f, layerMask); //can jump under?

            doSelectAction = true; //need to check more things. go to HandleActionBasedOnAngle()
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
    private void ChooseAction(List<IState<PlayerController>> statelist1, List<IState<PlayerController>> statelist2)
    {
        listCurTurn = curTurnAngle == 0.0f ? statelist1 : statelist2; //?  forward  :  side or rear
    }
    private void StartAction() //***** when it is executed, that means the global cycle starts.
    {
        listCommandLog.Add(curKey); //command log update
        TurnManager.turnManager.turnClock = true;
        seq = 0; //of player
        sm.SetState(listCurTurn[seq]); //of player
    }

    // Functions for Time Rewind
    private void ToggleTimeRewind()
    {
        if (!isTimeRewinding)
        {
            listCommandOrderForPhantom.Clear();
            maxTurn = TurnManager.turnManager.turn;
            isTimeRewinding = true;
        }
        else
        {
            if (TurnManager.turnManager.turn < maxTurn)
            {
                int startIndex = TurnManager.turnManager.turn;
                // Make list of Command Orders for phantom
                listCommandOrderForPhantom.AddRange(listCommandLog.GetRange(startIndex, listCommandLog.Count - startIndex));
                listCommandLog.RemoveRange(startIndex, listCommandLog.Count - startIndex); // turn 0: no element in listCommandLog
                listPosLog.RemoveRange(startIndex + 1, listPosLog.Count - startIndex - 1); // turn 0: one initial element in listPosLog
            }
            isTimeRewinding = false;
        }
    }

    private void HandleTimeRewindInput(string command)
    {
        if (!isTimeRewinding) return;
        switch (command)
        {
            case "q": // go to the Past (turn -1)
                if (TurnManager.turnManager.turn >= 1)
                {
                    GoToThePastOrFuture(-1);
                }
                else
                {
                    print("Cannot go further to the Past!!!");
                }
                break;

            case "e": // go to the Future (turn +1)
                if (TurnManager.turnManager.turn <= maxTurn - 1)
                {
                    GoToThePastOrFuture(1);
                }
                else
                {
                    print("Cannot go further to the Future!!!");
                }
                break;
        }
    }

    private void GoToThePastOrFuture(int turnDelta)
    {
        TurnManager.turnManager.turn += turnDelta;
        //position tracking log -> preview the current position(and rotation)
        this.transform.position = listPosLog[TurnManager.turnManager.turn].Item1;
        this.transform.rotation = listPosLog[TurnManager.turnManager.turn].Item2;
        playerCurPos = this.transform.position;
        playerCurRot = this.transform.rotation;
    }
}