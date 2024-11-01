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

    // 원래는 여러 오브젝트들 일괄적으로 관리해야 해서 여기에 있으면 안 되는 변수지만
    // 일단 플레이어부터 동작시켜봐야 하니 임시로 여기다가 두겠습니다.
    public bool turnClock = false;
    //when turnClock false -> true  state변화에 대한 판단 및 실행을 진행함.

    // turnClock이 켜질 시에 list에 들어있는 순서대로 차례차례 setState를 하기 위해 1씩 증가하는 값
    private int seq = 0;
    // 각 state에서의 동작이 끝났는지 확인하기 위한 변수. (끝나면 해당 변수가 true가 됨)
    public bool doneAction = false;


    //턴 단위로 현재의 위치,회전정보 기록하는 변수
    public Vector3 playerCurPos;
    public Quaternion playerCurRot;

    public float curTurnDir = 1.0f;
    public float curTurnAngle = 90.0f;

    public float moveSpeedHor = 6.0f;
    public float moveSpeedVer = 3.0f;
    public float turnSpeed = 6.0f;
    public float curSpeed { get; set; }
    public float curRotSpeed { get; set; }


    private Dictionary<PlayerState, IState<PlayerController>> dicState = new Dictionary<PlayerState, IState<PlayerController>>();
    
    //각 턴에서 동작할 state들을 순서대로 모은 list를 담을 그릇.
    private List<IState<PlayerController>> listCurTurn = new List<IState<PlayerController>>();

    private List<IState<PlayerController>> listStay = new List<IState<PlayerController>>();
    private List<IState<PlayerController>> listMoveForward = new List<IState<PlayerController>>();
    private List<IState<PlayerController>> listMoveSideRear = new List<IState<PlayerController>>();
    private List<IState<PlayerController>> listHopOverForward = new List<IState<PlayerController>>();
    private List<IState<PlayerController>> listHopOverSideRear = new List<IState<PlayerController>>();
    private List<IState<PlayerController>> listHopUnderForward = new List<IState<PlayerController>>();
    private List<IState<PlayerController>> listHopUnderSideRear = new List<IState<PlayerController>>();
    private StateMachine<PlayerController> sm;

    // Start is called before the first frame update
    void Start()
    {
        playerCurPos = this.transform.position;

        //실제 IState 목록. 이걸 SetState의 Argument로 넣으면 state가 해당 IState로 바뀐다.
        IState<PlayerController> idle = new PlayerIdle();
        IState<PlayerController> move = new PlayerMove();
        IState<PlayerController> turn = new PlayerTurn();
        IState<PlayerController> hop = new PlayerHop();

        //위의 PlayerState 이름으로 검색해서 각각의 IState를 불러오도록 Dictionary를 만들었다.
        dicState.Add(PlayerState.Idle, idle);
        dicState.Add(PlayerState.Move, move);
        dicState.Add(PlayerState.Turn, turn);
        dicState.Add(PlayerState.Hop, hop);

        //한 턴에서 동작할 수 있는 경우들에 대해 각각 List를 만들고 행동순서(State 변화 순서)를 기록했다.

        //가만히 서서 턴 넘기기: idle -> idle
        listStay.Add(idle);
        //앞으로 한 칸 이동하기: idle -> move(forward방향이다.) -> idle
        listMoveForward.Add(move);
        //옆이나 뒤로 한 칸 이동하기: idle -> turn(x) -> move -> idle  (x=f(i)) (i:input, f: 어느방향으로돌지)
        listMoveSideRear.Add(turn);
        listMoveSideRear.Add(move);
        //앞으로 한 칸 점프해서 올라가기: idle -> hop(+) -> move -> idle
        listHopOverForward.Add(hop);
        listHopOverForward.Add(move);
        //옆이나 뒤로 한 칸 점프해서 올라가기: idle -> turn(x) -> hop(+) -> move -> idle
        listHopOverSideRear.Add(turn);
        listHopOverSideRear.Add(hop);
        listHopOverSideRear.Add(move);
        //앞으로 한 칸 점프해서 내려가기: idle -> move -> hop(-) -> idle
        listHopOverForward.Add(move);
        listHopOverForward.Add(hop);
        //옆이나 뒤로 한 칸 점프해서 내려가기: idle -> turn(x) -> move -> hop(-) -> idle
        listHopOverSideRear.Add(turn);
        listHopOverSideRear.Add(move);
        listHopOverSideRear.Add(hop);

        //StateMachine class의 Object를 생성함! //기본 State는 Idle.
        sm = new StateMachine<PlayerController>(this, dicState[PlayerState.Idle]);
    }

    void Update()
    {
        //여기서 어떠한 list를 불러와야 하는지 판단하는 logic 작성해야 함
        if (!turnClock) {
            if (Input.GetKeyDown(KeyCode.W)) //일단은 상대적인 방향으로...
            {
                //이번 턴에서 수행할 동작들을 지정한다.
                playerCurPos = this.transform.position;
                listCurTurn = listMoveForward;
                turnClock = true;
                seq = 0; //여기다 둘까?
                sm.SetState(listCurTurn[seq]);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                //이번 턴에서 수행할 동작들을 지정한다.
                curTurnDir = 1.0f;
                curTurnAngle = 180.0f;
                playerCurRot = this.transform.rotation;
                listCurTurn = listMoveSideRear;
                turnClock = true;
                seq = 0; //여기다 둘까?
                sm.SetState(listCurTurn[seq]);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                //이번 턴에서 수행할 동작들을 지정한다.
                curTurnDir = -1.0f;
                curTurnAngle = -90.0f;
                playerCurRot = this.transform.rotation;
                listCurTurn = listMoveSideRear;
                turnClock = true;
                seq = 0; //여기다 둘까?
                sm.SetState(listCurTurn[seq]);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                //이번 턴에서 수행할 동작들을 지정한다.
                curTurnDir = 1.0f;
                curTurnAngle = 90.0f;
                playerCurRot = this.transform.rotation;
                listCurTurn = listMoveSideRear;
                turnClock = true;
                seq = 0; //여기다 둘까?
                sm.SetState(listCurTurn[seq]);
            }
        }
        if (turnClock)
        {
            // 동작이 마무리되었는지의 여부를 판단하는 logic
            sm.IsDoneAction();
            // 동작이 마무리되었으면 doneAction: true.
            if (doneAction)
            {
                //우선 seq를 1 증가시키고
                seq++;
                if (seq < listCurTurn.Count) //아직 list범위 내면 다음 state로 setState
                {
                    sm.SetState(listCurTurn[seq]);
                    doneAction = false;
                }
                else //list를 끝까지 읽었다면 Idle state로 돌아오고 turnClock 끈다.
                {
                    sm.SetState(dicState[PlayerState.Idle]);
                    doneAction = false;
                    turnClock = false;
                    //seq = 0; //아니면 여기다 둘까?
                }
            }
        }

        //update always 
        sm.DoOperateUpdate();
    }

}
