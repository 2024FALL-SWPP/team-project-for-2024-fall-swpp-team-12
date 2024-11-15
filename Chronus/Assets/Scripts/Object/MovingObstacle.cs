using System.Collections;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    // Don't forget to add tag "Obstacle" at the gameObject!
    private Vector3 hiddenPosition;
    public Vector3 visiblePosition;
    public int turnCycle = 3; // turn interval
    private int turnCount = 0;
    private float moveSpeed = 3.0f;
    private bool isVisible = false;
    public bool isMoveComplete = false;

    // Need to implement Log for time rewind <<<<<<<<<<<<<<<<<<

    private void Start()
    {
        hiddenPosition = transform.position;
    }

    public void AdvanceTurn() 
    {   
        turnCount++;
        if (turnCount == turnCycle)
        {
            StartCoroutine(MoveObstacle(isVisible ? hiddenPosition : visiblePosition));
            isVisible = !isVisible;
            turnCount = 0;
        }
        else
        {
            isMoveComplete = true;
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

        isMoveComplete = true;
    }
}
