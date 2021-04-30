using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] clips;

    public AudioSource audioSource;
    public float audioVolume = 0.1f;

    public ParticleSystem footstepLFX;
    public ParticleSystem footstepRFX;
    void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
    }

    public void FootstepL(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5)
        {
            audioSource.PlayOneShot((AudioClip)clips[0], audioVolume);
            footstepLFX.Play();
        }
    }

    public void FootstepR(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5)
        {
            audioSource.PlayOneShot((AudioClip)clips[1], audioVolume);
            footstepRFX.Play();
        }
    }

    public void JumpStart(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5)
        {
            audioSource.PlayOneShot((AudioClip)clips[2], audioVolume);
        }
    }

    public void Land(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5)
        {
            audioSource.PlayOneShot((AudioClip)clips[3], audioVolume);
        }
    }
}
