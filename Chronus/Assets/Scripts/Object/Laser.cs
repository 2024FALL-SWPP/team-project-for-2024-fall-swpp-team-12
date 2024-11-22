using UnityEngine;

public class Laser : MonoBehaviour
{
    public float laserSpeed = 20f; 
    public float laserLength = 100f; // arbitrary maximum length
    private LineRenderer lineRenderer; 

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2; // start and end point
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f; // 0.05f;
    }

    public void ToggleLaser()
    {
        // turn off if on, turn on if off
        if (gameObject.activeSelf) {gameObject.SetActive(false);}
        else {gameObject.SetActive(true);}
    }

    void Update()
    {
        CastLaser();
    }

    void CastLaser()
    {
        Vector3 start = transform.position;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(start, direction, out RaycastHit hit, Mathf.Infinity))
        {
            // stop at the collision(hit) point 
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, hit.point);
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("Game Over!");
            }
        }
        else
        {
            // if there's no collision, it goes infinitely 
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, start + direction * laserLength);
        }
    }
}