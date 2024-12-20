using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

//사운드의 타입이다. 사운드를 중단을 식별하기위해 사용한다.
public enum SoundType
{
    BGM,
    SFX,
    AMBIENT
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager soundManager; //singleton

    //Mixer
    [SerializeField] private AudioMixer mAudioMixer;

    //Volume
    private float mCurrentBGMVolume = 2.0f;
    private float mCurrentSFXVolume = 20.0f;
    private float mCurrentAMBIENTVolume = 2.0f;

    //Sound Clips
    private Dictionary<string, AudioClip> mClipsDictionary;
    [SerializeField] private AudioClip[] mPreloadClips;

    //TemporarySoundPlayer class -for-> Each Sound instantiated
    private List<TemporarySoundPlayer> mInstantiatedSounds;

    private void Awake()
    {
        if (soundManager == null) { soundManager = this; }

        mClipsDictionary = new Dictionary<string, AudioClip>();
        foreach (AudioClip clip in mPreloadClips)
        {
            mClipsDictionary.Add(clip.name, clip);
        }

        mInstantiatedSounds = new List<TemporarySoundPlayer>();
    }

    private void Start()
    {
        InitVolumes(mCurrentBGMVolume, mCurrentSFXVolume, mCurrentAMBIENTVolume);
    }

    private AudioClip GetClip(string clipName)
    {
        AudioClip clip = mClipsDictionary[clipName];

        if (clip == null) { Debug.LogError(clipName + "doesn't exist"); }

        return clip;
    }
    //if it is to be looped -> store in list. -> stop loop: delete from list
    private void AddToList(TemporarySoundPlayer soundPlayer)
    {
        mInstantiatedSounds.Add(soundPlayer);
    }

    //START or STOP PLAYING
    public void StopLoopSound(string clipName)
    {
        if (mInstantiatedSounds == null) return;
        foreach (TemporarySoundPlayer audioPlayer in mInstantiatedSounds)
        {
            if (audioPlayer.ClipName == clipName)
            {
                mInstantiatedSounds.Remove(audioPlayer);
                Destroy(audioPlayer.gameObject);
                return;
            }
        }

        Debug.LogWarning(clipName + "cannot be found");
    }
    public void StopAllLoopSound()
    {
        if (mInstantiatedSounds == null) return;
        TemporarySoundPlayer audioPlayer;
        for (int i = mInstantiatedSounds.Count - 1; i >= 0; i--)
        {
            audioPlayer = mInstantiatedSounds[i];
            mInstantiatedSounds.Remove(audioPlayer);
            Destroy(audioPlayer.gameObject);
        }
    }
    public void PlaySound2D(string clipName, float volume = 1.0f, bool isLoop = false, SoundType type = SoundType.SFX)
    {
        GameObject obj = new GameObject("TemporarySoundPlayer 2D");
        TemporarySoundPlayer soundPlayer = obj.AddComponent<TemporarySoundPlayer>();

        //if it is to be looped -> store in list.
        if (isLoop) { AddToList(soundPlayer); }

        soundPlayer.InitSound2D(GetClip(clipName), volume);
        soundPlayer.Play(mAudioMixer.FindMatchingGroups(type.ToString())[0], isLoop);
    }
    public void PlaySound3D(string clipName, Transform audioTarget, float volume = 1.0f, bool isLoop = false, SoundType type = SoundType.SFX, bool attachToTarget = false, float minDistance = 0.0f, float maxDistance = 64.0f)
    {
        GameObject obj = new GameObject("TemporarySoundPlayer 3D");
        obj.transform.localPosition = audioTarget.transform.position; //sound Source!
        if (attachToTarget) { obj.transform.parent = audioTarget; }

        TemporarySoundPlayer soundPlayer = obj.AddComponent<TemporarySoundPlayer>();

        //if it is to be looped -> store in list.
        if (isLoop) { AddToList(soundPlayer); }

        soundPlayer.InitSound3D(GetClip(clipName), volume, minDistance, maxDistance);
        soundPlayer.Play(mAudioMixer.FindMatchingGroups(type.ToString())[0], isLoop);
    }

    //VOLUME SET when scene load
    public void InitVolumes(float bgm, float effect, float ambient)
    {
        SetVolume(SoundType.BGM, bgm);
        SetVolume(SoundType.SFX, effect);
        SetVolume(SoundType.AMBIENT, ambient);
    }
    public void SetVolume(SoundType type, float value)
    {
        mAudioMixer.SetFloat(type.ToString(), value);
    }
    /*
    //for playing random sounds
    public static string Range(int from, int includedTo, bool isStartZero = false)
    {
        if (includedTo > 100 && isStartZero) { Debug.LogWarning("no morethan100 with number0."); }

        int value = UnityEngine.Random.Range(from, includedTo + 1);

        return value < 10 && isStartZero ? '0' + value.ToString() : value.ToString();
    }*/
}
//Reference: https://bonnate.tistory.com/183
