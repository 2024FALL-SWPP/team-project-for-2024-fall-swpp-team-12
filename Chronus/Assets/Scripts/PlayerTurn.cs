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
            float rotationStep = _playerController.curRotSpeed * Time.deltaTime;
            _playerController.transform.Rotate(0f, Mathf.Sign(_playerController.curTurnAngle) * rotationStep, 0f);
        }
    }
    public void DoneAction(PlayerController sender)
    {
        float currentYRotation = _playerController.transform.eulerAngles.y;
        float targetYRotation = _playerController.playerCurRot.eulerAngles.y + _playerController.curTurnAngle;

        if (Mathf.Abs(Mathf.DeltaAngle(currentYRotation, targetYRotation)) < 1.0f) 
        {
            CompleteRotation(targetYRotation);
        }
    }

    private void CompleteRotation(float targetYRotation)
    {
        _playerController.transform.rotation = Quaternion.Euler(0.0f, targetYRotation, 0.0f);
        _playerController.playerCurRot = _playerController.transform.rotation;
        _playerController.doneAction = true;
    }
}
