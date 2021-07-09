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

    int currentSlidingMode;
    float previousSlidingSpeed;
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
        if (s.clips.Length > 0)
        {
            source.clip = s.clips[UnityEngine.Random.Range(0, s.clips.Length)];
            source.volume = s.volume;
            source.pitch = s.pitch;
            source.loop = s.loop;
            source.outputAudioMixerGroup = s.audioGroup;
        }
        else 
        {
            Debug.LogError("There is no sound clip applied to " + s.name);
        }
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
            inactiveSoundSources.RemoveAt(0);
            activeSoundSources.Add(soundSource);
        }
        return soundSource;
    }

    void SetSoundSourceInactive(ResonanceAudioSource source, bool sourceToNull)
    {
        Sound s = Array.Find(sounds, sound => sound.Source == source);
        if (s != null)
        {
            if (sourceToNull && s.Source != null)
            {
                s.Source = null;
            }
            activeSoundSources.Remove(source);
            inactiveSoundSources.Add(source);
        }
    }
    public IEnumerator SetInactiveWhenNotPlaying(ResonanceAudioSource source)
    {
        WaitForEndOfFrame delay = new WaitForEndOfFrame();
        while (source.audioSource.isPlaying) 
        {
            yield return delay;
        }
        SetSoundSourceInactive(source,true);
        
    }
    public void PlayRandom(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        if (s.Source == null) 
        {
            s.Source = GetInactiveSoundSource();
            ApplyValuesToSource(s, s.Source.audioSource);
            
        }
        s.Source.transform.position = ObjectManager.instance.pSM.transform.position;
        s.Source.transform.parent = ObjectManager.instance.pSM.transform;
        s.Source.audioSource.Play();
        StartCoroutine(SetInactiveWhenNotPlaying(s.Source));
    }
    void PlayRandom(Sound s)
    {
        
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        if (s.Source == null)
        {
            s.Source = GetInactiveSoundSource();
            ApplyValuesToSource(s, s.Source.audioSource);

        }
        s.Source.transform.position = ObjectManager.instance.pSM.transform.position;
        s.Source.transform.parent = ObjectManager.instance.pSM.transform;
        s.Source.audioSource.Play();
        StartCoroutine(SetInactiveWhenNotPlaying(s.Source));
    }
    public void PlayRandom(string name,Vector3 position)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        if (s.Source == null)
        {
            s.Source = GetInactiveSoundSource();
            ApplyValuesToSource(s, s.Source.audioSource);
        }
        s.Source.transform.position = position;
        s.Source.audioSource.Play();

        StartCoroutine(SetInactiveWhenNotPlaying(s.Source));
    }
    void PlayRandom(Sound s, Vector3 position)
    {
        
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        if (s.Source == null)
        {
            s.Source = GetInactiveSoundSource();
            ApplyValuesToSource(s, s.Source.audioSource);
        }
        s.Source.transform.position = position;
        s.Source.audioSource.Play();

        StartCoroutine(SetInactiveWhenNotPlaying(s.Source));
    }
    public void StopSound(string name,bool sourceToNull =true)
    {
        Sound s = Array.Find(sounds, item => item.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        if (s.Source != null)
        {
            s.Source.audioSource.Stop();
            SetSoundSourceInactive(s.Source,sourceToNull);
        }
    }

    public void StopSound(Sound s, bool sourceToNull = true)
    {
       
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        if (s.Source != null)
        {
            s.Source.audioSource.Stop();
            SetSoundSourceInactive(s.Source,sourceToNull);
        }
    }
    public void BlendSoundIn(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        if (s.Source != null)
        {
            StartCoroutine(blendSoundIn(s));
        }
    }
    void BlendSoundIn(Sound s)
    {
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        if (s.Source != null)
        {
            StartCoroutine(blendSoundIn(s));
        }
    }
    IEnumerator blendSoundIn(Sound sound) 
    {
        if (sound.Source != null)
        {
            float timer = 0;
            if (sound.Source.audioSource.volume == sound.volume)
                sound.Source.audioSource.volume = 0;
            WaitForEndOfFrame delay = new WaitForEndOfFrame();
            while (sound.Source.audioSource.volume < sound.volume)
            {
                timer += Time.deltaTime;
                sound.Source.audioSource.volume = Mathf.Lerp(0, sound.volume, timer / sound.blendDuration);
                yield return delay;
            }
        }
    }
    public void BlendSoundOut(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        if (s.Source != null) 
        {
            StartCoroutine(blendSoundOut(s));
        }
    }
    public void BlendSoundOut(Sound s)
    {
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        if (s.Source != null)
        {
            StartCoroutine(blendSoundOut(s));
        }
    }
    IEnumerator blendSoundOut(Sound sound)
    {
        if (sound.Source != null)
        {
            float timer = 0;
            sound.Source.audioSource.volume = sound.volume;
            WaitForEndOfFrame delay = new WaitForEndOfFrame();
            while (sound.Source.audioSource.volume < sound.volume)
            {
                timer += Time.deltaTime;
                sound.Source.audioSource.volume = Mathf.Lerp(sound.volume, 0, timer / sound.blendDuration);
                yield return delay;
            }
            StopSound(sound);
        }
    }

    public void StopSlidingSound() 
    {
        StopSound("slidingSlow");
        StopSound("slidingMedium");
        StopSound("slidingFast");
    }
    public void SlidingSoundCalculation(float slidingSpeed)
    {
        float maxSlidingSpeed = ObjectManager.instance.pSM.stats.maxSlidingSpeed;
        float mediumSpeed = maxSlidingSpeed*.8f;
        float highSpeed = maxSlidingSpeed*.5f;
        float speed1;
        float speed2;
        Sound s;

        if (slidingSpeed >= previousSlidingSpeed)
        {
            if (slidingSpeed == 0) 
            {

                StopSlidingSound();
            }

            if (slidingSpeed < mediumSpeed)
            {
                s = Array.Find(sounds, sound => sound.name == "slidingSlow");
                if (s.Source == null) 
                {
                    PlayRandom(s);
                }
                speed1 = 0;
                speed2 = mediumSpeed;
                if (currentSlidingMode == 1)
                {
                    BlendSoundOut("slidingMedium");
                    PlayRandom(s);
                    BlendSoundIn(s);
                    currentSlidingMode = 0;

                }
                if (currentSlidingMode == 2)
                {
                    BlendSoundOut("slidingFast");
                    BlendSoundIn(s);
                    currentSlidingMode = 0;
                }

            }
            else if (slidingSpeed > highSpeed)
            {
                s = Array.Find(sounds, sound => sound.name == "slidingFast");
                speed1 = highSpeed;
                speed2 = maxSlidingSpeed;
                if (s.Source == null)
                {
                    PlayRandom(s);
                }
                if (currentSlidingMode == 0)
                {
                    BlendSoundOut("slidingSlow");
                    PlayRandom(s);
                    BlendSoundIn(s);
                    currentSlidingMode = 2;

                }
                if (currentSlidingMode == 1)
                {
                    BlendSoundOut("slidingMedium");
                    PlayRandom(s);
                    BlendSoundIn(s);
                    currentSlidingMode = 2;
                }

            }
            else
            {
                s = Array.Find(sounds, sound => sound.name == "slidingMedium");
                speed1 = mediumSpeed;
                speed2 = highSpeed;
                if (s.Source == null)
                {
                    PlayRandom(s);
                }
                if (currentSlidingMode == 0)
                {
                    BlendSoundOut("slidingSlow");
                    PlayRandom(s);
                    BlendSoundIn(s);
                    currentSlidingMode = 1;

                }
                if (currentSlidingMode == 2)
                {
                    BlendSoundOut("slidingFast");
                    PlayRandom(s);
                    BlendSoundIn(s);
                    currentSlidingMode = 1;
                }
            }
            //Adjust Pitch 
            if (s != null && s.Source!=null)
            {
                s.Source.audioSource.pitch = Mathf.Lerp(1f, 1.2f, slidingSpeed / maxSlidingSpeed);
            }
        }
        else 
        {
            //apply break 
        }
        previousSlidingSpeed = slidingSpeed;

    }
}
