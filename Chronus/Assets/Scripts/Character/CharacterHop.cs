using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHop : MonoBehaviour, IState<CharacterBase>
{
    private CharacterBase _CharacterBase;

    private Vector3 targetTranslation;

    private float smallHopRate;
    private float speedVer;
    private bool meetLocalMax;

    public void OperateEnter(CharacterBase sender)
    {
        _CharacterBase = sender;
        _CharacterBase.curHopSpeed = _CharacterBase.moveSpeedVer;
        _CharacterBase.curSpeed = _CharacterBase.moveSpeedHor;

        if (_CharacterBase.animator != null)
        {
            _CharacterBase.animator.SetBool("isMoving", true);
        }

        targetTranslation = _CharacterBase.targetTranslation;

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
        // There's no way. Need to capture targetTranslation changes. (to check if being pushed)
        targetTranslation = _CharacterBase.targetTranslation;

        //small hop motion (log graph shape, non-linear it is.) (part of animation yeah)
        if (!meetLocalMax) speedVer -= Mathf.Log(speedVer + 1.0f) * 0.01f;
        else speedVer -= Mathf.Log(-speedVer + 1.0f) * 0.01f;

        float hopStep = _CharacterBase.curHopSpeed * Time.deltaTime;
        _CharacterBase.transform.Translate(Vector3.up * _CharacterBase.curHopDir * hopStep);

        float moveStep = _CharacterBase.curSpeed * Time.deltaTime;
        if (_CharacterBase.pushDirection != Vector3.zero)
        {
            Vector3 currentTranslation = _CharacterBase.transform.position;
            Vector3 direction = (targetTranslation - currentTranslation).normalized;
            _CharacterBase.transform.Translate(2f * direction * moveStep, Space.World);
        }
        else
        {
            _CharacterBase.transform.Translate(Vector3.forward * moveStep);
        }

        //small hop motion (part of animation yeah)
        float smallHopStep = speedVer * Time.deltaTime;
        _CharacterBase.transform.Translate(Vector3.up * smallHopStep);
        if (!meetLocalMax)
        {
            Vector3 currentTranslation = _CharacterBase.transform.position;
            float planeDistance = Mathf.Sqrt((targetTranslation.x - currentTranslation.x) * (targetTranslation.x - currentTranslation.x) + (targetTranslation.z - currentTranslation.z) * (targetTranslation.z - currentTranslation.z));
            if (planeDistance < 0.5f * 2.0f)
            {//less than half distance
                meetLocalMax = true;
                speedVer = -3.0f * smallHopRate;
            }
        }
    }
    public void DoneAction(CharacterBase sender)
    {
        Vector3 currentTranslation = _CharacterBase.transform.position;
        if (Vector3.Distance(currentTranslation, targetTranslation) < 0.2f)
        {
            _CharacterBase.transform.position = targetTranslation;
            _CharacterBase.playerCurPos = _CharacterBase.transform.position;
            _CharacterBase.doneAction = true;
            _CharacterBase.pushDirection = Vector3.zero;
        }
    }
}
