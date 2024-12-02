using System.Collections;
using UnityEngine;

public class InputManagerTesting : MonoBehaviour
{
    private InputManager inputManager;
    
    public bool case1 = false; // Pushing same box to different direction (-> + ^)
    public bool case2 = false; // Box target is the same as player target
    public bool case3 = false; // Box target is the same as clone target
    public bool case4 = false; // Invalid + Valid inputs
    public bool case5 = false; // Simultaneous Actions
    public bool case6 = false; // Time rewind without Space Toggle

    private void Start()
    {
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
    
    private void LogAction(string input)
    {
        Debug.Log($"Action Invoked: {input}");
    }
}
