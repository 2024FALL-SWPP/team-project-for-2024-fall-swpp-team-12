using System.Collections;
using UnityEngine;

public class InputManagerTesting : MonoBehaviour
{
    private InputManager inputManager;

    private void Start()
    {
        inputManager = InputManager.inputManager;
        if (inputManager == null)
        {
            return;
        }

        StartCoroutine(TestCornerCase());
    }

    private IEnumerator TestCornerCase()
    {
        inputManager.OnMovementControl += LogAction;
        inputManager.OnTimeRewindControl += LogAction;
        yield return new WaitForSeconds(2);
        
        inputManager.OnMovementControl?.Invoke("d"); 
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("w"); 
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("w"); 
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("a"); 
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("s"); 
        yield return new WaitForSeconds(2);
        
        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(2);

        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(2);
        
        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(2);
        
        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(2);
        
        inputManager.OnTimeRewindControl?.Invoke("q");
        yield return new WaitForSeconds(2);
        
        inputManager.OnTimeRewindModeToggle?.Invoke();
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(2);
        
        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(2);
        
        inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(2);
        
        inputManager.OnMovementControl?.Invoke("a");
        yield return new WaitForSeconds(2);

        inputManager.OnMovementControl -= LogAction;
        inputManager.OnTimeRewindControl -= LogAction;

        Debug.Log("Test completed successfully.");
    }
    
    private void LogAction(string input)
    {
        Debug.Log($"Action Invoked: {input}");
    }
}
