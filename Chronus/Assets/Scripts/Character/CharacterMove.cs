using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMove : MonoBehaviour, IState<CharacterBase>
{
    private CharacterBase _CharacterBase;

    private Vector3 targetTranslation;

    private float smallHopRate;
    private float speedVer;
    private bool meetLocalMax;

    public void OperateEnter(CharacterBase sender)
    {
        _CharacterBase = sender;
        _CharacterBase.curSpeed = _CharacterBase.moveSpeedHor;
        
        if (_CharacterBase.animator != null)
        {
            _CharacterBase.animator.SetBool("isMoving", true);
        }

        targetTranslation = _CharacterBase.targetTranslation;

        //small hop motion (part of animation yeah)
        smallHopRate = 2.0f;
        speedVer = _CharacterBase.moveSpeedVer * smallHopRate;
        meetLocalMax = false;
    }

    public void OperateExit(CharacterBase sender)
    {
        if (_CharacterBase.animator != null)
        {
            _CharacterBase.animator.SetBool("isMoving", false);
        }
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
            float moveStep = _CharacterBase.curSpeed * Time.deltaTime;
            _CharacterBase.transform.Translate(Vector3.forward * moveStep);

            //small hop motion (part of animation yeah)
            float smallHopStep = speedVer * Time.deltaTime;
            _CharacterBase.transform.Translate(Vector3.up * smallHopStep);
            if (!meetLocalMax)
            {
                Vector3 currentTranslation = _CharacterBase.transform.position;
                float planeDistance = Mathf.Sqrt((targetTranslation.x - currentTranslation.x)*(targetTranslation.x - currentTranslation.x) + (targetTranslation.z - currentTranslation.z)*(targetTranslation.z - currentTranslation.z));
                if (planeDistance < 0.5f * 2.0f)
                {//less than half distance
                    meetLocalMax = true;
                    speedVer = -3.0f * smallHopRate;
                }
            }
        }
    }
    public void DoneAction(CharacterBase sender) //just check for x and z (no need to check y gap)
    {
        Vector3 currentTranslation = _CharacterBase.transform.position;
        float gap = Mathf.Sqrt((_CharacterBase.playerCurPos.x - currentTranslation.x) * (_CharacterBase.playerCurPos.x - currentTranslation.x) + (_CharacterBase.playerCurPos.z - currentTranslation.z) * (_CharacterBase.playerCurPos.z - currentTranslation.z));
        float planeDistance = Mathf.Sqrt((targetTranslation.x - currentTranslation.x)*(targetTranslation.x - currentTranslation.x) + (targetTranslation.z - currentTranslation.z)*(targetTranslation.z - currentTranslation.z));
        if (planeDistance < 0.1f || gap >= 2.0f)
        {
            CompleteTranslation(targetTranslation);
            _CharacterBase.doneAction = true;
        }
    }
    private void CompleteTranslation(Vector3 targetTranslation)
    {
        _CharacterBase.transform.position = targetTranslation;
        _CharacterBase.playerCurPos = _CharacterBase.transform.position; //update current position information
    }
}
