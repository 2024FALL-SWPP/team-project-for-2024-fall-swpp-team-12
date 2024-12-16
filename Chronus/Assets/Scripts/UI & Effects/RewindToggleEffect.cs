using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RewindToggleEffect : MonoBehaviour
{
    public Color rewindOverlayColor = new Color(0, 0, 1, 0.1f);
    private float transitionSpeed = 2f;
    public AudioClip rewindStartAudio;
    private AudioSource audioSource;
    public Image overlayImage;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void EnterRewindMode()
    {
        StartOverlayTransition(Color.clear, rewindOverlayColor);
        PlayRewindAudio();
    }

    public void LeaveRewindMode()
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