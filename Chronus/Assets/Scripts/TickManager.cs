using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TickManager : MonoBehaviour
{
    public static event Action OnTick;
    public float tickInterval = 0.5f;

    private void Start()
    {
        InvokeRepeating(nameof(Tick), tickInterval, tickInterval);
    }

    private void Tick()
    {
        OnTick?.Invoke();
    }
}