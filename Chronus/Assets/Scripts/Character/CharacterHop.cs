using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHop : MonoBehaviour, IState<CharacterBase>
{
    private CharacterBase _CharacterBase;
    private Vector3 tempTargetTranslation;

    private float smallHopRate;
    private float speedVer;
    private bool meetLocalMax;

    public void OperateEnter(CharacterBase sender)
    {
        _CharacterBase = sender;
        _CharacterBase.curHopSpeed = _CharacterBase.moveSpeedVer;
        _CharacterBase.curSpeed = _CharacterBase.moveSpeedHor;

        tempTargetTranslation = _CharacterBase.targetTranslation;

        if (_CharacterBase.animator != null)
        {
            _CharacterBase.animator.SetBool("isMoving", true);
        }

        //small hop motion (part of animation yeah)
        smallHopRate = 3.0f;
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
        float hopStep = _CharacterBase.curHopSpeed * Time.deltaTime;
        _CharacterBase.transform.Translate(Vector3.up * _CharacterBase.curHopDir * hopStep);

        if (_CharacterBase.pushDirection != Vector3.zero)
        {
            float moveStep = _CharacterBase.pushSpeed * Time.deltaTime;
            Vector3 currentTranslation = _CharacterBase.transform.position;
            Vector3 direction = (_CharacterBase.targetTranslation - currentTranslation).normalized;
            _CharacterBase.transform.Translate(direction * moveStep, Space.World);
        }
        else
        {
            float moveStep = _CharacterBase.curSpeed * Time.deltaTime;
            _CharacterBase.transform.Translate(Vector3.forward * moveStep);

            //small hop motion (log graph shape, non-linear it is.) (part of animation yeah)
            if (!meetLocalMax) speedVer -= Mathf.Log(speedVer + 1.0f) * 0.01f;
            else speedVer -= Mathf.Log(-speedVer + 1.0f) * 0.01f;

            //small hop motion (part of animation yeah)
            float smallHopStep = speedVer * Time.deltaTime;
            _CharacterBase.transform.Translate(Vector3.up * smallHopStep);
            if (!meetLocalMax)
            {
                Vector3 currentTranslation = _CharacterBase.transform.position;
                float planeDistance = Mathf.Sqrt((_CharacterBase.targetTranslation.x - currentTranslation.x) * (_CharacterBase.targetTranslation.x - currentTranslation.x)
                    + (_CharacterBase.targetTranslation.z - currentTranslation.z) * (_CharacterBase.targetTranslation.z - currentTranslation.z));
                float maxGap = Mathf.Sqrt((_CharacterBase.targetTranslation.x - _CharacterBase.playerCurPos.x) * (_CharacterBase.targetTranslation.x - _CharacterBase.playerCurPos.x) +
                (_CharacterBase.targetTranslation.z - _CharacterBase.playerCurPos.z) * (_CharacterBase.targetTranslation.z - _CharacterBase.playerCurPos.z));
                if (planeDistance < 0.5f * maxGap)
                {//less than half distance
                    meetLocalMax = true;
                    speedVer = -3.0f * smallHopRate;
                }
            }
        }

        
    }
    public void DoneAction(CharacterBase sender)
    {
        Vector3 currentTranslation = _CharacterBase.transform.position;
        float gap;
        float maxGap;
        if (_CharacterBase.targetTranslation == _CharacterBase.playerCurPos)
        {
            gap = Mathf.Sqrt((currentTranslation.x - tempTargetTranslation.x) * (currentTranslation.x - tempTargetTranslation.x) +
            (currentTranslation.z - tempTargetTranslation.z) * (currentTranslation.z - tempTargetTranslation.z));
            maxGap = Mathf.Sqrt((_CharacterBase.targetTranslation.x - tempTargetTranslation.x) * (_CharacterBase.targetTranslation.x - tempTargetTranslation.x) +
                (_CharacterBase.targetTranslation.z - tempTargetTranslation.z) * (_CharacterBase.targetTranslation.z - tempTargetTranslation.z));
        }
        else
        {
            gap = Mathf.Sqrt((currentTranslation.x - _CharacterBase.playerCurPos.x) * (currentTranslation.x - _CharacterBase.playerCurPos.x) +
            (currentTranslation.z - _CharacterBase.playerCurPos.z) * (currentTranslation.z - _CharacterBase.playerCurPos.z));
            maxGap = Mathf.Sqrt((_CharacterBase.targetTranslation.x - _CharacterBase.playerCurPos.x) * (_CharacterBase.targetTranslation.x - _CharacterBase.playerCurPos.x) +
                (_CharacterBase.targetTranslation.z - _CharacterBase.playerCurPos.z) * (_CharacterBase.targetTranslation.z - _CharacterBase.playerCurPos.z));
        }
        if (Vector3.Distance(currentTranslation, _CharacterBase.targetTranslation) < 0.1f || gap >= maxGap)
        {
            _CharacterBase.transform.position = _CharacterBase.targetTranslation;
            _CharacterBase.playerCurPos = _CharacterBase.transform.position;
            if (_CharacterBase.pushDirection != Vector3.zero)
            {
                _CharacterBase.pushDirection = Vector3.zero;
                _CharacterBase.pushSpeed = 0;
            }
            _CharacterBase.doneAction = true;
        }
    }
}
