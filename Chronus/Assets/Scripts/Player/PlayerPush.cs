using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    public static PlayerPush playerPush;

    public float pushDistance = 2f;
    public float jumpHeight = 2f;
    public float heightOffset = 0.5f; 
    private bool isOnBox = false;
    private GameObject currentBox = null;

    private void Awake() //singleton
    {
        if (PlayerPush.playerPush == null) { PlayerPush.playerPush = this; }
    }

    private void Start()
    {
        
    }
    private void Update()
    {

    }

    public void TryPush(Vector3 direction, RaycastHit hit)
    {
        if (Physics.Raycast(transform.position, direction, out hit, pushDistance))
        {
            GameObject box = hit.collider.gameObject;
            if (box != null && box.CompareTag("Box"))
            {
                if (Physics.Raycast(box.transform.position, direction, out RaycastHit obstacleHit, pushDistance))
                {
                    if (obstacleHit.collider.CompareTag("Wall"))
                    {
                        StartCoroutine(JumpOntoBox(box));
                        return;
                    }
                }

                Rigidbody rb = box.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.MovePosition(box.transform.position + direction * pushDistance);
                    
                    TurnManager.turnManager.dicTurnCheck["Box"] = true;
                }
            }
        }
    }

    private IEnumerator JumpOntoBox(GameObject box)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = box.transform.position + Vector3.up * (jumpHeight + heightOffset);
        float jumpSpeed = 3f;

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
            elapsedTime += Time.deltaTime * jumpSpeed;
            yield return null;
        }

        transform.position = targetPosition; 
        isOnBox = true;
        currentBox = box;
    }
}
