using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterIdle : MonoBehaviour, IState<CharacterBase>
{
    private CharacterBase _CharacterBase;
    private Vector3 targetTranslation;
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
        if (_CharacterBase.pushDirection != Vector3.zero)
        {
            targetTranslation = _CharacterBase.targetTranslation;
            float moveStep = _CharacterBase.moveSpeedHor * Time.deltaTime;
            Vector3 currentTranslation = _CharacterBase.transform.position;
            Vector3 direction = (targetTranslation - currentTranslation).normalized;
            _CharacterBase.transform.Translate(direction * moveStep, Space.World);
        }
        //need "fall" condition
        //game over by fell condition also.
    }
    public void DoneAction(CharacterBase sender)
    {
        if (_CharacterBase.pushDirection != Vector3.zero)
        {
            Vector3 currentTranslation = _CharacterBase.transform.position;
            if (Vector3.Distance(currentTranslation, targetTranslation) < 0.1f)
            {
                _CharacterBase.transform.position = targetTranslation;
                _CharacterBase.playerCurPos = _CharacterBase.transform.position;
                _CharacterBase.doneAction = true;
                _CharacterBase.pushDirection = Vector3.zero;
            }
        }
        else
        {
            _CharacterBase.doneAction = true;
        }
    }
}
