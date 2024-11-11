using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    public static PlayerPush playerPush;

    public float pushDistance = 2f;
    public float jumpHeight = 2f;
    public float heightOffset = 0.5f;

    public bool canPushBox = false;

    private void Awake() //singleton
    {
        if (PlayerPush.playerPush == null) { PlayerPush.playerPush = this; }
    }

    private void Start()
    {
        
    }
    private void Update()
    {/*
        if (Input.GetKeyDown(KeyCode.W)) TryPush(Vector3.forward);
        if (Input.GetKeyDown(KeyCode.S)) TryPush(Vector3.back);
        if (Input.GetKeyDown(KeyCode.A)) TryPush(Vector3.left);
        if (Input.GetKeyDown(KeyCode.D)) TryPush(Vector3.right);
        */
    }

    public void TryPush(Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, pushDistance))
        {
            GameObject box = hit.collider.gameObject;
            if (box != null && box.CompareTag("Box"))
            {
                if (Physics.Raycast(box.transform.position, direction, out RaycastHit obstacleHit, pushDistance)) //cannot push box
                {
                    Debug.Log("cannotpush");
                    //StartCoroutine(JumpOntoBox(box));
                    canPushBox = false;
                }
                else //can push box
                {
                    Rigidbody rb = box.GetComponent<Rigidbody>();
                    //smoothly move
                    if (Physics.Raycast(box.transform.position, transform.up, out RaycastHit playerHit, pushDistance))
                    {
                        if (playerHit.collider.CompareTag("Player")) //need for phantom too *****
                        {
                            playerHit.collider.gameObject.GetComponent<Transform>().transform.position += direction * pushDistance;
                            PlayerController.playerController.playerCurPos = playerHit.collider.gameObject.GetComponent<Transform>().transform.position;
                        }
                    }
                    rb.MovePosition(box.transform.position + direction * pushDistance);
                    canPushBox = true;
                }
            }
        }
    }

    //don't use the function below, but it is a good reference of player hop, need to fix that
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
        
        canPushBox = false;
    }
}
