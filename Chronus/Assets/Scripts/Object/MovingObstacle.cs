using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    // Don't forget to add tag "MovingObstacle" at the gameObject!
    private Vector3 hiddenPosition;
    public Vector3 visiblePosition;
    public int turnCycle = 3; // turn interval
    public float moveSpeed = 4.0f;
    private int turnCount = 0;
    private bool isVisible = false;
    public bool isMoveComplete = false;
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
            StartCoroutine(MoveObstacle(isVisible ? hiddenPosition : visiblePosition));
            isVisible = !isVisible;
            turnCount = 0;
        }
        else
        {
            isMoveComplete = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // THIS DOESN'T WORK
        // because the collider is 1x1x1, and Player doesn't move in only horizontal ways.
        if (other.CompareTag("Player"))
        {
            // if current position is not hidden or visible position,
            // then there was a collision at "the middle" of the grid.
            if (transform.position != hiddenPosition && transform.position != visiblePosition)
            {
                Debug.Log("Game Over!");
            }
            Debug.Log($"Player: {other.transform.position}");
            Debug.Log($"Block: {transform.position}");
        }
    }

    private void CheckOverlapWithPlayer(Vector3 targetPosition) 
    {
        // Check if player is "here" by grid coordination
        // This works because if this is "visible", player cannot pass(works like a wall).
        
        Vector3 playerPosition = PlayerController.playerController.playerCurPos;
        //Debug.Log($"Player: {playerPosition}");
        //Debug.Log($"Player+1: {playerPosition????}");
        //Debug.Log($"Block+1: {targetPosition}");
        // 지금 게임 오버 판정이 안되고 있다.
        // 현재 턴+1의 "위치", 즉 현재 턴이 진행된 결과를 받아야 하는데, 지금은 현재 턴의 위치를 가져오고 있다.
        if (Vector3.Distance(playerPosition, targetPosition) <= 0.1f) 
        {
            Debug.Log("Game Over??");
        }
    }

    private IEnumerator MoveObstacle(Vector3 targetPosition)
    {
        // first, check if the target position overlaps with player
        CheckOverlapWithPlayer(targetPosition);
        // move the obstacle to the target position
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
