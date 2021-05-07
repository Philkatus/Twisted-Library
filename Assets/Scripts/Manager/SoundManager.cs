using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] footsteps;
    public AudioClip[] landingSounds;

    public AudioSource audioSource;
    public float audioVolume = 0.1f;

    public ParticleSystem footstepLFX;
    public ParticleSystem footstepRFX;
    void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
    }
    public void Landing(int index)
    {
        audioSource.PlayOneShot((AudioClip)landingSounds[index], audioVolume);
    }
    public void HardImpact(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5)
        {
            audioSource.PlayOneShot((AudioClip)landingSounds[0], audioVolume);
        }
    }
    public void FootstepL(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5)
        {
            audioSource.PlayOneShot((AudioClip)footsteps[Random.Range (0, 4)], audioVolume);
            if(footstepLFX != null)
                footstepLFX.Play();
        }
    }

    public void FootstepR(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5)
        {
            audioSource.PlayOneShot((AudioClip)footsteps[Random.Range(5, 9)], audioVolume);
            if (footstepRFX != null)
                footstepRFX.Play();
        }
    }

    public void JumpStart(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5)
        {
            audioSource.PlayOneShot((AudioClip)footsteps[2], audioVolume);
        }
    }

    public void Land(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5)
        {
            audioSource.PlayOneShot((AudioClip)footsteps[3], audioVolume);
        }
    }
}
