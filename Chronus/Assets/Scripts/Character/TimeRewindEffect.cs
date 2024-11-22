using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeRewindEffect : MonoBehaviour
{
    public ParticleSystem newPositionEffect;
    public ParticleSystem surroundEffect;
    public float cooldown = 1f;

    private Renderer playerRenderer;
    private bool isRewinding = false;
    private bool isRewindModeActive = false;
    private float lastAbilityTime = 0f;

    private ParticleSystem activeSurroundEffect;
    private ParticleSystem activeNewPositionEffect;

    void Start()
    {
        playerRenderer = GetComponentInChildren<Renderer>();

        if (surroundEffect != null)
        {
            activeSurroundEffect = Instantiate(surroundEffect, transform.position, Quaternion.identity);
            activeSurroundEffect.transform.SetParent(transform);
            activeSurroundEffect.Stop();
        }

        if (newPositionEffect != null)
        {
            activeNewPositionEffect = Instantiate(newPositionEffect, transform.position, Quaternion.identity);
            activeNewPositionEffect.Stop();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleRewindMode();
        }

        if (isRewindModeActive && Time.time >= lastAbilityTime + cooldown)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Rewind();
                lastAbilityTime = Time.time;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                FastForward();
                lastAbilityTime = Time.time;
            }
        }

        if (activeSurroundEffect != null && activeSurroundEffect.isPlaying)
        {
            activeSurroundEffect.transform.position = transform.position;
        }
    }

    void ToggleRewindMode()
    {
        isRewindModeActive = !isRewindModeActive;

        if (isRewindModeActive)
        {
            if (activeSurroundEffect != null && !activeSurroundEffect.isPlaying)
            {
                activeSurroundEffect.Play();
            }
        }
        else
        {
            if (activeSurroundEffect != null)
            {
                activeSurroundEffect.Stop();
                activeSurroundEffect.Clear();
            }
        }
    }

    void Rewind()
    {
        if (isRewinding || !isRewindModeActive) return;

        isRewinding = true;
        Vector3 previousPosition = transform.position;
        StartCoroutine(TeleportWithEffects(previousPosition));
    }

    void FastForward()
    {
        if (isRewinding || !isRewindModeActive) return;

        isRewinding = true;
        Vector3 nextPosition = transform.position;
        StartCoroutine(TeleportWithEffects(nextPosition));
    }

    IEnumerator TeleportWithEffects(Vector3 targetPosition)
    {
        if (activeSurroundEffect != null)
        {
            activeSurroundEffect.Stop();
            activeSurroundEffect.Clear();
        }

        if (activeNewPositionEffect != null)
        {
            activeNewPositionEffect.Play();
            yield return new WaitForSeconds(Mathf.Min(activeNewPositionEffect.main.duration, 0.1f));
            activeNewPositionEffect.Stop();
            
            activeNewPositionEffect.transform.position = transform.position;
        }

        transform.position = targetPosition;

        if (activeSurroundEffect != null)
        {
            activeSurroundEffect.Play();
        }
        
        isRewinding = false;
    }
}
