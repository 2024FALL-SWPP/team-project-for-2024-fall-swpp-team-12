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
    // 일단 플레이어 얘부터 동작시켜봐야 하니 임시로 여기다가 두겠습니다.
    public bool turnClock = false;
    //when turnClock false -> true  state 변화



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

        //StateMachine class의 Object를 생성함!
        sm = new StateMachine<PlayerController>(this, dicState[PlayerState.Idle]);
    }

    void Update()
    {
        //여기서 State를 어떻게 변화시킬 지에 대한 logic 작성해야 함
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
