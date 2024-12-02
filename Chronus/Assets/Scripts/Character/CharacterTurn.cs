using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTurn : MonoBehaviour, IState<CharacterBase>
{
    private CharacterBase _CharacterBase;

    private float targetYRotation;

    private float smallHopRate;
    private float speedVer;
    private bool meetLocalMax;


    public void OperateEnter(CharacterBase sender)
    {
        _CharacterBase = sender;
        //turn left or right speed  x 2   =   turn behind speed
        _CharacterBase.curRotSpeed = _CharacterBase.turnSpeed * _CharacterBase.curTurnAngle / 90.0f;
        targetYRotation = _CharacterBase.playerCurRot.eulerAngles.y + _CharacterBase.curTurnAngle;
        if (targetYRotation >= 360.0f)
        {
            targetYRotation -= 360.0f;
        }
        else if (targetYRotation < 0.0f)
        {
            targetYRotation += 360.0f;
        }

        //small hop motion (part of animation yeah)
        smallHopRate = 1.3f;
        speedVer = _CharacterBase.moveSpeedVer * smallHopRate;
        meetLocalMax = false;
    }

    public void OperateExit(CharacterBase sender)
    {
    }

    public void OperateUpdate(CharacterBase sender)
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

        if (_CharacterBase)
        {
            float rotationStep = _CharacterBase.curRotSpeed * Time.deltaTime;
            _CharacterBase.transform.Rotate(0f, rotationStep, 0f);

            //small hop motion (part of animation yeah)
            float smallHopStep = speedVer * Time.deltaTime;
            _CharacterBase.transform.Translate(Vector3.up * smallHopStep);
            if (!meetLocalMax)
            {
                float currentYRotation = _CharacterBase.transform.eulerAngles.y;
                float angle = Mathf.Abs(Mathf.DeltaAngle(currentYRotation, targetYRotation));
                if (angle < 0.5f * Mathf.Abs(_CharacterBase.curTurnAngle))
                {//less than half angle
                    meetLocalMax = true;
                    speedVer = -3.0f * smallHopRate;
                }
            }
        }
    }
    public void DoneAction(CharacterBase sender)
    {
        float currentYRotation = _CharacterBase.transform.eulerAngles.y;
        float gap = Mathf.DeltaAngle(_CharacterBase.playerCurRot.eulerAngles.y, currentYRotation);
        float angle = Mathf.Abs(Mathf.DeltaAngle(currentYRotation, targetYRotation));
        if (angle < 1.0f ||
            (_CharacterBase.curTurnAngle > 0 && (gap >= _CharacterBase.curTurnAngle || gap < 0)) ||
            (_CharacterBase.curTurnAngle < 0 && (gap <= _CharacterBase.curTurnAngle || gap > 0)))
        {
            CompleteRotation(targetYRotation);
            CompleteTranslation();
            _CharacterBase.doneAction = true;
        }
    }

    private void CompleteRotation(float targetYRotation)
    {
        _CharacterBase.transform.rotation = Quaternion.Euler(0.0f, targetYRotation, 0.0f);
        _CharacterBase.playerCurRot = _CharacterBase.transform.rotation; //update current rotation information
    }
    private void CompleteTranslation()
    {
        _CharacterBase.transform.position = _CharacterBase.playerCurPos; //initial position is the target position.
    }
}
