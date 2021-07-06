using UnityEngine.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    List<ResonanceAudioSource> activeSoundSources = new List<ResonanceAudioSource>();
    List<ResonanceAudioSource> inactiveSoundSources = new List<ResonanceAudioSource>();

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
        s.source.audioSource.clip = s.clip;
        s.source.audioSource.volume = s.volume;
        s.source.audioSource.pitch = s.pitch;
        s.source.audioSource.loop = s.loop;
    }
    ResonanceAudioSource GetInactiveSoundSource() 
    {
        ResonanceAudioSource soundSource;
        if (inactiveSoundSources.Count == 0) 
        {

            soundSource = Instantiate(new GameObject()).AddComponent<ResonanceAudioSource>();
        }
        else 
        {
            soundSource = inactiveSoundSources[0];
            soundSource.gameObject.SetActive(true);
            inactiveSoundSources.RemoveAt(0);
            activeSoundSources.Add(soundSource);
        }
        return soundSource;
    }

    void SetSoundSourceInactive(ResonanceAudioSource source) 
    {
        Sound s = Array.Find(sounds, sound => sound.source == source);
        s.source = null;
        activeSoundSources.Remove(source);
        inactiveSoundSources.Add(source);
        source.gameObject.SetActive(false);
    }


    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        if (s.source == null) 
        {
            s.source = GetInactiveSoundSource();
        }
        s.source.audioSource.Play();
    }

    public void StopSound(string name)
    {
        Sound s = Array.Find(sounds, item => item.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        if (s.source != null)
        {
            s.source.audioSource.Stop();
        }
    }
}
