using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhantomController : MonoBehaviour
{
    public Transform startPoint;

    public static PhantomController phantomController; // singleton

    public Animator animator; // animator.



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



    public List<string> listCommandOrder; //command order list for phantom input -> condition check -> action (Copy Commands)
    private int order = 0;



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
    public GameObject phantomStates;
    //real state names!
    private PhantomIdle phantomIdle;
    private PhantomMove phantomMove;
    private PhantomTurn phantomTurn;
    private PhantomHop phantomHop;
    private IState<PhantomController> idle;
    private IState<PhantomController> move;
    private IState<PhantomController> turn;
    private IState<PhantomController> hop;

    //State List!!!
    private List<IState<PhantomController>> listCurTurn;
    private List<IState<PhantomController>> listStay;
    private List<IState<PhantomController>> listTurn;
    private List<IState<PhantomController>> listMoveForward;
    private List<IState<PhantomController>> listMoveSideRear;
    private List<IState<PhantomController>> listHopForward;
    private List<IState<PhantomController>> listHopSideRear;

    //the state machine of player.
    public StateMachine<PhantomController> sm;



    private void Awake() //Singleton
    {
        if (PhantomController.phantomController == null) { PhantomController.phantomController = this; }
    }



    void Start()
    {
        animator = transform.Find("PhantomCharacter").GetComponent<Animator>(); //rabbit animator!



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



        listCommandOrder = new List<string>();
        //w, s, a, d, r



        moveSpeedHor = 6.0f;
        moveSpeedVer = 0.5f * moveSpeedHor;
        turnSpeed = 480.0f;



        //get state scripts. (empty object PlayerStates)
        phantomIdle = phantomStates.GetComponent<PhantomIdle>();
        phantomMove = phantomStates.GetComponent<PhantomMove>();
        phantomTurn = phantomStates.GetComponent<PhantomTurn>();
        phantomHop = phantomStates.GetComponent<PhantomHop>();
        idle = phantomIdle;
        move = phantomMove;
        turn = phantomTurn;
        hop = phantomHop;

        //State List write!!
        //idle -> idle
        listStay = new List<IState<PhantomController>> { idle };
        //idle -> turn -> idle
        listTurn = new List<IState<PhantomController>> { turn };
        //idle -> move(forward.) -> idle
        listMoveForward = new List<IState<PhantomController>> { move };
        //idle -> turn(x) -> move -> idle    (x=f(i)) (i:input, f: how much to rotate)
        listMoveSideRear = new List<IState<PhantomController>> { turn, move };
        //idle -> hop(over or under) -> idle
        listHopForward = new List<IState<PhantomController>> { hop };
        //idle -> turn(x) -> hop(over or under) -> idle    (x=f(i)) (i:input, f: how much to rotate)
        listHopSideRear = new List<IState<PhantomController>> { turn, hop };

        //StateMachine for Player, which we handle all the time.
        sm = new StateMachine<PhantomController>(this, idle);
    }

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

                    order++;
                    TurnManager.turnManager.dicTurnCheck["Phantom"] = true;
                }
            }
        }

        //update always ~
        sm.DoOperateUpdate();
    }





    // Functions for Spatial Condition Check //
    private void HandleMovementInput(string command) //get absolute orientation from Command Input (or stand still)
    {
        if (TurnManager.turnManager.turnClock) return; //***** active when Not executing Actions and Not Time Rewinding Mode.

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

    private void ChooseAction(List<IState<PhantomController>> statelist1, List<IState<PhantomController>> statelist2)
    {
        listCurTurn = curTurnAngle == 0.0f ? statelist1 : statelist2; //?  forward  :  side or rear
    }

    private void StartAction() //********** special function for phantom.
    {
        seq = 0; //of phantom
        sm.SetState(listCurTurn[seq]); //of phantom
    }
}
