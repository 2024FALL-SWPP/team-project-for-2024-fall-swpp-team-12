using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindManager : MonoBehaviour
{
    public GameObject clockPrefab;
    public Color rewindFogColor = Color.blue;
    public float fogTransitionSpeed = 50f;
    public float rewindFogDensity = 0.1f;
    public float normalFogDensity = 0f;

    public float clockSpinSpeed = 150f; 
    public float clockSlideDuration = 1.5f; 
    public float clockDuration = 2f; 
    public Vector3 clockStartPosition = new Vector3(-1f, 2f, -11f);
    public Vector3 clockEndPosition = new Vector3(10f, 2f, -2f);

    public AudioClip rewindStartAudio;
    private AudioSource audioSource;

    private bool isRewinding = false;
    private GameObject activeClock;
    private Transform[] clockParts;

    void Start()
    {
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 5f;
        RenderSettings.fogEndDistance = 50f;
        RenderSettings.fogDensity = normalFogDensity;
        RenderSettings.fog = false;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        GameObject testClock = Instantiate(clockPrefab, new Vector3(0, 2, 0), Quaternion.identity);
        testClock.transform.localScale = Vector3.zero;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleRewindMode();
        }
    }

    void ToggleRewindMode()
    {
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
        StartFogTransition(normalFogDensity, rewindFogDensity, Color.clear, rewindFogColor);
        TriggerClockAnimation();
        PlayRewindAudio();
    }

    private void ExitRewindMode()
    {
        StartFogTransition(rewindFogDensity, normalFogDensity, rewindFogColor, Color.clear);
        StopClockAnimation();
        StopRewindAudio();
    }

    private void StartFogTransition(float startDensity, float endDensity, Color startColor, Color endColor)
    {
        RenderSettings.fog = true;
        StartCoroutine(ChangeFogDensity(startDensity, endDensity, disableFogOnComplete: !isRewinding));
        StartCoroutine(ChangeFogColor(startColor, endColor));
    }

    private IEnumerator ChangeFogDensity(float startDensity, float endDensity, bool disableFogOnComplete = false)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fogTransitionSpeed;
            RenderSettings.fogDensity = Mathf.Lerp(startDensity, endDensity, t);
            yield return null;
        }

        if (disableFogOnComplete)
        {
            RenderSettings.fog = false;
        }
    }

    private IEnumerator ChangeFogColor(Color startColor, Color endColor)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fogTransitionSpeed;
            RenderSettings.fogColor = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
    }

    private void TriggerClockAnimation()
    {
        if (activeClock != null) return;

        Quaternion clockRotation = Quaternion.Euler(-30f, 140f, 0f);
        activeClock = Instantiate(clockPrefab, clockStartPosition, clockRotation);
        activeClock.transform.localScale = new Vector3(70, 70, 70);
        
        clockParts = new Transform[]
        {
            activeClock.transform.Find("Clockwork01"),
            activeClock.transform.Find("Clockwork02"),
            activeClock.transform.Find("Gear01"),
            activeClock.transform.Find("Gear02"),
            activeClock.transform.Find("Gear03")
        };

        StartCoroutine(AnimateClock());
    }

    private IEnumerator AnimateClock()
    {
        // Step 1: Slide to center
        float t = 0f;
        Vector3 startPos = clockStartPosition;
        Vector3 endPos = clockEndPosition;

        while (t < 1f)
        {
            t += Time.deltaTime / clockSlideDuration;
            activeClock.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // Step 2: Spin clock parts
        float elapsedTime = 0f;
        while (elapsedTime < clockDuration)
        {
            foreach (Transform part in clockParts)
            {
                if (part != null)
                {
                    if (part.name == "Clockwork01")
                    {
                        part.Rotate(0, 0, -6f * Time.deltaTime);
                    }
                    else if (part.name == "Clockwork02") 
                    {
                        part.Rotate(0, 0, -4f * Time.deltaTime);
                    }
                    else
                    {
                        part.Rotate(0, 0, -clockSpinSpeed * Time.deltaTime);
                    }
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Step 3: Disappear (scale down)
        float disappearTime = 0.5f;
        for (t = 0; t < disappearTime; t += Time.deltaTime)
        {
            float scale = Mathf.Lerp(1, 0, t / disappearTime);
            activeClock.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        Destroy(activeClock);
        activeClock = null;
    }


    private void StopClockAnimation()
    {
        if (activeClock != null)
        {
            Destroy(activeClock);
            activeClock = null;
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