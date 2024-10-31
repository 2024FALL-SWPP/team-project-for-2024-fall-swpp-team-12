using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T>
{
    private T m_sender;

    //current State는 이 public Property에 담긴다. CurState!!!
    public IState<T> CurState { get; set; }

    //기본 상태를 설정하는 Constructer
    public StateMachine(T sender, IState<T> state)
    {
        m_sender = sender;
        SetState(state);
    }

    //State Replacement!
    public void SetState(IState<T> state)
    {
        Debug.Log("SetState : " + state);

        // null에러출력
        if (m_sender == null)
        {
            Debug.LogError("m_sender ERROR");
            return;
        }

        if (CurState == state)
        {
            Debug.LogWarningFormat("Same state : ", state);
            return;
        }

        //Exit
        if (CurState != null)
            CurState.OperateExit(m_sender);

        //Replace State.
        CurState = state;

        //Enter
        if (CurState != null)
            CurState.OperateEnter(m_sender);

        Debug.Log("SetNextState : " + state);

    }

    //Update (always)
    public void DoOperateUpdate()
    {
        if (m_sender == null)
        {
            Debug.LogError("invalid m_sener");
            return;
        }
        //Update
        CurState.OperateUpdate(m_sender);
    }
}
