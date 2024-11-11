using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    private Vector3 hiddenPosition;
    public Vector3 visiblePosition;
    public int turnCycle = 3; // turn interval
    private int turnCount = 0;
    private float moveSpeed = 3.0f;
    private bool isVisible = false;
    
    private void Start()
    {
        hiddenPosition = transform.position;
    }

    public void OnTurnIncrease() // This is called at TurnManager
    {   
        turnCount++;
        if(turnCount == turnCycle)
        {
            StartCoroutine(MoveObstacle(isVisible ? hiddenPosition : visiblePosition));
            isVisible = !isVisible;
            turnCount = 0;
        }
    }

    private void CheckOverlapWithPlayer(Vector3 targetPosition) 
    {
        // Check if player is here by grid coordination
        // This works because if this is "visible", player cannot pass(works like a wall).
        Vector3 playerPosition = PlayerController.playerController.playerCurPos;
        if (Vector3.Distance(playerPosition, targetPosition) <= 0.1f) 
        {
            Debug.Log("Game Over!");
        }
    }

    private IEnumerator MoveObstacle(Vector3 targetPosition)
    {
        // first, check if the position overlaps with player
        CheckOverlapWithPlayer(targetPosition);
        // move the obstacle to the target position
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        
        // And should add dicTurnCheck["Obstacle"] = true; ... something like that here.
        // This should be done at F-023...? I guess.
    }
}