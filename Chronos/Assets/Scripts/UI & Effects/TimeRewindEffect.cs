using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeRewindEffect : MonoBehaviour
{
    public ParticleSystem newPositionEffect;
    public ParticleSystem surroundEffect;
    private ParticleSystem activeSurroundEffect;
    private ParticleSystem activeNewPositionEffect;

    void Start()
    {
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
        if (activeSurroundEffect != null && activeSurroundEffect.isPlaying)
        {
            activeSurroundEffect.transform.position = transform.position;
        }
    }

    public void EnterRewindMode()
    {
        if (activeSurroundEffect != null && !activeSurroundEffect.isPlaying)
        {
            activeSurroundEffect.Play();
        }
    }

    public void LeaveRewindMode()
    {
        if (activeSurroundEffect != null)
        {
            activeSurroundEffect.Stop();
            activeSurroundEffect.Clear();
        }
    }

    public void InvokeTimeRewindEffect()
    {
        StartCoroutine(TeleportWithEffects());
    }

    IEnumerator TeleportWithEffects()
    {
        if (activeSurroundEffect != null)
        {
            activeSurroundEffect.Stop();
            activeSurroundEffect.Clear();
        }

        if (activeNewPositionEffect != null)
        {
            activeNewPositionEffect.transform.position = transform.position;
            activeNewPositionEffect.Play();
            yield return new WaitForSeconds(0.3f);
            activeNewPositionEffect.Stop();
            activeNewPositionEffect.Clear();
        }

        if (PlayerController.playerController.isTimeRewinding) activeSurroundEffect?.Play();
    }
}