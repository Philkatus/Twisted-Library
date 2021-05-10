using System.Collections;
using System.Collections.Generic;
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

    [Header("References")]
    public AudioSource audioSource;
    public PlayerMovementStateMachine movementScript;
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
    }

    void Update()
    {
        Footsteps();
    }
    public void Landing(int index)
    {
        audioSource.PlayOneShot((AudioClip)landingSounds[index], audioVolume);
    }

    public void Sliding()
    {
        if (movementScript.playerState == PlayerMovementStateMachine.PlayerState.sliding)
        {
            if(movementScript.slidingInput != 0)
            {
                audioSource.loop = true;
                audioSource.clip = slidingSounds[0];
                audioSource.Play();
            }
            else
            {
                audioSource.Play();
            }
        }
    }
    
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
