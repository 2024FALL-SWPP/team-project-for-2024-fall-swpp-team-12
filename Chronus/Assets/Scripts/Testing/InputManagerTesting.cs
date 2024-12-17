using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManagerTesting : MonoBehaviour
{
    public static InputManagerTesting instance;
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
        if (instance == null)  instance = this;
        else Destroy(gameObject); 

        // assure that both InputManager and TurnManager are present
        inputManager = InputManager.inputManager;
        turnManager = TurnManager.turnManager;

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
            { "Player", new Vector3(1, 1, 1) },
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
        if (!result) failedCases.Add(caseNumber);

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

    private IEnumerator WaitForTurnEnd()
    {
        while (turnManager.CLOCK)
        {
            yield return null;
        }
    }

    private void InvokeInput(string input)
    {
        switch (input) // no undo
        {
            case "w": inputManager.OnMovementControl?.Invoke("w"); break;
            case "a": inputManager.OnMovementControl?.Invoke("a"); break;
            case "s": inputManager.OnMovementControl?.Invoke("s"); break;
            case "d": inputManager.OnMovementControl?.Invoke("d"); break;
            case "r": inputManager.OnMovementControl?.Invoke("r"); break;
            case "q": inputManager.OnTimeRewindControl?.Invoke("q"); break;
            case "e": inputManager.OnTimeRewindControl?.Invoke("e"); break;
            case " ": inputManager.OnTimeRewindModeToggle?.Invoke(); break; // Space
            default: break;
        }
    }

    private IEnumerator ExecuteInputSequence(List<string> inputSequence)
    {
        foreach (var input in inputSequence)
        {
            InvokeInput(input); 
            yield return WaitForTurnEnd(); 
        }
    }

    private IEnumerator Case1()
    {
        yield return new WaitForSeconds(2);
        List<string> inputs = new() { "d", "w", "a", " ", "q", "q", "q", " ", "r", "r", "w" };
        yield return ExecuteInputSequence(inputs);
    }

    private IEnumerator Case2()
    {
        yield return new WaitForSeconds(2);
        List<string> inputs = new() { "d", "w", "w", "a", "s", " ", "q", "q", "q", "q", " ", "r", "r", "r", "a" };
        yield return ExecuteInputSequence(inputs);
    }

    private IEnumerator Case3()
    {
        yield return new WaitForSeconds(2);
        List<string> inputs = new() { "d", "w", "w", "a", " ", "q", "q", "q", "q", " ", "r", "r", "r", "w" };
        yield return ExecuteInputSequence(inputs);
    }

    private IEnumerator Case4()
    {
        yield return new WaitForSeconds(2);
        List<string> inputs = new() { "g", "h", "d", "g" };
        yield return ExecuteInputSequence(inputs);
    }

    private IEnumerator Case5()
    {
        yield return new WaitForSeconds(1);
        List<string> inputs = new() { "d", " ", "w", "q", " ", "s" };
        yield return ExecuteInputSequence(inputs);
    }

    private IEnumerator Case6()
    {
        yield return new WaitForSeconds(1);
        List<string> inputs = new() { "q", "q" };
        yield return ExecuteInputSequence(inputs);
    }

    private IEnumerator Case7()
    {
        yield return new WaitForSeconds(2);
        List<string> inputs = new() { "a", "a", "w", "d", "w", "d", "s", "s" };
        yield return ExecuteInputSequence(inputs);
    }

    private IEnumerator Case8()
    {
        yield return new WaitForSeconds(2);
        List<string> inputs = new() { "a", "a", "w", "d", "d", " ", "q", "q", "q", "q", "q", " ", "r", "r", "r", "r", "w" };
        yield return ExecuteInputSequence(inputs);
    }

    private IEnumerator Case9()
    {
        yield return new WaitForSeconds(2);
        List<string> inputs = new() { "a", "a", "w", "d", "s", "d", "r", "r", "r", "w", " ", "q", "q", "q", "q", " ", "d", "w", "w", "a" };
        yield return ExecuteInputSequence(inputs);
    }

    private IEnumerator Case10()
    {
        yield return new WaitForSeconds(2);
        List<string> inputs = new() { "a", "a", "w", "d", "s", "d", "d", "w", "w", "a", " ", "q", "q", "q", "q", " ", "r", "r", "r", "w" };
        yield return ExecuteInputSequence(inputs);
    }

    private IEnumerator Case11()
    {
        yield return new WaitForSeconds(2);
        List<string> inputs = new() { "a", "a", "w", "d", "s", "d", "w", "d", "d", " ", "q", "q", " ", "r", "w" };
        yield return ExecuteInputSequence(inputs);
    }

    private IEnumerator Case12()
    {
        yield return new WaitForSeconds(2);
        List<string> inputs = new() { "a", "a", "w", "d", "s", "d", "w", "d", "w", "d", " ", "q", "q", "q", " ", "w", "r", "w", "r", "r" };
        yield return ExecuteInputSequence(inputs);
    }

    private IEnumerator Case13()
    {
        yield return new WaitForSeconds(2);
        List<string> inputs = new() { "a", "a", "w", "d", "s", "d", "w", "s", "a", "a", "w", "w", "d", "w", "w", "d", "s", "d", "a", "s" };
        yield return ExecuteInputSequence(inputs);
    }

    private IEnumerator Case14()
    {
        yield return new WaitForSeconds(2);
        List<string> inputs = new() { "a", "a", "w", "d", "s", "d", "r", "r", "r", "r", "w", " ", "q", "q", "q", "q", "q", " ", "a", "a", "w", "d", "d", "d" };
        yield return ExecuteInputSequence(inputs);
    }

    private void LogAction(string input)
    {
        Debug.Log($"Action Invoked: {input}");
    }
}
