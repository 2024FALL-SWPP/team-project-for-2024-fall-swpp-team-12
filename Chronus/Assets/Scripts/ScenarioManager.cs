using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager scenarioManager;
    public GameObject monologueCanvas;
    private string[] monologues;
    private void Awake()
    {
        if (scenarioManager == null) { scenarioManager = this; }
        monologues = new string[]
        {
            "Monologue001-1",
            "Monologue001-2",
            "Monologue001-3",
            "Monologue001-4",
            "Monologue001-5",
            "Monologue002-1",
            "Monologue002-2",
            "Monologue002-3",
            "Monologue002-4",
            "Monologue003-1",
            "Monologue003-2",
            "Monologue003-3",
            "Monologue003-4",
            "Monologue004-1",
            "Monologue004-2",
            "Monologue004-3"
        };
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
