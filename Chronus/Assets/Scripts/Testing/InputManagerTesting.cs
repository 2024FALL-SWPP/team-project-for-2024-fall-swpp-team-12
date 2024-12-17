using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManagerTesting : MonoBehaviour
{
    public static InputManagerTesting instance;
    private InputManager inputManager;
    private TurnManager turnManager;
    public PlayerController player;
    private Dictionary<int, Dictionary<string, Vector3>> expectedPositions = new();
    private List<int> failedCases = new();
    private List<List<string>> testCaseInputs = new()
    {
        new List<string>(), 
        new List<string> { "d", "w", "a", " ", "q", "q", "q", " ", "r", "r", "w" }, // Case1: Pushing same box to different direction (-> + ^)
        new List<string> { "d", "w", "w", "a", "s", " ", "q", "q", "q", "q", " ", "r", "r", "r", "a" }, // Case2: Box target is the same as player target
        new List<string> { "d", "w", "w", "a", " ", "q", "q", "q", "q", " ", "r", "r", "r", "w" }, // Case3: Box target is the same as clone target
        new List<string> { "g", "h", "d", "g" }, // Case4: Invalid + Valid inputs
        new List<string> { "q", "q" }, // Case5: Time rewind without Space Toggle
        new List<string> { "a", "a", "w", "d", "w", "d", "s", "s" }, // Case6: Pushing Stacked Boxes + Stacked Boxes Fall
        new List<string> { "a", "a", "w", "d", "d", " ", "q", "q", "q", "q", "q", " ", "r", "r", "r", "r", "w" }, // Case7: Splitting Two boxes + Stacked boxes pushed in different directions
        new List<string> { "a", "a", "w", "d", "s", "d", "r", "r", "r", "w", " ", "q", "q", "q", "q", " ", "d", "w", "w", "a" }, // Case8: Stacked Box target is the same as player target
        new List<string> { "a", "a", "w", "d", "s", "d", "d", "w", "w", "a", " ", "q", "q", "q", "q", " ", "r", "r", "r", "w" }, // Case9: Stacked Box target is the same as clone target
        new List<string> { "a", "a", "w", "d", "s", "d", "w", "d", "d", " ", "q", "q", " ", "r", "w" }, // Case10: Stacked Boxes drop when tile disappears at the same time (tile initially active)
        new List<string> { "a", "a", "w", "d", "s", "d", "w", "d", "w", "d", " ", "q", "q", "q", " ", "w", "r", "w", "r", "r" }, // Case11: Stacked Boxes drop when tile disappears at the same time (tile initially inactive)
        new List<string> { "a", "a", "w", "d", "s", "d", "w", "s", "a", "a", "w", "w", "d", "w", "w", "d", "s", "d", "a", "s" }, // Case12: Stack 1 more box on top of 2 boxes and move 3 boxes together
        new List<string> { "a", "a", "w", "d", "s", "d", "r", "r", "r", "r", "w", " ", "q", "q", "q", "q", "q", " ", "a", "a", "w", "d", "d", "d" } // Case13: Top box moved by main character and bottom by phantom
    };

    private void Start()
    {
        if (instance == null) instance = this;
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
            { "Player", new Vector3(1, 1, -1) },
            { "boxA", new Vector3(1, 1, 1) },
            { "boxA (1)", new Vector3(-1, 4, 1) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };

        expectedPositions[6] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, -1) },
            { "boxA", new Vector3(1, -10.1834f, -3) },
            { "boxA (1)", new Vector3(-1, -10.03161f, -3) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };

        expectedPositions[7] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, 1) },
            { "Phantom", new Vector3(1,1,1)},
            { "boxA", new Vector3(1, 1, 3) },
            { "boxA (1)", new Vector3(1, 3, 3) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };

        expectedPositions[8] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, 3) },
            { "Phantom", new Vector3(1,1,-1)},
            { "boxA", new Vector3(1, 1, 1) },
            { "boxA (1)", new Vector3(1, 3, 1) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };

        expectedPositions[9] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, 1) },
            { "boxA", new Vector3(1, 1, 3) },
            { "boxA (1)", new Vector3(1, 3, 3) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };

        expectedPositions[10] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, 3) },
            { "Phantom", new Vector3(5,1,1)},
            { "boxA", new Vector3(1, -10.1834f, 5) },
            { "boxA (1)", new Vector3(1, -10.03161f, 5) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };

        expectedPositions[11] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, 5) },
            { "boxA", new Vector3(1, -10.1834f, 7) },
            { "boxA (1)", new Vector3(1, -10.03161f, 7) },
            { "boxA (3)", new Vector3(1, 7, 5) }
        };

        expectedPositions[12] = new Dictionary<string, Vector3>
        {
            { "Player", new Vector3(1, 1, 3) },
            { "boxA", new Vector3(1, 1, 1) },
            { "boxA (1)", new Vector3(1, 3,1) },
            { "boxA (3)", new Vector3(1, 5, 1) }
        };

        expectedPositions[13] = new Dictionary<string, Vector3>
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

        for (int i = 1; i <= 13; i++)
        {
            Debug.Log($"Starting Test Case {i}...");
            yield return RunTestCase(i);
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

    private IEnumerator RunTestCase(int caseNumber)
    {
        Debug.Log($"Running Test Case {caseNumber}...");
        yield return new WaitForSeconds(2);
        yield return ExecuteInputSequence(testCaseInputs[caseNumber]);

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
    
    private void LogAction(string input)
    {
        Debug.Log($"Action Invoked: {input}");
    }
}
