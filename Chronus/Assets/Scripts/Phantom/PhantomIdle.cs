using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhantomIdle : MonoBehaviour, IState<PhantomController>
{
    private PhantomController _playerController;

    // State Replace!
    public void OperateEnter(PhantomController sender)
    {
        _playerController = sender;
        if (_playerController != null)
        {
            _playerController.curSpeed = 0;
        }
    }

    // State be Replaced by others
    public void OperateExit(PhantomController sender)
    {

    }

    // Always Do something when the current state is this state
    public void OperateUpdate(PhantomController sender)
    {
        //need "fall" condition
        //game over by fell condition also.
    }
    public void DoneAction(PhantomController sender)
    {
        if (TurnManager.turnManager.turnClock && !TurnManager.turnManager.dicTurnCheck["Phantom"])
        {
            _playerController.doneAction = true;
        }
    }
}
