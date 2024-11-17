using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    // Don't forget to add tag "MovingObstacle" at the gameObject!
    private Vector3 hiddenPosition;
    public Vector3 visiblePosition;
    private Vector3 direction; // moving direction
    public int turnCycle = 3; // turn interval
    public float moveSpeed = 7.0f;
    private int turnCount = 0;
    // for time rewind
    private bool isVisible = false;
    public bool isMoveComplete = false;
    private bool isObstacleMoved = false; // internal flags to check isMoveComplete.
    public List<Vector3> listObstPosLog;

    private void Start()
    {
        hiddenPosition = transform.position;
        listObstPosLog = new List<Vector3>() { transform.position };
    }

    public void AdvanceTurn()
    {
        turnCount++;
        if (turnCount == turnCycle)
        {   
            //StartCoroutine(Move(isVisible ? hiddenPosition : visiblePosition));
            Invoke("StartMove", 0.2f); // This is not great, but temporary.
            isVisible = !isVisible;
            turnCount = 0;
        }
        else
        {
            isMoveComplete = true;
        }
    }

    private void StartMove() { Move(isVisible ? hiddenPosition : visiblePosition); }

    private void Move(Vector3 targetPosition)
    {
        isObstacleMoved = false;
        direction = (targetPosition - transform.position).normalized;
        // first, check if the target position overlaps with player's target position.
        Vector3 playerTargetPosition = PlayerController.playerController.targetTranslation;
        if (playerTargetPosition == targetPosition)
        {
            // If this is a block: going to push the player
            PlayerController.playerController.pushDirection = direction;
            PlayerController.playerController.targetTranslation = targetPosition + direction * 2.0f;
            // Else, if this is a spear: just game over.
        }
        if (PhantomController.phantomController.isPhantomExisting)
        {
            Vector3 phantomTargetPosition = PlayerController.playerController.targetTranslation;
            if (phantomTargetPosition == targetPosition)
            {
                // If this is a block: going to push the phantom
                PhantomController.phantomController.pushDirection = direction;
                PhantomController.phantomController.targetTranslation = targetPosition + direction * 2.0f;
                // Else, if this is a spear: kill it.
            }
        }

        // move the obstacle to the target position
        StartCoroutine(MoveObstacle(targetPosition));
    }

    private IEnumerator MoveObstacle(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
        isMoveComplete = true;
    }

    public void SaveCurrentPos()
    {
        listObstPosLog.Add(transform.position);
    }
    public void RemoveLog(int startIndex)
    {
        listObstPosLog.RemoveRange(startIndex + 1, listObstPosLog.Count - startIndex - 1);
    }
    public void RestorePos(int turn)
    {
        transform.position = listObstPosLog[turn];
        isVisible = transform.position != hiddenPosition;
    }
}
