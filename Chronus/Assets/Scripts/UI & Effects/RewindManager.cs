using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewindManager : MonoBehaviour
{
    public Color rewindOverlayColor = new Color(0, 0, 1, 0.1f);
    public float transitionSpeed = 2f;

    public AudioClip rewindStartAudio;
    private AudioSource audioSource;

    private Image overlayImage;
    private bool isRewinding = false;

    void Start()
    {
        CreateOverlay();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        InputManager.inputManager.OnTimeRewindModeToggle += ToggleRewindMode;
    }

    void OnDestroy()
    {
        InputManager.inputManager.OnTimeRewindModeToggle -= ToggleRewindMode;
    }

    void ToggleRewindMode()
    {
        if (!PlayerController.playerController.isTimeRewinding &&
            (PlayerController.playerController.isBlinking || TurnManager.turnManager.CLOCK || PlayerController.playerController.willDropDeath)) return;
        // assuring that every action should be ended (during the turn)

        isRewinding = !isRewinding;

        if (isRewinding)
        {
            EnterRewindMode();
        }
        else
        {
            ExitRewindMode();
        }
    }

    private void EnterRewindMode()
    {
        StartOverlayTransition(Color.clear, rewindOverlayColor);
        PlayRewindAudio();
    }

    private void ExitRewindMode()
    {
        StartOverlayTransition(rewindOverlayColor, Color.clear);
        StopRewindAudio();
    }

    private void StartOverlayTransition(Color startColor, Color endColor)
    {
        StartCoroutine(ChangeOverlayColor(startColor, endColor));
    }

    private IEnumerator ChangeOverlayColor(Color startColor, Color endColor)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            overlayImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
    }

    private void CreateOverlay()
    {
        GameObject canvasObj = new GameObject("RewindCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject imageObj = new GameObject("OverlayImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        overlayImage = imageObj.AddComponent<Image>();
        overlayImage.color = Color.clear;
        overlayImage.rectTransform.anchorMin = Vector2.zero;
        overlayImage.rectTransform.anchorMax = Vector2.one;
        overlayImage.rectTransform.sizeDelta = Vector2.zero;
    }

    private void PlayRewindAudio()
    {
        if (rewindStartAudio != null && audioSource != null)
        {
            audioSource.clip = rewindStartAudio;
            audioSource.loop = false;
            audioSource.Play();
        }
    }

    private void StopRewindAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}