using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable()]
public struct SoundParameters
{
    [Range(0,1)]
    public float Volume;

    [Range(-3, 3)]
    public float Pitch;

    public bool Loop;
}

[System.Serializable()]
public class Sound
{
    [SerializeField] string name;
    public string Name { get { return name; } }

    [SerializeField] AudioClip clip;
    public AudioClip Clip { get { return clip; } }

    [SerializeField] SoundParameters parameters;
    public SoundParameters Parameters { get { return parameters; } }

    [HideInInspector]
    public AudioSource Source;

    public void Play()
    {
        Source.clip = Clip;

        Source.volume = Parameters.Volume;
        Source.pitch = Parameters.Pitch;
        Source.loop = Parameters.Loop;

        Source.Play();
    }
    public void Stop()
    {
        Source.Stop();
    }

}

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance = null;
    public static AudioManager Instance { get { return instance; } }

    [SerializeField] Sound[] sounds;
    [SerializeField] AudioSource sourcePrefab;

    [SerializeField] public string startupTrack;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
            
        }
        DontDestroyOnLoad(this.gameObject);
        InitSound();
    }

    private void Start()
    {
        if(string.IsNullOrEmpty(startupTrack) != true)
        {
            PlaySound(startupTrack);
        }
    }

    void InitSound()
    {
        foreach (var sound in sounds)
        {
            AudioSource source = (AudioSource)Instantiate(sourcePrefab, gameObject.transform);
            source.name = sound.Name;
            sound.Source = source;
        }
    }

    public void PlaySound(string name)
    {
        var sound = GetSound(name);
        if (sound != null)
        {
            sound.Play();
        }
        else
        {
            Debug.LogWarningFormat("SOund by the name {0} is not found. Issue on PlaySound() on AudioManager", name);
        }
    }
    public void StopSound(string name)
    {
        var sound = GetSound(name);
        if (sound != null)
        {
            sound.Stop();
        }
        else
        {
            Debug.LogWarningFormat("SOund by the name {0} is not found. Issue on StopSound() on Audiomanager", name);
        }
    }

    public void StopAllSound()
    {
        foreach (var soundS in sounds)
        {
            soundS.Stop();
        }
    }

    Sound GetSound(string name)
    {
        foreach (var sound in sounds)
        {
            if (sound.Name == name)
            {
                return sound;
            }
        }
        return null;
    }
}
