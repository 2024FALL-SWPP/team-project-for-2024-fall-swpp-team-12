using System.Collections;
using UnityEngine;

public class InputManagerTesting : MonoBehaviour
{
    public static InputManagerTesting instance; //singleton
    private InputManager inputManager;

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

        StartCoroutine(RunTests());
    }

    private IEnumerator RunTests()
    {
        inputManager.OnMovementControl += LogAction;
        inputManager.OnTimeRewindControl += LogAction;

        if (case1)
        {
            yield return Case1();
        }

        if (case2)
        {
            yield return Case2();
        }

        if (case3)
        {
            yield return Case3();
        }

        if (case4)
        {
            yield return Case4();
        }

        if (case5)
        {
            yield return Case5();
        }

        if (case6)
        {
            yield return Case6();
        }

        if (case7)
        {
            yield return Case7();
        }
        
        if (case8)
        {
            yield return Case8();
        }
        
        if (case9)
        {
            yield return Case9();
        }
        
        if (case10)
        {
            yield return Case10();
        }
        
        if (case11)
        {
            yield return Case11();
        }
        
        if (case12)
        {
            yield return Case12();
        }
        
        if (case13)
        {
            yield return Case13();
        }
        
        if (case14)
        {
            yield return Case14();
        }

        inputManager.OnMovementControl -= LogAction;
        inputManager.OnTimeRewindControl -= LogAction;

        Debug.Log("All selected tests completed successfully.");
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

        Debug.Log("Case 4 completed: Invalid Inputs.");
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

        Debug.Log("Case 5 completed: Simultaneous Actions");
    }

    private IEnumerator Case6()
    {
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(1);

        Debug.Log("Case 6 completed: Rewind Without Toggle");
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

        Debug.Log("Case 7 completed: Invalid Inputs.");
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

        Debug.Log("Case 8 completed: Invalid Inputs.");
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
        
        Debug.Log("Case 9 completed: Invalid Inputs.");
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
        
        Debug.Log("Case 10 completed: Invalid Inputs.");
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
        
        Debug.Log("Case 11 completed: Invalid Inputs.");
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
        
        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);
        
        inputManager.OnMovementControl?.Invoke("w");
        yield return new WaitForSeconds(1);
        
        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);
        
        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1);
        
        Debug.Log("Case 12 completed: Invalid Inputs.");
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
        yield return new WaitForSeconds(1);
        
        inputManager.OnMovementControl?.Invoke("d");
        yield return new WaitForSeconds(1);
        
        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(1);
        
        inputManager.OnMovementControl?.Invoke("s");
        yield return new WaitForSeconds(1);
        
        Debug.Log("Case 13 completed: Invalid Inputs.");
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
        
        Debug.Log("Case 14 completed: Invalid Inputs.");
    }
    
    private void LogAction(string input)
    {
        Debug.Log($"Action Invoked: {input}");
    }
}
