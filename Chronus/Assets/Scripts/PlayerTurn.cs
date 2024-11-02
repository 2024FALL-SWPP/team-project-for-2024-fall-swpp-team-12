using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurn : MonoBehaviour, IState<PlayerController>
{
    private PlayerController _playerController;

    private float currentYRotation;
    private float targetYRotation;

    public void OperateEnter(PlayerController sender)
    {
        _playerController = sender;
        _playerController.curRotSpeed = _playerController.turnSpeed * Mathf.Abs(_playerController.curTurnAngle) / 90.0f;
        
        targetYRotation = _playerController.playerCurRot.eulerAngles.y + _playerController.curTurnAngle;
        if (targetYRotation == 360.0f)
        {
            targetYRotation = 0.0f;
        }
        else if (targetYRotation == -90.0f)
        {
            targetYRotation = 270.0f;
        }
        else if (targetYRotation == 450.0f)
        {
            targetYRotation = 90.0f;
        }
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
        currentYRotation = _playerController.transform.eulerAngles.y;
        if (Mathf.Abs(Mathf.DeltaAngle(currentYRotation, targetYRotation)) < 3.0f) 
        {
            CompleteRotation(targetYRotation);
            _playerController.doneAction = true;
        }
    }

    private void CompleteRotation(float targetYRotation)
    {
        _playerController.transform.rotation = Quaternion.Euler(0.0f, targetYRotation, 0.0f);
        _playerController.playerCurRot = _playerController.transform.rotation;
    }
}
