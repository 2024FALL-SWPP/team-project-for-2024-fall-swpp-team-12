using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    public float pushDistance = 2f;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) TryPush(Vector3.forward);
        if (Input.GetKeyDown(KeyCode.S)) TryPush(Vector3.back);
        if (Input.GetKeyDown(KeyCode.A)) TryPush(Vector3.left);
        if (Input.GetKeyDown(KeyCode.D)) TryPush(Vector3.right);
    }

    private void TryPush(Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, pushDistance))
        {
            GameObject box = hit.collider.gameObject;
            if (box != null && box.CompareTag("Box"))
            {   
                if (Physics.Raycast(box.transform.position, direction, out RaycastHit obstacleHit, pushDistance))
                {
                    if (obstacleHit.collider.CompareTag("Obstacle"))
                    {
                        return;
                    }
                }
                
                Rigidbody rb = box.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.MovePosition(box.transform.position + direction * pushDistance);
                }
            }
        }
    }
}
