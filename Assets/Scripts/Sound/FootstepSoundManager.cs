using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class FootstepSoundManager : MonoBehaviour
{
    #region Variables

    [Header("References")]
    public ResonanceAudioSource resonanceAudio;
    public PlayerMovementStateMachine movementScript;
    public AnimationStateController animScript;
    public Animator animator;

    [Header("Sound Lists")]
    [SerializeField]
    private AudioClip[] footstepsLeft;
    [SerializeField]
    private AudioClip[] footstepsRight;
    [SerializeField]
    private AudioClip[] footstepsLeftGras;
    [SerializeField]
    private AudioClip[] footstepsRightGras;
    [SerializeField]
    private AudioClip[] footstepsLeftWater;
    [SerializeField]
    private AudioClip[] footstepsRightWater;


    [SerializeField]
    private AudioClip exhale;

    [Header("Volume")]
    [Range(0.1f, 10f)]  public float audioVolume = 0.1f;

    [Header("Movement Particle Systems")]
    public ParticleSystem footstepLFX;
    public ParticleSystem footstepRFX;


    private bool isOnWater;
    private bool isOnGras;


    private float currentFrameFootstepLeft;
    private float currentFrameFootstepRight;
    private float lastFrameFootstepLeft;
    private float lastFrameFootstepRight;
    private float currentFrameExhale;
    private float lastFrameExhale;
    #endregion

    void Start()
    {
       
        animator = GetComponent<Animator>();
        animScript = GetComponent<AnimationStateController>();
        movementScript = GetComponent<PlayerMovementStateMachine>();
    }

    void Update()
    {
        Footsteps();
        Exhale();
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
            PlayLeftStep();
        }
        lastFrameFootstepLeft = animator.GetFloat("FootstepL");


        currentFrameFootstepRight = animator.GetFloat("FootstepR");
        if (currentFrameFootstepRight < 0 && lastFrameFootstepRight > 0)
        {
            PlayRightStep();
        }
        lastFrameFootstepRight = animator.GetFloat("FootstepR");
    }

    private void PlayLeftStep() 
    {
        if (isOnGras &&footstepsLeftGras.Length>0) 
        {
            resonanceAudio.audioSource.PlayOneShot((AudioClip)footstepsLeftGras[Random.Range(0, footstepsLeft.Length)], audioVolume);
        }
        else if (isOnWater && footstepsLeftWater.Length > 0) 
        {
            resonanceAudio.audioSource.PlayOneShot((AudioClip)footstepsLeftWater[Random.Range(0, footstepsLeft.Length)], audioVolume);
        }
        else if(footstepsLeft.Length > 0)
        {
            resonanceAudio.audioSource.PlayOneShot((AudioClip)footstepsLeft[Random.Range(0, footstepsLeft.Length)], audioVolume);
        }
    }

    private void PlayRightStep()
    {
        if (isOnGras && footstepsRightGras.Length > 0)
        {
            resonanceAudio.audioSource.PlayOneShot((AudioClip)footstepsRightGras[Random.Range(0, footstepsLeft.Length)], audioVolume);
        }
        else if (isOnWater && footstepsRightWater.Length > 0)
        {
            resonanceAudio.audioSource.PlayOneShot((AudioClip)footstepsRightWater[Random.Range(0, footstepsLeft.Length)], audioVolume);
        }
        else if(footstepsRight.Length > 0)
        {
            resonanceAudio.audioSource.PlayOneShot((AudioClip)footstepsRight[Random.Range(0, footstepsLeft.Length)], audioVolume);
        }
    }

    public void Exhale()
    {
        currentFrameExhale = animator.GetFloat("Exhale");
        if (currentFrameExhale > 0 && lastFrameExhale < 0)
        {
            resonanceAudio.audioSource.PlayOneShot(exhale, audioVolume);
        }
        lastFrameExhale = animator.GetFloat("Exhale");
    }

    public void SetGras(bool setTo)
    {
        isOnGras = setTo;
    }
    public void SetWater(bool setTo)
    {
        isOnWater = setTo;
    }
}
