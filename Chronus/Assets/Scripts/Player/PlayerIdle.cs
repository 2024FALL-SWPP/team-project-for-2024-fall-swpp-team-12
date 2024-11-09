using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdle : MonoBehaviour, IState<PlayerController>
{
    private PlayerController _playerController;

    // State Replace!
    public void OperateEnter(PlayerController sender)
    {
        _playerController = sender;
        if (_playerController != null)
        {
            _playerController.curSpeed = 0;
        }
    }

    // State be Replaced by others
    public void OperateExit(PlayerController sender)
    {

    }

    // Always Do something when the current state is this state
    public void OperateUpdate(PlayerController sender)
    {
        //need "fall" condition
        //game over by fell condition also.
    }
    public void DoneAction(PlayerController sender)
    {
        _playerController.doneAction = true;
    }
}
