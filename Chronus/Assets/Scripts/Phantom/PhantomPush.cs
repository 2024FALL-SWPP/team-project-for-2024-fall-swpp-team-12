using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhantomPush : MonoBehaviour
{
    public static PhantomPush phantomPush;

    public float pushDistance = 2f;
    public float jumpHeight = 2f;
    public float heightOffset = 0.5f;

    public bool canPushBox = false;
    public bool canRideBox = false;

    private int layerMask;

    private void Awake() //singleton
    {
        if (PhantomPush.phantomPush == null) { PhantomPush.phantomPush = this; }
    }

    private void Start()
    {
        layerMask = 1 << 0;
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
        if (Physics.Raycast(transform.position, direction, out hit, pushDistance, layerMask))
        {
            GameObject box = hit.collider.gameObject;
            if (box != null && box.CompareTag("Box"))
            {
                if (Physics.Raycast(box.transform.position, direction, out RaycastHit obstacleHit, pushDistance, layerMask)) //cannot push box
                {
                    Debug.Log("cannotpush");
                    //StartCoroutine(JumpOntoBox(box));
                    canRideBox = false;
                    canPushBox = false;
                }
                else //can push box
                {
                    Rigidbody rb = box.GetComponent<Rigidbody>();
                    //smoothly move
                    if (Physics.Raycast(box.transform.position, transform.up, out RaycastHit playerHit, pushDistance, layerMask))
                    {
                        if (playerHit.collider.CompareTag("Player"))
                        {
                            canRideBox = true;
                            playerHit.collider.gameObject.GetComponent<Transform>().transform.position += direction * pushDistance;
                            PlayerController.playerController.playerCurPos = playerHit.collider.gameObject.GetComponent<Transform>().transform.position;
                        }
                        else if (playerHit.collider.CompareTag("Phantom")) 
                        {
                            canRideBox = false;
                            // kill phantom.
                        }
                        else
                        {
                            canRideBox = false;
                        }
                    }
                    else
                    {
                        canRideBox = false;
                    }
                    rb.MovePosition(box.transform.position + direction * pushDistance); //statemachine replace is neeeeded
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
