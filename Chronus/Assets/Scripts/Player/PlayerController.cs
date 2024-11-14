using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController playerController; // singleton
    //public GameObject phantomPrefab;
    //public GameObject phantomInstance;
    //public PhantomController phantomScript;
    public bool isPhantomExists = false;

    public Animator animator; // animator.

    
    //Input
    private string curKey = "r"; //Command Log, it will be written on.
    public bool isTimeRewinding = false; //Time Rewinding Mode Toggle
    private int maxTurn = 0; //Time Rewinding Mode
    


    //Input & Spatial Condition -> rotate/hop Direction
    public float curHopDir = 1.0f;
    public float curTurnAngle = 0.0f;



    //Raycast for Spatial Condition Check
    private RaycastHit hitWall;
    private RaycastHit hitFloor;
    private RaycastHit hitOverFloor;
    private RaycastHit hitUnderFloor;
    private float rayDistance;
    private float rayJumpInterval;
    private int layerMask;

    //for state decision 'branch' from spatial condition check
    private bool isUnderJump = false;



    //Current transformation write.
    public Vector3 playerCurPos;
    public Quaternion playerCurRot;



    public List<string> listCommandLog; //command log for time reverse(objects) and phantom action(copy commands)
    public List<(Vector3, Quaternion)> listPosLog; //position tracking log for time reverse(reset position)



    //act speed.
    public float moveSpeedHor;
    public float moveSpeedVer;
    public float turnSpeed;
    //sender get & set. (speed)
    public float curSpeed { get; set; }
    public float curHopSpeed { get; set; }
    public float curRotSpeed { get; set; }



    // index of state list
    private int seq = 0;
    // task of a state ended, need to jump to next state (next index of the state list)
    //can be changed from state's DoneAction func, through sender
    public bool doneAction = false;



    //get state scripts. (empty object PlayerStates)
    public GameObject playerStates;
    //real state names!
    private PlayerIdle playerIdle; 
    private PlayerMove playerMove;
    private PlayerTurn playerTurn;
    private PlayerHop playerHop;
    private IState<PlayerController> idle;
    private IState<PlayerController> move;
    private IState<PlayerController> turn;
    private IState<PlayerController> hop;

    //State List!!!
    private List<IState<PlayerController>> listCurTurn;
    private List<IState<PlayerController>> listStay;
    private List<IState<PlayerController>> listTurn;
    private List<IState<PlayerController>> listMoveForward;
    private List<IState<PlayerController>> listMoveSideRear;
    private List<IState<PlayerController>> listHopForward;
    private List<IState<PlayerController>> listHopSideRear;

    //the state machine of player.
    public StateMachine<PlayerController> sm;



    private void Awake() //Singleton
    {
        if (PlayerController.playerController == null) { PlayerController.playerController = this; }
    }
    


    void Start()
    {
        animator = transform.Find("MainCharacter").GetComponent<Animator>(); //rabbit animator!



        //Input -> Condition Check or Time Rewinding Mode
        InputManager.inputManager.OnCommand += HandleMovementInput;
        InputManager.inputManager.OnTimeRewindModeToggle += ToggleTimeRewindMode;
        InputManager.inputManager.OnTimeRewindControl += HandleTimeRewindInput;



        //Raycast for spatial condition check
        hitWall = new RaycastHit();
        hitFloor = new RaycastHit();
        hitOverFloor = new RaycastHit();
        hitUnderFloor = new RaycastHit();
        rayDistance = 1.0f;
        rayJumpInterval = 1.0f;
        layerMask = 1 << 0;



        playerCurPos = this.transform.position;
        playerCurRot = this.transform.rotation;



        listCommandLog = new List<string>(); //command log for time reverse(objects) and phantom action(copy commands)
        listPosLog = new List<(Vector3, Quaternion)> { (playerCurPos, playerCurRot) }; //position tracking log for time reverse and reset position
        // always: listPosLog.Count = 1 + listCommandLog.Count ***
        


        moveSpeedHor = 6.0f;
        moveSpeedVer = 0.5f * moveSpeedHor;
        turnSpeed = 480.0f;


        playerStates = GameObject.Find("PlayerStates");
        //get state scripts. (empty object PlayerStates)
        playerIdle = playerStates.GetComponent<PlayerIdle>();
        playerMove = playerStates.GetComponent<PlayerMove>();
        playerTurn = playerStates.GetComponent<PlayerTurn>();
        playerHop = playerStates.GetComponent<PlayerHop>();
        idle = playerIdle;
        move = playerMove;
        turn = playerTurn;
        hop = playerHop;

        //State List write!!
        //idle -> idle
        listStay = new List<IState<PlayerController>> { idle };
        //idle -> turn -> idle
        listTurn = new List<IState<PlayerController>> { turn, idle };
        //idle -> move(forward.) -> idle
        listMoveForward = new List<IState<PlayerController>> { move };
        //idle -> turn(x) -> move -> idle    (x=f(i)) (i:input, f: how much to rotate)
        listMoveSideRear = new List<IState<PlayerController>> { turn, move };
        //idle -> hop(over or under) -> idle
        listHopForward = new List<IState<PlayerController>> { hop };
        //idle -> turn(x) -> hop(over or under) -> idle    (x=f(i)) (i:input, f: how much to rotate)
        listHopSideRear = new List<IState<PlayerController>> { turn, hop };

        //StateMachine for Player, which we handle all the time.
        sm = new StateMachine<PlayerController>(this, idle);
    }

    //it's good to think that Update functions are together an StateMachine too. (if-else grammer)

    // [Update() of InputManager.cs]: Input&Condition Check
    // <-> [Update() of PlayerController.cs]: Action of this Turn(State Change)
    // <-> [Update() of InputManager.cs]: Time Rewind
    void Update() //Action of this Turn(State Change)
    {
        if (TurnManager.turnManager.turnClock) //when turn clock in ON.
        {
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
                    sm.SetState(idle);
                    doneAction = false;
                    listPosLog.Add((playerCurPos, playerCurRot)); //position tracking log update!

                    TurnManager.turnManager.dicTurnCheck["Player"] = true;
                }
            }
        }

        //update always ~
        sm.DoOperateUpdate();
    }





    // Functions for Spatial Condition Check //
    private void HandleMovementInput(string command) //get absolute orientation from Command Input (or stand still)
    {
        if (TurnManager.turnManager.CLOCK || isTimeRewinding) return; //***** active when Not executing Actions and Not Time Rewinding Mode.

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
        StartAction();
    }

    private void HandleDirection(Vector3 direction, float[] angles, Vector3 rayOffset) //from HandleMovementInput() with local direction Array
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
        if (Physics.Raycast(playerCurPos, rayDirection, out hitWall, rayDistance + 1.0f, layerMask)) //wall check
        {
            if (hitWall.collider.tag == "Lever") //lever.
            {
                if (hitWall.collider.gameObject.GetComponent<LeverSwitch>().canToggleDirection == rayDirection) //can push lever (right direction)
                {
                    hitWall.collider.gameObject.GetComponent<LeverSwitch>().ToggleLever(); //push lever!!!
                    ChooseAction(listStay, listTurn); //we should set animation trigger on Enter and Exit of PlayerIdle state.
                    StartAction(); //cycle starts!!!
                }
                else
                {
                    if (curTurnAngle != 0.0f)
                    {
                        listCurTurn = listTurn; //we should set animation trigger on Enter and Exit of PlayerTurn state.
                        StartAction(); //cycle starts!!!
                    }
                }
            }
            else if (hitWall.collider.tag == "Box") //box.
            {
                PlayerPush.playerPush.TryPush(rayDirection); //push box!!!
                if (PlayerPush.playerPush.canPushBox)
                {
                    ChooseAction(listMoveForward, listMoveSideRear); //we should use new state (like playerpush ..) and animation trigger on Enter and Exit of that state.
                    StartAction(); //cycle starts!!!
                }
                else
                {
                    if (!Physics.Raycast(playerCurPos + new Vector3(0, 0.5f, 0), rayDirection, out hitOverFloor, rayDistance + 1.0f, layerMask)) //available to jump over
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
            }
            else if (!Physics.Raycast(playerCurPos + new Vector3(0, 0.5f, 0), rayDirection, out hitOverFloor, rayDistance + 1.0f, layerMask)) //available to jump over
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

    private void StartAction() //********** when it is executed, that means the global cycle starts.
    {
        listCommandLog.Add(curKey); //command log update
        seq = 0; //of player
        sm.SetState(listCurTurn[seq]); //of player
        //START CLOCK
        TurnManager.turnManager.CLOCK = true;
    }



    // Functions for Time Rewind //
    private void ToggleTimeRewindMode()
    {
        if (TurnManager.turnManager.CLOCK) return; //***** active when Not executing Actions.

        if (!isTimeRewinding) //when OFF
        {
            PhantomController.phantomController.listCommandOrder.Clear();
            PhantomController.phantomController.gameObject.SetActive(false);
            PhantomController.phantomController.transform.position = this.listPosLog[0].Item1 + new Vector3(0, -4, 0);
            PhantomController.phantomController.transform.rotation = this.listPosLog[0].Item2;
            PhantomController.phantomController.playerCurPos = PhantomController.phantomController.transform.position;
            PhantomController.phantomController.playerCurRot = PhantomController.phantomController.transform.rotation;
            //Destroy(phantomInstance);
            isPhantomExists = false;
            maxTurn = TurnManager.turnManager.turn;
            isTimeRewinding = true; //toggle ON
        }
        else //when ON
        {
            if (TurnManager.turnManager.turn < maxTurn)
            {
                int startIndex = TurnManager.turnManager.turn;
                PhantomController.phantomController.transform.position = this.listPosLog[startIndex].Item1;
                PhantomController.phantomController.transform.rotation = this.listPosLog[startIndex].Item2;
                PhantomController.phantomController.playerCurPos = PhantomController.phantomController.transform.position;
                PhantomController.phantomController.playerCurRot = PhantomController.phantomController.transform.rotation;
                PhantomController.phantomController.gameObject.SetActive(true);
                //phantomInstance = Instantiate(phantomPrefab, listPosLog[startIndex].Item1, listPosLog[startIndex].Item2);
                //phantomScript = phantomInstance.GetComponent<PhantomController>();
                isPhantomExists = true;
                // Make Copy list of Command Orders for phantom
                PhantomController.phantomController.listCommandOrder.AddRange(listCommandLog.GetRange(startIndex, listCommandLog.Count - startIndex)); //copy!!!
                PhantomController.phantomController.order = 0;
                //phantomScript.listCommandOrder.AddRange(listCommandLog.GetRange(startIndex, listCommandLog.Count - startIndex)); //copy!!!
                listCommandLog.RemoveRange(startIndex, listCommandLog.Count - startIndex); // turn 0: no element in listCommandLog
                listPosLog.RemoveRange(startIndex + 1, listPosLog.Count - startIndex - 1); // turn 0: one initial element in listPosLog
                
            }
            isTimeRewinding = false; //toggle OFF
        }
    }



    private void HandleTimeRewindInput(string command)
    {
        if (TurnManager.turnManager.CLOCK || !isTimeRewinding) return; //***** active when Time Rewinding Mode.
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
        //print(TurnManager.turnManager.turn);
        //position tracking log -> preview the current position(and rotation)
        this.transform.position = listPosLog[TurnManager.turnManager.turn].Item1;
        this.transform.rotation = listPosLog[TurnManager.turnManager.turn].Item2;
        playerCurPos = this.transform.position;
        playerCurRot = this.transform.rotation;
    }
}