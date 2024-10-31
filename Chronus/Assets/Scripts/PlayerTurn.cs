using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurn : MonoBehaviour, IState<PlayerController>
{
    private Vector3 _turnDirection;
    private PlayerController _playerController;
    public void OperateEnter(PlayerController sender)
    {
        if (!_playerController)
            _playerController = sender;

        _turnDirection.x = (float)_playerController.curTurnDir;
    }

    public void OperateExit(PlayerController sender)
    {
    }

    public void OperateUpdate(PlayerController sender)
    {
        if (_playerController.curSpeed > 0)
        {
            _playerController.transform.Translate(_turnDirection * _playerController.turnDistance);
        }

    }
}
