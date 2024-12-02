using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeRewindEffect : MonoBehaviour
{
    public ParticleSystem newPositionEffect;
    public ParticleSystem surroundEffect;

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

        InputManager.inputManager.OnTimeRewindModeToggle += ToggleRewindMode;
        InputManager.inputManager.OnTimeRewindControl += HandleRewindControl;
    }

    void OnDestroy()
    {
        InputManager.inputManager.OnTimeRewindModeToggle -= ToggleRewindMode;
        InputManager.inputManager.OnTimeRewindControl -= HandleRewindControl;
    }

    void Update()
    {
        if (activeSurroundEffect != null && activeSurroundEffect.isPlaying)
        {
            activeSurroundEffect.transform.position = transform.position;
        }
    }

    void ToggleRewindMode()
    {
        if (!PlayerController.playerController.isTimeRewinding &&
            (PlayerController.playerController.isBlinking || TurnManager.turnManager.CLOCK || PlayerController.playerController.willDropDeath)) return;
        // assuring that every action should be ended (during the turn)

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

    private void HandleRewindControl(string command)
    {
        if (!isRewindModeActive || isRewinding) return;

        switch (command)
        {
            case "q":
            case "e":
                PerformTimeEffect();
                break;
        }
    }

    void PerformTimeEffect()
    {
        if (isRewinding) return;

        isRewinding = true;
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

        if (activeSurroundEffect != null)
        {
            activeSurroundEffect.Play();
        }

        isRewinding = false;
    }
}