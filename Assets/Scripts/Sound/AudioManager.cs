using UnityEngine.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    List<AudioSource> activeSoundSources = new List<AudioSource>();
    List<AudioSource> inactiveSoundSources = new List<AudioSource>();

    public static AudioManager Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        
        foreach (Sound s in sounds)
        {
            CreateSoundSource(s);
        }
    }

    void CreateSoundSource(Sound s) 
    {
        s.source = GetInactiveSoundSource();
        s.source.clip = s.clip;
        s.source.volume = s.volume;
        s.source.pitch = s.pitch;
        s.source.loop = s.loop;
    }
    AudioSource GetInactiveSoundSource() 
    {
       AudioSource soundSource;
        if (inactiveSoundSources.Count == 0) 
        {

            soundSource = Instantiate(new GameObject()).AddComponent<AudioSource>();
        }
        else 
        {
            soundSource = inactiveSoundSources[0];
            soundSource.gameObject.SetActive(true);
        }
        return soundSource;
    }

    void SetSoundSourceInactive(GameObject source) 
    {

    }


    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    public void StopSound(string name)
    {
        Sound s = Array.Find(sounds, item => item.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Stop();
    }
}
