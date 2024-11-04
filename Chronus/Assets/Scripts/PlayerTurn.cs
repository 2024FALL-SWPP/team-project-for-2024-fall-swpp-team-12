using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurn : MonoBehaviour, IState<PlayerController>
{
    private PlayerController _playerController;

    private float targetYRotation;

    private float smallHopRate;
    private float speedVer;
    private bool meetLocalMax;


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

        smallHopRate = 1.0f;
        speedVer = _playerController.moveSpeedVer * smallHopRate;
        meetLocalMax = false;
    }

    public void OperateExit(PlayerController sender)
    {
    }

    public void OperateUpdate(PlayerController sender)
    {
        if (!meetLocalMax)
        {
            speedVer -= Mathf.Log(speedVer + 1.0f) * 0.01f;
        }
        else
        {
            speedVer -= Mathf.Log(-speedVer + 1.0f) * 0.01f;
        }

        if (_playerController)
        {
            float rotationStep = _playerController.curRotSpeed * Time.deltaTime;
            _playerController.transform.Rotate(0f, Mathf.Sign(_playerController.curTurnAngle) * rotationStep, 0f);

            float smallHopStep = speedVer * Time.deltaTime;
            _playerController.transform.Translate(Vector3.up * smallHopStep);
            if (!meetLocalMax)
            {
                float currentYRotation = _playerController.transform.eulerAngles.y;
                float angle = Mathf.Abs(Mathf.DeltaAngle(currentYRotation, targetYRotation));
                if (angle < 0.5f * Mathf.Abs(_playerController.curTurnAngle))
                {//less than half angle
                    meetLocalMax = true;
                    speedVer = -3.0f * smallHopRate;
                }
            }
        }
    }
    public void DoneAction(PlayerController sender)
    {
        float currentYRotation = _playerController.transform.eulerAngles.y;
        float angle = Mathf.Abs(Mathf.DeltaAngle(currentYRotation, targetYRotation));
        if (angle < 2.0f) 
        {
            CompleteRotation(targetYRotation);
            CompleteTranslation();
            _playerController.doneAction = true;
        }
    }

    private void CompleteRotation(float targetYRotation)
    {
        _playerController.transform.rotation = Quaternion.Euler(0.0f, targetYRotation, 0.0f);
        _playerController.playerCurRot = _playerController.transform.rotation;
    }
    private void CompleteTranslation()
    {
        _playerController.transform.position = _playerController.playerCurPos;
    }
}
