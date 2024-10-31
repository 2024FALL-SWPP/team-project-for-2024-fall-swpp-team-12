using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHop : MonoBehaviour, IState<PlayerController>
{
    private PlayerController _playerController;
    public void OperateEnter(PlayerController sender)
    {
        _playerController = sender;
        _playerController.curSpeed = _playerController.moveSpeedVer;
    }

    public void OperateExit(PlayerController sender)
    {
    }

    public void OperateUpdate(PlayerController sender)
    {
        if (_playerController)
        {
            if (_playerController.curSpeed > 0)
            {
                _playerController.transform.Translate(Vector3.up * _playerController.curSpeed);
            }
        }
    }
}
