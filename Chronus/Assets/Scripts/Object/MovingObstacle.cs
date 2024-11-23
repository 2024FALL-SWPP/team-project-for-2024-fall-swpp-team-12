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
    public TurnLogIterator<(Vector3, bool, int)> positionIterator;

    public bool isDependantOnSwitch = false;

    private void Start()
    {
        hiddenPosition = transform.position;
        InitializeLog();
    }

    public void InitializeLog() 
    {
        var initialPositionLog = new List<(Vector3, bool, int)> { (transform.position, isVisible, turnCount ) };
        positionIterator = new TurnLogIterator<(Vector3, bool, int)>(initialPositionLog);
    }

    public void AdvanceTurn()
    {
        if (!isDependantOnSwitch) turnCount++; //count cycle by itself
        if (turnCount == turnCycle)
        {
            Invoke("Move", 0.2f);
            isVisible = !isVisible;
            turnCount = 0;
        }
        else
        {
            isMoveComplete = true;
        }
    }

    private void Move() 
    { 
        Vector3 targetPosition = isVisible ? hiddenPosition : visiblePosition; 
        CheckOverlapWithCharacters(targetPosition);
        StartCoroutine(MoveObstacle(targetPosition));
    }

    private void CheckOverlapWithCharacters(Vector3 targetPosition)
    {
        direction = (targetPosition - transform.position).normalized;
        // first, check if the target position overlaps with player's target position.
        Vector3 playerTargetPosition = PlayerController.playerController.targetTranslation;
        if (playerTargetPosition == targetPosition)
        {   
            // If this is a block: going to push the player
            PlayerController.playerController.pushDirection = direction;
            PlayerController.playerController.pushSpeed = moveSpeed;
            PlayerController.playerController.targetTranslation = targetPosition + direction * 2.0f;
            // Else, if this is a spear: just game over.
        }
        if (PhantomController.phantomController.isPhantomExisting)
        {
            Vector3 phantomTargetPosition = PhantomController.phantomController.targetTranslation;
            if (phantomTargetPosition == targetPosition)
            {
                // If this is a block: going to push the phantom
                PhantomController.phantomController.pushDirection = direction;
                PhantomController.phantomController.pushSpeed = moveSpeed;
                PhantomController.phantomController.targetTranslation = targetPosition + direction * 2.0f;
                // Else, if this is a spear: kill it.
            }
        }
    }

    private IEnumerator MoveObstacle(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f && Vector3.Distance(transform.position, startPosition) < 2.0f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
        isMoveComplete = true;
    }

    public void SaveCurrentPos()
    {
        positionIterator.Add((transform.position, isVisible, turnCount));
    }
    public void RemoveLog(int k)
    {
        positionIterator.RemoveLastK(k);
    }
    public void RestoreState()
    {
        var current = positionIterator.Current;
        transform.position = current.Item1;
        isVisible = current.Item2;
        turnCount = current.Item3;
    }
}
