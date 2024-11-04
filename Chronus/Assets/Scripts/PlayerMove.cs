using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour, IState<PlayerController>
{
    private PlayerController _playerController;
    public void OperateEnter(PlayerController sender)
    {
        _playerController = sender;
        _playerController.curSpeed = _playerController.moveSpeedHor;
    }

    public void OperateExit(PlayerController sender)
    {
    }

    public void OperateUpdate(PlayerController sender)
    {
        if (_playerController)
        {
            _playerController.transform.Translate(Vector3.forward * _playerController.curSpeed * Time.deltaTime);
        }
    }
    public void DoneAction(PlayerController sender)
    {
        if (Vector3.Distance(_playerController.playerCurPos, _playerController.transform.position) >= 2.0f)
        {
            if (_playerController.playerCurRot == Quaternion.Euler(0, 0, 0))
            {
                _playerController.transform.position = _playerController.playerCurPos + new Vector3(0, 0, 2); //Ȥ�ó��� ���� ���ɼ� ������ ��Ȯ�� ��ġ �Է�����
            }
            else if (_playerController.playerCurRot == Quaternion.Euler(0, 90, 0))
            {
                _playerController.transform.position = _playerController.playerCurPos + new Vector3(2, 0, 0); //Ȥ�ó��� ���� ���ɼ� ������ ��Ȯ�� ��ġ �Է�����
            }
            else if (_playerController.playerCurRot == Quaternion.Euler(0, -90, 0))
            {
                _playerController.transform.position = _playerController.playerCurPos + new Vector3(-2, 0, 0); //Ȥ�ó��� ���� ���ɼ� ������ ��Ȯ�� ��ġ �Է�����
            }
            else if (_playerController.playerCurRot == Quaternion.Euler(0, 180, 0))
            {
                _playerController.transform.position = _playerController.playerCurPos + new Vector3(0, 0, -2); //Ȥ�ó��� ���� ���ɼ� ������ ��Ȯ�� ��ġ �Է�����
            }

            _playerController.playerCurPos = _playerController.transform.position; //���� ��ġ���� ����
            _playerController.doneAction = true;
        }
    }
}
