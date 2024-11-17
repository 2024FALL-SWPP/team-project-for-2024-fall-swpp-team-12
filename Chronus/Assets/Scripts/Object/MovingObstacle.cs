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
    public float moveSpeed = 4.0f;
    private int turnCount = 0;
    // for time rewind
    private bool isVisible = false;
    public bool isMoveComplete = false;
    private bool isPlayerPushed = false, isObstacleMoved = false; // internal flags to check isMoveComplete.
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

    private void StartMove() { StartCoroutine(Move(isVisible ? hiddenPosition : visiblePosition)); }

    private IEnumerator Move(Vector3 targetPosition)
    {
        isPlayerPushed = false;
        isObstacleMoved = false;
        direction = (targetPosition - transform.position).normalized;
        // first, check if the target position overlaps with player's target position.
        Vector3 playerTargetPosition = PlayerController.playerController.targetTranslation;
        Debug.Log($"Player: {PlayerController.playerController.playerCurPos}");
        Debug.Log($"Player+1: {playerTargetPosition}");
        Debug.Log($"Block+1: {targetPosition}");
        if (playerTargetPosition == targetPosition)
        {
            // If this is a block: going to push the player
            StartCoroutine(PushPlayer(PlayerController.playerController, targetPosition));
            // Else, if this is a spear: just game over.
        }
        else isPlayerPushed = true;

        // move the obstacle to the target position
        StartCoroutine(MoveObstacle(targetPosition));

        while (!isPlayerPushed || !isObstacleMoved) yield return null;
        isMoveComplete = true;
    }

    private IEnumerator PushPlayer(PlayerController player, Vector3 targetPosition)
    {
        Vector3 pushedPosition = targetPosition + direction * 2.0f;
        // the case when player and obstacle meeting like this (opposite) -> <-
        // -direction == PlayerController.playerController.targetDirection
        // this works poorly. 

        while (Vector3.Distance(player.transform.position, pushedPosition) > 0.01f)
        {
            Vector3 moveVector = (pushedPosition - player.transform.position).normalized * moveSpeed * Time.deltaTime;
            player.transform.Translate(moveVector, Space.World);

            yield return null;
        }
        /*while (Vector3.Distance(player.transform.position, pushedPosition) > 0.01f)
            {
                player.transform.position = Vector3.MoveTowards(
                    player.transform.position,
                    pushedPosition,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }*/

        player.transform.position = pushedPosition;
        player.playerCurPos = pushedPosition;
        isPlayerPushed = true;
    }

    private IEnumerator MoveObstacle(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
        isObstacleMoved = true;
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
