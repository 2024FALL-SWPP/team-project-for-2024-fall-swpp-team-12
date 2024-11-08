using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHop : MonoBehaviour, IState<PlayerController>
{
    private PlayerController _playerController;

    private Vector3 targetTranslation;

    private float smallHopRate;
    private float speedVer;
    private bool meetLocalMax;

    public void OperateEnter(PlayerController sender)
    {
        _playerController = sender;
        _playerController.curHopSpeed = _playerController.moveSpeedVer;
        _playerController.curSpeed = _playerController.moveSpeedHor;

        if (_playerController.animator != null)
        {
            _playerController.animator.SetBool("isMoving", true);
        }

        if (_playerController.playerCurRot.eulerAngles.y == 0.0f)
        {
            targetTranslation = _playerController.playerCurPos + new Vector3(0, 1.0f * _playerController.curHopDir, 2.0f); //혹시나의 오차 가능성 때문에 정확한 위치 입력해줌
        }
        else if (_playerController.playerCurRot.eulerAngles.y == 90.0f)
        {
            targetTranslation = _playerController.playerCurPos + new Vector3(2.0f, 1.0f * _playerController.curHopDir, 0); //혹시나의 오차 가능성 때문에 정확한 위치 입력해줌
        }
        else if (_playerController.playerCurRot.eulerAngles.y == 270.0f)
        {
            targetTranslation = _playerController.playerCurPos + new Vector3(-2.0f, 1.0f * _playerController.curHopDir, 0); //혹시나의 오차 가능성 때문에 정확한 위치 입력해줌
        }
        else if (_playerController.playerCurRot.eulerAngles.y == 180.0f)
        {
            targetTranslation = _playerController.playerCurPos + new Vector3(0, 1.0f * _playerController.curHopDir, -2.0f); //혹시나의 오차 가능성 때문에 정확한 위치 입력해줌
        }

        //small hop motion (part of animation yeah)
        smallHopRate = 3.0f;
        speedVer = _playerController.moveSpeedVer * smallHopRate;
        meetLocalMax = false;
    }

    public void OperateExit(PlayerController sender)
    {
        if (_playerController.animator != null)
        {
            _playerController.animator.SetBool("isMoving", false);
        }
    }

    public void OperateUpdate(PlayerController sender)
    {
        //small hop motion (log graph shape, non-linear it is.) (part of animation yeah)
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
            float hopStep = _playerController.curHopSpeed * Time.deltaTime;
            _playerController.transform.Translate(Vector3.up * _playerController.curHopDir * hopStep);

            float moveStep = _playerController.curSpeed * Time.deltaTime;
            _playerController.transform.Translate(Vector3.forward * moveStep);

            //small hop motion (part of animation yeah)
            float smallHopStep = speedVer * Time.deltaTime;
            _playerController.transform.Translate(Vector3.up * smallHopStep);
            if (!meetLocalMax)
            {
                Vector3 currentTranslation = _playerController.transform.position;
                float planeDistance = Mathf.Sqrt((targetTranslation.x - currentTranslation.x)*(targetTranslation.x - currentTranslation.x) + (targetTranslation.z - currentTranslation.z)*(targetTranslation.z - currentTranslation.z));
                if (planeDistance < 0.5f * 2.0f)
                {//less than half distance
                    meetLocalMax = true;
                    speedVer = -3.0f * smallHopRate;
                }
            }
        }
    }
    public void DoneAction(PlayerController sender) //just check for x and z (no need to check y gap)
    {
        Vector3 currentTranslation = _playerController.transform.position;
        float gap = Mathf.Sqrt((_playerController.playerCurPos.x - currentTranslation.x) * (_playerController.playerCurPos.x - currentTranslation.x) + (_playerController.playerCurPos.z - currentTranslation.z) * (_playerController.playerCurPos.z - currentTranslation.z));
        float planeDistance = Mathf.Sqrt((targetTranslation.x - currentTranslation.x)*(targetTranslation.x - currentTranslation.x) + (targetTranslation.z - currentTranslation.z)*(targetTranslation.z - currentTranslation.z));
        if (planeDistance < 0.1f || gap >= 2.0f)
        {
            CompleteTranslation(targetTranslation);
            _playerController.doneAction = true;
        }
    }
    private void CompleteTranslation(Vector3 targetTranslation)
    {
        _playerController.transform.position = targetTranslation;
        _playerController.playerCurPos = _playerController.transform.position; //현재 위치정보 갱신
    }
}
