using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMove : MonoBehaviour, IState<CharacterBase>
{
    private CharacterBase _CharacterBase;
    private Vector3 tempTargetTranslation;

    private float smallHopRate;
    private float speedVer;
    private bool meetLocalMax;

    public void OperateEnter(CharacterBase sender)
    {
        _CharacterBase = sender;
        _CharacterBase.curSpeed = _CharacterBase.moveSpeedHor;

        tempTargetTranslation = _CharacterBase.targetTranslation;

        //small hop motion (part of animation yeah)
        smallHopRate = 1.7f;
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
        gap = (_CharacterBase.targetTranslation == _CharacterBase.playerCurPos)
            ? Vector3.Distance(currentTranslation, tempTargetTranslation)
            : Vector3.Distance(currentTranslation, _CharacterBase.playerCurPos);
        maxGap = (_CharacterBase.targetTranslation == _CharacterBase.playerCurPos)
            ? Vector3.Distance(_CharacterBase.targetTranslation, tempTargetTranslation)
            : Vector3.Distance(_CharacterBase.targetTranslation, _CharacterBase.playerCurPos);
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
