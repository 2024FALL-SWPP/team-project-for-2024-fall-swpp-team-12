using UnityEngine;

public class Laser : MonoBehaviour
{
    public float laserSpeed = 20f; 
    public float laserLength = 100f; // arbitrary maximum length
    private LineRenderer lineRenderer;
    private int layerMask;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2; // start and end point
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f; // 0.05f;
        layerMask = (1 << 0) | (1 << 3) | (1 << 7) | (1 << 8); //default and LeverStick
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

        if (Physics.Raycast(start, direction, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            // stop at the collision(hit) point 
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, hit.point);
            if (TurnManager.turnManager.CLOCK && hit.collider.CompareTag("Player"))
            {
                if (!PlayerController.playerController.isBlinking && hit.collider.name == "Player" && !PlayerController.playerController.willLaserKillCharacter)
                {
                    PlayerController.playerController.willLaserKillCharacter = true;
                }
                if (hit.collider.name == "Phantom" && !PhantomController.phantomController.willLaserKillCharacter)
                {
                    PhantomController.phantomController.willLaserKillCharacter = true;
                }
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