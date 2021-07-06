using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip[] clips;

    [Range(0, 1)]
    public float volume;
    [Range(0.1f, 3)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public ResonanceAudioSource source;
    public enum typeOfAudioSource 
    {
        Burst,
        Ambience,
        Stoppable,
        Variance
    }
    
}
