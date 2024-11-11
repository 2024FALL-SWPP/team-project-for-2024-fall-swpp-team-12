using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterIdle : MonoBehaviour, IState<CharacterBase>
{
    private CharacterBase _CharacterBase;

    // State Replace!
    public void OperateEnter(CharacterBase sender)
    {
        _CharacterBase = sender;
        if (_CharacterBase != null)
        {
            _CharacterBase.curSpeed = 0;
        }
    }

    // State be Replaced by others
    public void OperateExit(CharacterBase sender)
    {

    }

    // Always Do something when the current state is this state
    public void OperateUpdate(CharacterBase sender)
    {
        //need "fall" condition
        //game over by fell condition also.
    }
    public void DoneAction(CharacterBase sender)
    {
        _CharacterBase.doneAction = true;
    }
}
