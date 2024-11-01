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

    // ������ ���� ������Ʈ�� �ϰ������� �����ؾ� �ؼ� ���⿡ ������ �� �Ǵ� ��������
    // �ϴ� �÷��̾���� ���۽��Ѻ��� �ϴ� �ӽ÷� ����ٰ� �ΰڽ��ϴ�.
    public bool turnClock = false;
    //when turnClock false -> true  state��ȭ�� ���� �Ǵ� �� ������ ������.

    // turnClock�� ���� �ÿ� list�� ����ִ� ������� �������� setState�� �ϱ� ���� 1�� �����ϴ� ��
    private int seq = 0;
    // �� state������ ������ �������� Ȯ���ϱ� ���� ����. (������ �ش� ������ true�� ��)
    public bool doneAction = false;


    //�� ������ ������ ��ġ,ȸ������ ����ϴ� ����
    public Vector3 playerCurPos;
    public Quaternion playerCurRot;
    public float curTurnAngle;

    public float moveSpeedHor = 6.0f;
    public float moveSpeedVer = 3.0f;
    public float turnSpeed = 60.0f;
    public float curSpeed { get; set; }
    public float curRotSpeed { get; set; }


    private Dictionary<PlayerState, IState<PlayerController>> dicState = new Dictionary<PlayerState, IState<PlayerController>>();
    
    //�� �Ͽ��� ������ state���� ������� ���� list�� ���� �׸�.
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

        //���� IState ���. �̰� SetState�� Argument�� ������ state�� �ش� IState�� �ٲ��.
        IState<PlayerController> idle = new PlayerIdle();
        IState<PlayerController> move = new PlayerMove();
        IState<PlayerController> turn = new PlayerTurn();
        IState<PlayerController> hop = new PlayerHop();

        //���� PlayerState �̸����� �˻��ؼ� ������ IState�� �ҷ������� Dictionary�� �������.
        dicState.Add(PlayerState.Idle, idle);
        dicState.Add(PlayerState.Move, move);
        dicState.Add(PlayerState.Turn, turn);
        dicState.Add(PlayerState.Hop, hop);

        //�� �Ͽ��� ������ �� �ִ� ���鿡 ���� ���� List�� ����� �ൿ����(State ��ȭ ����)�� ����ߴ�.

        //������ ���� �� �ѱ��: idle -> idle
        listStay.Add(idle);
        //������ �� ĭ �̵��ϱ�: idle -> move(forward�����̴�.) -> idle
        listMoveForward.Add(move);
        //���̳� �ڷ� �� ĭ �̵��ϱ�: idle -> turn(x) -> move -> idle  (x=f(i)) (i:input, f: ����������ε���)
        listMoveSideRear.Add(turn);
        listMoveSideRear.Add(move);
        //������ �� ĭ �����ؼ� �ö󰡱�: idle -> hop(+) -> move -> idle
        listHopOverForward.Add(hop);
        listHopOverForward.Add(move);
        //���̳� �ڷ� �� ĭ �����ؼ� �ö󰡱�: idle -> turn(x) -> hop(+) -> move -> idle
        listHopOverSideRear.Add(turn);
        listHopOverSideRear.Add(hop);
        listHopOverSideRear.Add(move);
        //������ �� ĭ �����ؼ� ��������: idle -> move -> hop(-) -> idle
        listHopOverForward.Add(move);
        listHopOverForward.Add(hop);
        //���̳� �ڷ� �� ĭ �����ؼ� ��������: idle -> turn(x) -> move -> hop(-) -> idle
        listHopOverSideRear.Add(turn);
        listHopOverSideRear.Add(move);
        listHopOverSideRear.Add(hop);

        //StateMachine class�� Object�� ������! //�⺻ State�� Idle.
        sm = new StateMachine<PlayerController>(this, dicState[PlayerState.Idle]);
    }

    void Update()
    {
        //���⼭ ��� list�� �ҷ��;� �ϴ��� �Ǵ��ϴ� logic �ۼ��ؾ� ��
        if (!turnClock) {
            if (Input.GetKeyDown(KeyCode.W)) //�ϴ��� ������� ��������...
            {
                //�̹� �Ͽ��� ������ ���۵��� �����Ѵ�.
                playerCurPos = this.transform.position;
                listCurTurn = listMoveForward;
                turnClock = true;
                seq = 0; //����� �ѱ�?
                sm.SetState(listCurTurn[seq]);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                //�̹� �Ͽ��� ������ ���۵��� �����Ѵ�.
                curTurnAngle = 180.0f;
                playerCurRot = this.transform.rotation;
                listCurTurn = listMoveSideRear;
                turnClock = true;
                seq = 0; //����� �ѱ�?
                sm.SetState(listCurTurn[seq]);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                //�̹� �Ͽ��� ������ ���۵��� �����Ѵ�.
                curTurnAngle = -90.0f;
                playerCurRot = this.transform.rotation;
                listCurTurn = listMoveSideRear;
                turnClock = true;
                seq = 0; //����� �ѱ�?
                sm.SetState(listCurTurn[seq]);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                //�̹� �Ͽ��� ������ ���۵��� �����Ѵ�.
                curTurnAngle = 90.0f;
                playerCurRot = this.transform.rotation;
                listCurTurn = listMoveSideRear;
                turnClock = true;
                seq = 0; //����� �ѱ�?
                sm.SetState(listCurTurn[seq]);
            }
        }
        if (turnClock)
        {
            // ������ �������Ǿ������� ���θ� �Ǵ��ϴ� logic
            sm.IsDoneAction();
            // ������ �������Ǿ����� doneAction: true.
            if (doneAction)
            {
                //�켱 seq�� 1 ������Ű��
                seq++;
                if (seq < listCurTurn.Count) //���� list���� ���� ���� state�� setState
                {
                    sm.SetState(listCurTurn[seq]);
                    doneAction = false;
                }
                else //list�� ������ �о��ٸ� Idle state�� ���ƿ��� turnClock ����.
                {
                    sm.SetState(dicState[PlayerState.Idle]);
                    doneAction = false;
                    turnClock = false;
                    //seq = 0; //�ƴϸ� ����� �ѱ�?
                }
            }
        }

        //update always 
        sm.DoOperateUpdate();
    }

}
