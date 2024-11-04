using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHop : MonoBehaviour, IState<PlayerController>
{
    private PlayerController _playerController;
    public void OperateEnter(PlayerController sender)
    {
        _playerController = sender;
        _playerController.curSpeed = _playerController.moveSpeedVer;
    }

    public void OperateExit(PlayerController sender)
    {
    }

    public void OperateUpdate(PlayerController sender)
    {
        if (_playerController)
        {
            if (_playerController.curSpeed > 0)
            {
                _playerController.transform.Translate(Vector3.up * _playerController.curSpeed);
            }
        }
    }
    public void DoneAction(PlayerController sender)
    {
        if (Vector3.Distance(_playerController.playerCurPos, _playerController.transform.position) >= 1.0f)
        {
            _playerController.transform.position = _playerController.playerCurPos + new Vector3(0, 1, 0); //Ȥ�ó��� ���� ���ɼ� ������ ��Ȯ�� ��ġ �Է�����
            _playerController.playerCurPos = _playerController.transform.position; //���� ��ġ���� ����
            _playerController.doneAction = true;
        }
    }
}
