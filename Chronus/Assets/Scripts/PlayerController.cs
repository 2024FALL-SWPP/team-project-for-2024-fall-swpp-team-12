using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController playerController; //public accessibility using Singleton

    public enum PlayerState //just naming for matching key of state dictionary(dicState), not that important.
    {
        Idle,
        Move,
        Turn,
        Hop
    } //honestly it seems better just to use string type lol (but I'm too lazy to change this)
    //(and you are not lazy for writing this long useless comments lmao)

    public Animator animator;
    //2 separated parts of 'input and spatial condition check' this bool variable for switching between two sides.
    private bool doSelectAction = false;

    //when turnClock false -> true:  state list reading (DoneAction <-> next state ...)
    public bool turnClock = false;
    //when player action ended -> turn check for player(true) -> when all objects true: turn count +1
    //-> turnClock true -> false: can detect input
    //object-player interaction rules:
    //turnClock = true -> objects also do their own action for this turn.
    //turnClock = false -> do condition check (collide, in the area, etc)
    //-> need condition check Sequence, Before player input check, After turn update

    //that means: Player have Initiative Role of turn update(action execution) == player is the manager.



    public bool isTimeReversing = false;

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
    private IState<PlayerController> idle;
    private IState<PlayerController> move;
    private IState<PlayerController> turn;
    private IState<PlayerController> hop;


    private Dictionary<PlayerState, IState<PlayerController>> dicState = new Dictionary<PlayerState, IState<PlayerController>>();
    
    //State List!!!
    private List<IState<PlayerController>> listCurTurn = new List<IState<PlayerController>>();

    private List<IState<PlayerController>> listStay = new List<IState<PlayerController>>();
    private List<IState<PlayerController>> listMoveForward = new List<IState<PlayerController>>();
    private List<IState<PlayerController>> listMoveSideRear = new List<IState<PlayerController>>();
    private List<IState<PlayerController>> listHopForward = new List<IState<PlayerController>>();
    private List<IState<PlayerController>> listHopSideRear = new List<IState<PlayerController>>();

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
        if (PlayerController.playerController == null)
        {
            PlayerController.playerController = this;
        }
    }

    // Start is called before the first frame update
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
        IState <PlayerController> idle = playerIdle;
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

        //don't need for Player, need for Phantom (currently I just wrote this on this .cs file sorry)
        //command order list for phantom action(copy commands)
        listCommandOrderForPhantom = new List<string>();
        //for matching String(impormation of State List & HopDir & TurnAngle) -> State List
        dicCommand.Add("Stay", listStay);
        dicCommand.Add("MoveForward", listMoveForward);
        dicCommand.Add("MoveRear", listMoveSideRear);
        dicCommand.Add("MoveLeft", listMoveSideRear);
        dicCommand.Add("MoveRight", listMoveSideRear);
        dicCommand.Add("HopOverForward", listHopForward);
        dicCommand.Add("HopUnderForward", listHopForward);
        dicCommand.Add("HopOverRear", listHopSideRear);
        dicCommand.Add("HopOverLeft", listHopSideRear);
        dicCommand.Add("HopOverRight", listHopSideRear);
        dicCommand.Add("HopUnderRear", listHopSideRear);
        dicCommand.Add("HopUnderLeft", listHopSideRear);
        dicCommand.Add("HopUnderRight", listHopSideRear);
        //Over: curHopDir = 1
        //Under: curHopDir = -1
        //Left: curTurnAngle; = -90.0f
        //Right: curTurnAngle; = 90.0f
        //Rear: curTurnAngle; = 180.0f
        //required logic above.



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
    }

    //Input&Condition Check <-> Action of this Turn(State Change) <-> Time Reverse ((not yet) <-> save-load)
    void Update() //it's good to think that Update function is an StateMachine too. (if-else grammer)
    {
        if (!turnClock) {
            if (!isTimeReversing)
            {   // from Absolute orientation (wsad) -> get Relative orientation (curTurnAngle)
                HandleInput();
                // select action which is towarding the orientation (curTurnAngle: Relative orientation)
                if (doSelectAction)
                {
                    HandleActionBasedOnAngle();
                    doSelectAction = false;
                }
            }
            else
            {
                UpdateTimeReversingMode();
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



    private void HandleInput() //global direction and check whether 'void or floor(plain or below 1.0)' is in front of player
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
        else if (Input.GetKeyDown(KeyCode.R)) //Stay and Other objects act. turn +1
        {
            KeepIdleAndPassTurn();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            ActivateTimeReversingMode();
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

    private void ActivateTimeReversingMode()
    {
        listCommandOrderForPhantom.Clear();
        maxTurn = TurnManager.turnManager.turn;
        isTimeReversing = true;
    }

    private void HandleDirection(Vector3 direction, float[] angles, Vector3 rayOffset) //from HandleMovementInput() with local direction Array
    {
        Debug.DrawRay(playerCurPos + rayOffset, transform.up * -rayDistance, Color.red, 0.8f);
        if (Physics.Raycast(playerCurPos + rayOffset + new Vector3(0, 0.1f, 0), -transform.up, out hitUnderFloor, rayDistance + rayJumpInterval + 0.1f, layerMask)) //void check
        {
            isUnderJump = !Physics.Raycast(playerCurPos + rayOffset + new Vector3(0, 0.1f, 0), -transform.up, out hitFloor, rayDistance + 0.1f, layerMask); //can jump under?

            int angleIndex = Mathf.RoundToInt(this.transform.eulerAngles.y / 90) % 4;
            curTurnAngle = angles[angleIndex]; //orientation!!
            
            doSelectAction = true; //need to check more things. go to HandleActionBasedOnAngle()
        }
        //if void, don't do action (no turn update)
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
            if (!Physics.Raycast(playerCurPos + new Vector3(0, 0.5f, 0), rayDirection, out hitOverFloor, rayDistance, layerMask)) //whether it is available to jump over or not
            {
                curHopDir = 1.0f;
                ChooseCommand(listHopForward, listHopSideRear, "HopOverForward", "HopOverRear", "HopOverLeft", "HopOverRight"); //Jump Over
                StartAction();
            }
            //if not, don't do action (no turn update)
        }
        else
        {
            if (isUnderJump)
            {
                curHopDir = -1.0f;
                ChooseCommand(listHopForward, listHopSideRear, "HopUnderForward", "HopUnderRear", "HopUnderLeft", "HopUnderRight"); //Jump Under
                isUnderJump = false;
            }
            else
            {
                ChooseCommand(listMoveForward, listMoveSideRear, "MoveForward", "MoveRear", "MoveLeft", "MoveRight"); //Horizontal Move
            }
            StartAction();
        }
    }
    private void ChooseCommand(List<IState<PlayerController>> statelist1, List<IState<PlayerController>> statelist2, string command1, string command2, string command3, string command4)
    {
        if (curTurnAngle == 0.0f)
        {
            listCurTurn = statelist1; //forward
            listCommandLog.Add(command1); //command log update
        }
        else
        {
            listCurTurn = statelist2; //side or rear
            if (curTurnAngle == 180.0f) //command log update
                listCommandLog.Add(command2);
            else if (curTurnAngle == -90.0f)
                listCommandLog.Add(command3);
            else if (curTurnAngle == 90.0f)
                listCommandLog.Add(command4);
        }
    }
    private void StartAction() //global effect.
    {
        turnClock = true; // all objects, including player, will start action of current turn!!!
        seq = 0; //of player
        sm.SetState(listCurTurn[seq]); //of player
    }



    private void UpdateTimeReversingMode()
    {
        if (Input.GetKeyDown(KeyCode.Q)) //go to the Past (turn -1)
        {
            if (TurnManager.turnManager.turn >= 1)
            {
                GoToThePastOrFuture(-1);
            }
            else
            {
                print("Cannot go further to the Past!!!");
            }
        }
        else if (Input.GetKeyDown(KeyCode.E)) //go to the Future (turn +1)
        {
            if (TurnManager.turnManager.turn <= maxTurn - 1)
            {
                GoToThePastOrFuture(1);
            }
            else
            {
                print("Cannot go further to the Future!!!");
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (TurnManager.turnManager.turn < maxTurn)
            {
                //make list of Command Orders for phantom!!
                for (int index = TurnManager.turnManager.turn; index <= listCommandLog.Count - 1; index++)
                {
                    listCommandOrderForPhantom.Add(listCommandLog[index]);
                }
                for (int index = listCommandLog.Count - 1; index >= TurnManager.turnManager.turn; index--)
                {
                    listCommandLog.RemoveAt(index); //turn 0 : no element in listCommandLog
                }
                for (int index = listPosLog.Count - 1; index >= TurnManager.turnManager.turn + 1; index--)
                {
                    listPosLog.RemoveAt(index); //turn 0 : one initial element in listPosLog
                }
            }
            isTimeReversing = false;
        }
    }

    private void GoToThePastOrFuture(int deltaturn)
    {
        TurnManager.turnManager.turn += deltaturn;
        print(TurnManager.turnManager.turn);
        //position tracking log -> preview the current position(and rotation)
        this.transform.position = listPosLog[TurnManager.turnManager.turn].Item1;
        this.transform.rotation = listPosLog[TurnManager.turnManager.turn].Item2;
        playerCurPos = this.transform.position;
        playerCurRot = this.transform.rotation;
    }
}
