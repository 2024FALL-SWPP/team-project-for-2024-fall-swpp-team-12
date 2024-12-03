using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager scenarioManager;

    private void Awake()
    {
        if (scenarioManager == null) { scenarioManager = this; } //singleton
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
