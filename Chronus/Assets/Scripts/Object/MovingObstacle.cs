using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    private Vector3 hiddenPosition;
    private Vector3 originalHiddenPosition; // to cope with offset changes by scene loading
    public Vector3 visiblePosition;
    private Vector3 direction; // moving direction
    public int turnCycle = 3; // turn interval
    public float moveSpeed = 7.0f;
    private int turnCount = 0;
    // for time rewind
    private bool isVisible = false;
    public bool isMoveComplete = false;
    public TurnLogIterator<(Vector3, bool, int)> positionIterator;
    private void Awake()
    {
        originalHiddenPosition = transform.position;
    }

    private void Start()
    {
        hiddenPosition = transform.position;
        InitializeLog();
        if (hiddenPosition != originalHiddenPosition) 
        {
            visiblePosition += hiddenPosition - originalHiddenPosition; // adding offset
        }
    }

    public void InitializeLog()
    {
        var initialPositionLog = new List<(Vector3, bool, int)> { (transform.position, isVisible, turnCount) };
        positionIterator = new TurnLogIterator<(Vector3, bool, int)>(initialPositionLog);
    }
    public void ResetToStart()
    {
        positionIterator.ResetToStart();
        RestoreState();
    }

    public void AdvanceTurn()
    {
        if (gameObject.activeSelf) turnCount++;
        if (turnCount == turnCycle)
        {
            Invoke("Move", 0.15f);
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
        HandlePush(PlayerController.playerController, targetPosition);
        if (PhantomController.phantomController.isPhantomExisting) HandlePush(PhantomController.phantomController, targetPosition);
    }
    
    // refactoring:
    private void HandlePush(CharacterBase character, Vector3 targetPosition)
    // check if the target position overlaps with a character's target position. if so, push it.
    {
        Vector3 targetTranslation = character.targetTranslation;
        if (targetTranslation.x == targetPosition.x && targetTranslation.z == targetPosition.z &&
            targetTranslation.y >= targetPosition.y - 1 && targetTranslation.y <= targetPosition.y + 1)
        {
            character.pushDirection = direction;
            character.pushSpeed = moveSpeed;
            character.targetTranslation = new Vector3(
                targetPosition.x + direction.x * 2,
                targetTranslation.y,
                targetPosition.z + direction.z * 2);
        }
    }

    private IEnumerator MoveObstacle(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float maxGap = Vector3.Distance(targetPosition, startPosition);
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f && Vector3.Distance(transform.position, startPosition) < maxGap)
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
