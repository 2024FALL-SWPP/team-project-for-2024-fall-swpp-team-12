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
    // �ϴ� �÷��̾� ����� ���۽��Ѻ��� �ϴ� �ӽ÷� ����ٰ� �ΰڽ��ϴ�.
    public bool turnClock = false;
    //when turnClock false -> true  state ��ȭ



    public float moveSpeedHor = 0.2f;
    public float moveSpeedVer = 0.1f;
    public float turnDistance = 0.2f;

    public float curSpeed { get; set; }
    public Direction curTurnDir { get; private set; }
    public enum Direction
    {
        Left = -1,
        Right = 1
    }


    private Dictionary<PlayerState, IState<PlayerController>> dicState = new Dictionary<PlayerState, IState<PlayerController>>();
    private StateMachine<PlayerController> sm;

    // Start is called before the first frame update
    void Start()
    {
        IState<PlayerController> idle = new PlayerIdle();
        IState<PlayerController> move = new PlayerMove();
        IState<PlayerController> turn = new PlayerTurn();
        IState<PlayerController> hop = new PlayerHop();

        dicState.Add(PlayerState.Idle, idle);
        dicState.Add(PlayerState.Move, move);
        dicState.Add(PlayerState.Turn, turn);
        dicState.Add(PlayerState.Hop, hop);

        //StateMachine class�� Object�� ������!
        sm = new StateMachine<PlayerController>(this, dicState[PlayerState.Idle]);
    }

    void Update()
    {
        //���⼭ State�� ��� ��ȭ��ų ���� ���� logic �ۼ��ؾ� ��
        if (Input.GetKeyDown(KeyCode.W))
        {
            sm.SetState(dicState[PlayerState.Move]);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            curTurnDir = Direction.Right;
            sm.SetState(dicState[PlayerState.Turn]);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            curTurnDir = Direction.Left;
            sm.SetState(dicState[PlayerState.Turn]);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            sm.SetState(dicState[PlayerState.Idle]);
        }


        //update always 
        sm.DoOperateUpdate();
    }

}
