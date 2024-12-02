using System.Collections;
using UnityEngine;

public class InputManagerTesting : MonoBehaviour
{
    private InputManager inputManager;
    
    public bool case1 = false;
    public bool case2 = false;
    public bool case3 = false;

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
    
    private void LogAction(string input)
    {
        Debug.Log($"Action Invoked: {input}");
    }
}
