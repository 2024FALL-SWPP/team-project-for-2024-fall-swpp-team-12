using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurn : MonoBehaviour, IState<PlayerController>
{
    private PlayerController _playerController;
    public void OperateEnter(PlayerController sender)
    {
        _playerController = sender;
        _playerController.curRotSpeed = _playerController.turnSpeed;

    }

    public void OperateExit(PlayerController sender)
    {
    }

    public void OperateUpdate(PlayerController sender)
    {
        if (_playerController.curRotSpeed > 0)
        {
            _playerController.transform.Rotate(0f, _playerController.curTurnDir * _playerController.curRotSpeed * Time.deltaTime, 0f);
        }

    }
    public void DoneAction(PlayerController sender)
    {
        if (_playerController.curTurnAngle >= 0)
            if (_playerController.transform.rotation.y - _playerController.playerCurRot.y >= _playerController.curTurnAngle)
            {
                _playerController.transform.rotation = _playerController.playerCurRot * Quaternion.Euler(0.0f, _playerController.curTurnAngle, 0.0f); //혹시나의 오차 가능성 때문에 정확한 위치 입력해줌
                _playerController.playerCurRot = _playerController.transform.rotation; //현재 위치정보 갱신
                _playerController.doneAction = true;
            }
        else
            if (_playerController.transform.rotation.y - _playerController.playerCurRot.y <= _playerController.curTurnAngle)
            {
                _playerController.transform.rotation = _playerController.playerCurRot * Quaternion.Euler(0.0f, _playerController.curTurnAngle, 0.0f); //혹시나의 오차 가능성 때문에 정확한 위치 입력해줌
                _playerController.playerCurRot = _playerController.transform.rotation; //현재 위치정보 갱신
                _playerController.doneAction = true;
            }
    }
}
