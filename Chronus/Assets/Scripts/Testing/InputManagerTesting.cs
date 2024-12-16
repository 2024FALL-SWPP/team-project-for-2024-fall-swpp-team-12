using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManagerTesting : MonoBehaviour
{
    public static InputManagerTesting instance; //singleton
    private InputManager inputManager;
    private TurnManager turnManager;
    public PlayerController player;

    public bool case1 = false; // Pushing same box to different direction (-> + ^)
    public bool case2 = false; // Box target is the same as player target
    public bool case3 = false; // Box target is the same as clone target
    public bool case4 = false; // Invalid + Valid inputs
    public bool case5 = false; // Simultaneous Actions
    public bool case6 = false; // Time rewind without Space Toggle
    public bool case7 = false; // F-043 Pushing Stacked Boxes + F-026-2 Stacked Boxes Fall when pushed to empty ground
    public bool case8 = false; // Splitting Two boxes + F-035 Stacked boxes pushed in different directions
    public bool case9 = false; // Stacked Box target is the same as player target
    public bool case10 = false; // Stacked Box target is the same as clone target
    public bool case11 = false; // Stacked Boxes drop when tile disappears at the same time (when tile initially active)
    public bool case12 = false; // Stacked Boxes drop when tile disappears at the same time (when tile intially inactive)
    public bool case13 = false; // Stack 1 more box on top of 2 boxes and see if 3 boxes move together
    public bool case14 = false; // Stacked Box: Top box moved by main character and bottom by phantom
    
    private Dictionary<int, Dictionary<string, Vector3>> expectedPositions = new Dictionary<int, Dictionary<string, Vector3>>();

    private List<int> failedCases = new List<int>();

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate if it exists
        }

        inputManager = InputManager.inputManager;
        if (inputManager == null)
        {
            return;
        }
        turnManager = FindObjectOfType<TurnManager>();
        if (turnManager == null)
        {
            Debug.LogError("TurnManager is not found in the scene. Ensure TurnManager is present.");
            return;
        }
        
        InitializeExpectedPositions();
        StartCoroutine(RunTests());
    }
    
    private void InitializeExpectedPositions()
    {
        expectedPositions[1] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, 1) },
            { "boxA", new Vector3(1, 1, 3) },
            { "boxA (1)", new Vector3(-1, 4, 1) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };


        expectedPositions[2] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, -1) },
            { "boxA", new Vector3(1, 1, 1) },
            { "boxA (1)", new Vector3(-1, 4, 1) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };
        
        expectedPositions[3] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, -1) },
            { "boxA", new Vector3(1, 1, 3) },
            { "boxA (1)", new Vector3(-1, 4, 1) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };
        
        expectedPositions[4] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(3, 1, -1) },
            { "boxA", new Vector3(1, 1, 1) },
            { "boxA (1)", new Vector3(-1, 4, 1) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };
        
        expectedPositions[5] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(3, 1, 1) },
            { "boxA", new Vector3(1, 1, 1) },
            { "boxA (1)", new Vector3(-1, 4, 1) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };
        
        expectedPositions[6] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, -1) },
            { "boxA", new Vector3(1, 1, 1) },
            { "boxA (1)", new Vector3(-1, 4, 1) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };
        
        expectedPositions[7] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, -1) },
            { "boxA", new Vector3(1, -10.1834f, -4) },
            { "boxA (1)", new Vector3(-1, -10.03161f, -3) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };
        
        expectedPositions[8] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, 1) }, 
            { "Phantom", new Vector3(1,1,1)},
            { "boxA", new Vector3(1, 1, 3) },
            { "boxA (1)", new Vector3(1, 3, 3) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };
        
        expectedPositions[9] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, 3) }, 
            { "Phantom", new Vector3(1,1,-1)},
            { "boxA", new Vector3(1, 1, 1) },
            { "boxA (1)", new Vector3(1, 3, 1) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };
        
        expectedPositions[10] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, 1) },
            { "boxA", new Vector3(1, 1, 3) },
            { "boxA (1)", new Vector3(1, 3, 3) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };
        
        expectedPositions[11] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, 3) }, 
            { "Phantom", new Vector3(5,1,1)},
            { "boxA", new Vector3(1, -10.1834f, 5) },
            { "boxA (1)", new Vector3(1, -10.03161f, 5) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };
        
        expectedPositions[12] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, 5) }, 
            { "boxA", new Vector3(1, -10.1834f, 7) },
            { "boxA (1)", new Vector3(1, -10.03161f, 7) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };
        
        expectedPositions[13] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, 3) }, 
            { "boxA", new Vector3(1, 1, 1) },
            { "boxA (1)", new Vector3(1, 3,1) },
            { "boxA (3)", new Vector3(1, 5, 1) }
        };
        
        expectedPositions[14] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(3, 1, 1) }, 
            { "boxA", new Vector3(1, 1, 3) },
            { "boxA (1)", new Vector3(5, 1,1) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };
        

    }

    private IEnumerator RunTests()
    {
        inputManager.OnMovementControl += LogAction;
        inputManager.OnTimeRewindControl += LogAction;

        Debug.Log("Starting all test cases...");

        for (int i = 1; i <= 14; i++)
        {
            Debug.Log($"Starting Test Case {i}...");
            IEnumerator testCase = GetTestCase(i);
            yield return RunTestCase(i, testCase);
            /*bool runCase = ShouldRunCase(i);
            if (runCase)
            {
                IEnumerator testCase = GetTestCase(i);
                yield return RunTestCase(i, testCase);
            }*/
        }

        inputManager.OnMovementControl -= LogAction;
        inputManager.OnTimeRewindControl -= LogAction;

        if (failedCases.Count == 0)
        {
            Debug.Log("All test cases passed successfully!");
        }
        else
        {
            Debug.Log($"Failed Test Cases: {string.Join(", ", failedCases)}");
        }
    }
    /*private bool ShouldRunCase(int caseNumber)
    {
        switch (caseNumber)
        {
            case 1: return case1;
            case 2: return case2;
            case 3: return case3;
            case 4: return case4;
            case 5: return case5;
            case 6: return case6;
            case 7: return case7;
            case 8: return case8;
            case 9: return case9;
            case 10: return case10;
            case 11: return case11;
            case 12: return case12;
            case 13: return case13;
            case 14: return case14;
            default: return false;
        }
    }*/


    
    private IEnumerator GetTestCase(int caseNumber)
    {
        switch (caseNumber)
        {
            case 1: return Case1();
            case 2: return Case2();
            case 3: return Case3();
            case 4: return Case4();
            case 5: return Case5();
            case 6: return Case6();
            case 7: return Case7();
            case 8: return Case8();
            case 9: return Case9();
            case 10: return Case10();
            case 11: return Case11();
            case 12: return Case12();
            case 13: return Case13();
            case 14: return Case14();
            default:
                return null;
        }
    }
    private IEnumerator RunTestCase(int caseNumber, IEnumerator testCase)
    {
        
        
        Debug.Log($"Running Test Case {caseNumber}...");

        yield return testCase;

        bool result = false;
        yield return StartCoroutine(ValidatePositions(caseNumber, (passed) => result = passed));

        if (!result)
        {
            failedCases.Add(caseNumber);
        }

        /*if (player.isBlinking || TurnManager.turnManager.CLOCK || player.isTimeRewinding) 
        {
            yield break; // Gracefully exit the coroutine
        }
        //block when: time rewind mode, clock on, running 'game over'.

        if (player.willDropDeath)
        {
            player.StopFallCharacter();
        }

        //from PlayerController - GameOverAndReset().
        if (!PhantomController.phantomController.isFallComplete) PhantomController.phantomController.KillCharacter();
        else PhantomController.phantomController.KillPhantom(); //ActiveSelf false, isPhantomExisting false.
        TurnManager.turnManager.boxList.ForEach(box => box.DropKillBox()); //setactive false
        TurnManager.turnManager.ResetPushingMotion();
        //the reason of 'partially copied' code above: consider pressing the reset button (ENTER) while playing.

        //reset logs (restore first positions,states  and  initialize iterator)
        TurnManager.turnManager.ResetObjects();*/
        inputManager.OnReset?.Invoke();
        
        
        Debug.Log($"Test Case {caseNumber} Completed. Positions Reset.");
        yield return new WaitForSeconds(1);
    }

    private IEnumerator ValidatePositions(int caseNumber, System.Action<bool> callback)
    {
        if (!expectedPositions.ContainsKey(caseNumber))
        {
            Debug.LogError($"No expected positions defined for Case {caseNumber}");
            callback(false);
            yield break;
        }

        bool passed = true;

        foreach (var kvp in expectedPositions[caseNumber])
        {
            string objectName = kvp.Key;
            Vector3 expectedPosition = kvp.Value;

            GameObject obj = GameObject.Find(objectName);

            // For destroyed objects that fell
            if (obj == null)
            {
                if (expectedPosition.y < -10f) // Destroyed objects had y value less than -10f
                {
                    Debug.Log($"Case {caseNumber}: {objectName} is destroyed as expected.");
                }
                else
                {
                    Debug.LogError($"Case {caseNumber} Failed: Object {objectName} not found but was expected at {expectedPosition}.");
                    passed = false;
                }
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            // Wait a second before checking position because the object might not be destroyed yet
            yield return new WaitForSeconds(1);

            Vector3 actualPosition = obj.transform.position;

            if (Vector3.Distance(actualPosition, expectedPosition) > 0.01f)
            {
                Debug.LogError($"Case {caseNumber} Failed: {objectName} position {actualPosition} does not match expected {expectedPosition}");
                passed = false;
            }
            else
            {
                Debug.Log($"Case {caseNumber}: {objectName} position matches {expectedPosition}");
            }
        }

        callback(passed);
    }



    private IEnumerator Case1()
    {
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);
    }
    private IEnumerator Case2()
    {
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("s");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);
    }

    private IEnumerator Case3()
    {
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);
    }
    private IEnumerator Case4()
    {
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("g");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("h");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1.5f);

        inputManager.OnMovementControl?.Invoke("g");
        yield return new WaitForSeconds(1);

        //Debug.Log("Case 4 completed: Invalid Inputs.");
    }

    private IEnumerator Case5()
    {
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(0.5f);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        inputManager.OnMovementControl?.Invoke("s");
        yield return new WaitForSeconds(1);

        //Debug.Log("Case 5 completed: Simultaneous Actions");
    }

    private IEnumerator Case6()
    {
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        //Debug.Log("Case 6 completed: Rewind Without Toggle");
    }

    private IEnumerator Case7()
    {
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("s");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("s");
        yield return new WaitForSeconds(1);

        //Debug.Log("Case 7 completed: Invalid Inputs.");
    }
    private IEnumerator Case8()
    {
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        //Debug.Log("Case 8 completed: Invalid Inputs.");
    }

    private IEnumerator Case9()
    {
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("s");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        //Debug.Log("Case 9 completed: Invalid Inputs.");
    }

    private IEnumerator Case10()
    {
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("s");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        //Debug.Log("Case 10 completed: Invalid Inputs.");
    }

    private IEnumerator Case11()
    {
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("s");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        //Debug.Log("Case 11 completed: Invalid Inputs.");
    }

    private IEnumerator Case12()
    {
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("s");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        /*inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);*/

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        //Debug.Log("Case 12 completed: Invalid Inputs.");
    }

    private IEnumerator Case13()
    {
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("s");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("s");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("s");
        yield return new WaitForSeconds(1.5f);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("s");
        yield return new WaitForSeconds(1);

        //Debug.Log("Case 13 completed: Invalid Inputs.");
    }

    private IEnumerator Case14()
    {
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("s");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);

        //Debug.Log("Case 14 completed: Invalid Inputs.");
    }

    private void LogAction(string input)
    {
        Debug.Log($"Action Invoked: {input}");
    }
}
