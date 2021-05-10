using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    #region Variables
    [Header("Sound Lists")]
    [SerializeField]
    private AudioClip[] footsteps;
    [SerializeField]
    private AudioClip[] landingSounds;
    [SerializeField]
    private AudioClip[] slidingSounds;
    [SerializeField]
    private AudioClip wooshSound;

    [Header("References")]
    public AudioSource audioSource;
    public PlayerMovementStateMachine movementScript;
    public AnimationStateController animScript;
    public Animator animator;

    [Header("Volume")]
    [Range(0.1f, 10f)]  public float audioVolume = 0.1f;

    [Header("Movement Particle Systems")]
    public ParticleSystem footstepLFX;
    public ParticleSystem footstepRFX;


    private float currentFrameFootstepLeft;
    private float currentFrameFootstepRight;
    private float lastFrameFootstepLeft;
    private float lastFrameFootstepRight;
    #endregion

    void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        animScript = GetComponent<AnimationStateController>();
        movementScript = GetComponent<PlayerMovementStateMachine>();
    }

    void Update()
    {
        Footsteps();
        SlidingSound();
        FallingSound();
    }
    public void Landing(int index)
    {
        audioSource.PlayOneShot((AudioClip)landingSounds[index], audioVolume);
    }

    public void SlidingSound()
    {
        /*
        if (movementScript.playerState == PlayerMovementStateMachine.PlayerState.sliding)
        {
            if(movementScript.slidingInput != 0)
            {
                
                audioSource.loop = true;
                audioSource.clip = slidingSounds[0];
                audioSource.Play();
                
                audioSource.PlayOneShot((AudioClip)slidingSounds[0], audioVolume);
            }
        }
        */
    }

    public void FallingSound()
    {
        /*
        if(animScript.airTimer > 0)
        {
            audioSource.loop = true;
            audioSource.clip = wooshSound;
            audioSource.Play();

            audioSource.volume = Mathf.Lerp(0, 1, Time.time);
        }
        else
        {
            audioSource.Stop();
            audioSource.volume = 0.1f;
        }
        */
    }
    

    //Old version using animation Events of each animation. Doesnt work with the Movement Blend Tree.
    //Using animation curves now

    //Dont delete yet, leads to errors
    public void FootstepL(AnimationEvent animationEvent)
    {
        /*
        if (animationEvent.animatorClipInfo.weight > 0.5)
        {
            audioSource.PlayOneShot((AudioClip)footsteps[Random.Range (0, 4)], audioVolume);
            if(footstepLFX != null)
                footstepLFX.Play();
        }
        */
    }

    public void FootstepR(AnimationEvent animationEvent)
    {
        /*
        if (animationEvent.animatorClipInfo.weight > 0.5)
        {
            audioSource.PlayOneShot((AudioClip)footsteps[Random.Range(5, 9)], audioVolume);
            if (footstepRFX != null)
                footstepRFX.Play();
        }
        */
    }
    
    public void Footsteps()
    {
        currentFrameFootstepLeft = animator.GetFloat("FootstepL");
        if(currentFrameFootstepLeft > 0 && lastFrameFootstepLeft < 0)
        {
            audioSource.PlayOneShot((AudioClip)footsteps[Random.Range(0, 4)], audioVolume);
        }
        lastFrameFootstepLeft = animator.GetFloat("FootstepL");


        currentFrameFootstepRight = animator.GetFloat("FootstepR");
        if (currentFrameFootstepRight < 0 && lastFrameFootstepRight > 0)
        {
            audioSource.PlayOneShot((AudioClip)footsteps[Random.Range(5, 9)], audioVolume);
        }
        lastFrameFootstepRight = animator.GetFloat("FootstepR");
    }
}
