using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingWindow : MonoBehaviour
{
    public Vector3 hiddenPosition;
    public Vector3 visiblePosition;
    public float moveSpeed = 2.0f;
    private bool isVisible = false;
    private int tickCounter = 0;
    private Collider windowCollider;

    private void OnEnable()
    {
        TickManager.OnTick += OnTickEvent;
    }

    private void OnDisable()
    {
        TickManager.OnTick -= OnTickEvent;
    }

    private void Start()
    {
        transform.position = hiddenPosition;
        windowCollider = GetComponent<Collider>();

        if (windowCollider != null)
        {
            windowCollider.enabled = false;
        }
    }

    private void OnTickEvent()
    {
        tickCounter++;

        if(tickCounter % 3 == 0)
        {
            isVisible = !isVisible;
            StartCoroutine(MoveWindow(isVisible ? visiblePosition : hiddenPosition));
        }
    }

    private IEnumerator MoveWindow(Vector3 targetPosition)
    {
        if (windowCollider != null)
        {
            windowCollider.enabled = true;
        }

        // move window to the target position
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        
        transform.position = targetPosition;

        if (windowCollider != null)
        {
            windowCollider.enabled = targetPosition == visiblePosition;
        }
    }
}