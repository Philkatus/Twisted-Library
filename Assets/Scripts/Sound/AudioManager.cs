using UnityEngine.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public List<ResonanceAudioSource> activeSoundSources = new List<ResonanceAudioSource>();
    public List<ResonanceAudioSource> inactiveSoundSources = new List<ResonanceAudioSource>();
    [SerializeField] GameObject SoundSourcePrefab;

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
        
    }

    void ApplyValuesToSource(Sound s, AudioSource source)
    {
        source.clip = s.clips[UnityEngine.Random.Range(0, s.clips.Length)];
        source.volume = s.volume;
        source.pitch = s.pitch;
        source.loop = s.loop;
        source.outputAudioMixerGroup = s.audioGroup;
    }
    void ApplyValuesToSource(Sound s, AudioSource source,int index)
    {
        source.clip = s.clips[index];
        source.volume = s.volume;
        source.pitch = s.pitch;
        source.loop = s.loop;
        source.outputAudioMixerGroup = s.audioGroup;
    }
    ResonanceAudioSource GetInactiveSoundSource() 
    {
        ResonanceAudioSource soundSource;
        if (inactiveSoundSources.Count == 0) 
        {

            soundSource = Instantiate(SoundSourcePrefab).GetComponent<ResonanceAudioSource>();
            activeSoundSources.Add(soundSource);
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

    public IEnumerator SetInactiveWhenNotPlaying(ResonanceAudioSource source)
    {
        WaitForEndOfFrame delay = new WaitForEndOfFrame();
        while (source.audioSource.isPlaying) 
        {
            yield return delay;
        }
        SetSoundSourceInactive(source);
        
    }
    public void PlayRandom(string name)
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
            ApplyValuesToSource(s, s.source.audioSource);
            
        }
        s.source.transform.position = transform.position;
        s.source.transform.parent = transform;
        s.source.audioSource.Play();
        StartCoroutine(SetInactiveWhenNotPlaying(s.source));
    }
    public void PlayRandom(string name,Vector3 position)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        if (s.source == null)
        {
            s.source = GetInactiveSoundSource();
            ApplyValuesToSource(s, s.source.audioSource);
        }
        s.source.transform.position = position;
        s.source.audioSource.Play();

        StartCoroutine(SetInactiveWhenNotPlaying(s.source));
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
            SetSoundSourceInactive(s.source);
        }
    }
}
