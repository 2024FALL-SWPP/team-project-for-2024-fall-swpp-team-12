using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMove : MonoBehaviour, IState<CharacterBase>
{
    private CharacterBase _CharacterBase;
    private Vector3 targetTranslation;

    public void OperateEnter(CharacterBase sender)
    {
        _CharacterBase = sender;
        _CharacterBase.curSpeed = _CharacterBase.moveSpeedHor;
        
        if (_CharacterBase.animator != null)
        {
            _CharacterBase.animator.SetBool("isMoving", true);
        }

        targetTranslation = _CharacterBase.targetTranslation;
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

        if (_CharacterBase)
        {
            float moveStep = _CharacterBase.curSpeed * Time.deltaTime;
            if (_CharacterBase.pushDirection != Vector3.zero)
            {   
                Vector3 currentTranslation = _CharacterBase.transform.position;
                Vector3 direction = (targetTranslation - currentTranslation).normalized;
                _CharacterBase.transform.Translate(direction * moveStep, Space.World);
            }
            else
            {
                _CharacterBase.transform.Translate(Vector3.forward * moveStep);
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
