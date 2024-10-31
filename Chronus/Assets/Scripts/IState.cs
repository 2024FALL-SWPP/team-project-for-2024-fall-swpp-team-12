using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState<T>
{
    // State Replace!
    void OperateEnter(T sender);
    // Always Do something when the current state is this state
    void OperateUpdate(T sender);
    // State be Replaced by others
    void OperateExit(T sender);

}
