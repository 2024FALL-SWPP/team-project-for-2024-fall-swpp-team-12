using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterIdle : MonoBehaviour, IState<CharacterBase>
{
    private CharacterBase _CharacterBase;
    // State Replace!
    public void OperateEnter(CharacterBase sender)
    {
        _CharacterBase = sender;
        if (_CharacterBase != null)
        {
            _CharacterBase.curSpeed = 0;
        }
    }

    // State be Replaced by others
    public void OperateExit(CharacterBase sender)
    {

    }

    // Always Do something when the current state is this state
    public void OperateUpdate(CharacterBase sender)
    {
        if (_CharacterBase.pushDirection != Vector3.zero) //obstacle push > box ride
        {
            float moveStep = _CharacterBase.pushSpeed * Time.deltaTime;
            Vector3 currentTranslation = _CharacterBase.transform.position;
            Vector3 direction = (_CharacterBase.targetTranslation - currentTranslation).normalized;
            _CharacterBase.transform.Translate(direction * moveStep, Space.World);
        }
        else if (_CharacterBase.isRidingBox)
        {
            float moveStep = _CharacterBase.moveSpeedHor * Time.deltaTime;
            Vector3 currentTranslation = _CharacterBase.transform.position;
            Vector3 direction = (_CharacterBase.targetTranslation - currentTranslation).normalized;
            _CharacterBase.transform.Translate(direction * moveStep, Space.World);
        }
        //need "fall" condition (maybe don't need that)
        //game over by fell condition also.
    }
    public void DoneAction(CharacterBase sender)
    {
        if (_CharacterBase.pushDirection != Vector3.zero) //obstacle push > box ride
        {
            Vector3 currentTranslation = _CharacterBase.transform.position;
            if (Vector3.Distance(currentTranslation, _CharacterBase.targetTranslation) <= 0.1f || Vector3.Distance(currentTranslation, _CharacterBase.playerCurPos) >= 2.0f)
            {
                _CharacterBase.transform.position = _CharacterBase.targetTranslation;
                _CharacterBase.playerCurPos = _CharacterBase.transform.position; //update position.
                _CharacterBase.pushDirection = Vector3.zero;
                _CharacterBase.pushSpeed = 0;
                _CharacterBase.doneAction = true;
                //_CharacterBase.isMoveComplete = true;
                //_CharacterBase.isFallComplete = false;
                _CharacterBase.AdvanceFall();
            }
        }
        else if (_CharacterBase.isRidingBox)
        {
            Vector3 currentTranslation = _CharacterBase.transform.position;
            if (Vector3.Distance(currentTranslation, _CharacterBase.targetTranslation) <= 0.1f || Vector3.Distance(currentTranslation, _CharacterBase.playerCurPos) >= 2.0f)
            {
                _CharacterBase.transform.position = _CharacterBase.targetTranslation;
                _CharacterBase.playerCurPos = _CharacterBase.transform.position;
                _CharacterBase.isRidingBox = false;
                _CharacterBase.doneAction = true;
            }
        }
        else
        {
            _CharacterBase.doneAction = true;
            //_CharacterBase.isMoveComplete = true;
            //_CharacterBase.isFallComplete = false;
            _CharacterBase.AdvanceFall();
        }
    }
}
