using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip[] clips;
    public AudioMixerGroup audioGroup;


    [Range(0, 1)]
    public float volume;
    [Range(0.1f, 3)]
    public float pitch;

    public float blendDuration;
    public bool loop;

    [HideInInspector]
    public ResonanceAudioSource Source 
    {
        get
        {
            return source;
        }
        set 
        {
            if (source != null ) 
            {
                AudioManager.Instance.StopSound(this,false);
            }
            source = value;
        }
    }
    ResonanceAudioSource source;
    Coroutine currentSoundPlaying;
    public enum typeOfAudioSource
    {
        Burst,
        Ambience,
        Stoppable
    }
}
