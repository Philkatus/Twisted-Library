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
    [Range(0.1f, 10f)] public float audioVolumeConcrete = 0.1f;
    [Range(0f, 10f)] public float audioVolumeWater = 0.1f;

    [Header("Movement Particle Systems")]
    public ParticleSystem footstepLFX;
    public ParticleSystem footstepRFX;

    private float currentFrameFootstepLeft;
    private float currentFrameFootstepRight;
    private float lastFrameFootstepLeft;
    private float lastFrameFootstepRight;
    private float currentFrameExhale;
    private float lastFrameExhale;


    [SerializeField] Transform root, footL, footR;
    #endregion

    void Start()
    {

        animator = GetComponent<Animator>();
        animScript = GetComponent<AnimationStateController>();
        movementScript = ObjectManager.instance.pSM;
    }

    void LateUpdate()
    {
        Footsteps();
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



    ////Old version using animation Events of each animation. Doesnt work with the Movement Blend Tree.
    ////Using animation curves now

    ////Dont delete yet, leads to errors
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

    bool leftPlayed, rightPlayed;


    public void Footsteps()
    {

        if (ObjectManager.instance.animationStateController.playerSM.playerState == PlayerMovementStateMachine.PlayerState.walking)
        {
            //Debug.Log(ObjectManager.instance.animationStateController.animator.GetBoneTransform(HumanBodyBones.LeftFoot).position);
            var footLPos = root.position.y - footL.position.y;
            var footRPos = root.position.y - footR.position.y;

            if (footLPos > 0.4f)
            {
                if (!leftPlayed)
                {
                    ObjectManager.instance.animationStateController.playerSM.effects.TriggerLeftFoot();
                    PlayLeftStep();
                    leftPlayed = true;
                }
            }
            else
            {
                leftPlayed = false;
            }
            if (footRPos > 0.4f)
            {
                if (!rightPlayed)
                {

                    ObjectManager.instance.animationStateController.playerSM.effects.TriggerRightFoot();

                    PlayRightStep();
                    rightPlayed = true;
                }
            }
            else
            {
                rightPlayed = false;
            }
        }

        //Debug.Log("FootL: " + footL.position);
        //currentFrameFootstepLeft = animator.GetFloat("FootstepL");
        //if (currentFrameFootstepLeft > 0 && lastFrameFootstepLeft < 0)
        //{
        //    PlayLeftStep();
        //}
        //lastFrameFootstepLeft = animator.GetFloat("FootstepL");


        //currentFrameFootstepRight = animator.GetFloat("FootstepR");
        //if (currentFrameFootstepRight < 0 && lastFrameFootstepRight > 0)
        //{
        //    PlayRightStep();
        //}
        //lastFrameFootstepRight = animator.GetFloat("FootstepR");
    }

    public void Land()
    {
        PlayLeftStep();
        PlayRightStep();
    }

    private void PlayLeftStep()
    {
        if (movementScript.isOnWater && footstepsLeftWater.Length > 0)
        {
            resonanceAudio.audioSource.PlayOneShot((AudioClip)footstepsLeftWater[Random.Range(0, footstepsLeft.Length)], audioVolumeWater);
        }
        else if (footstepsLeft.Length > 0)
        {

            resonanceAudio.audioSource.PlayOneShot((AudioClip)footstepsLeft[Random.Range(0, footstepsLeft.Length)], audioVolumeConcrete);
        }
    }

    private void PlayRightStep()
    {
        if (movementScript.isOnWater && footstepsRightWater.Length > 0)
        {
            resonanceAudio.audioSource.PlayOneShot((AudioClip)footstepsRightWater[Random.Range(0, footstepsLeft.Length)], audioVolumeWater);
        }
        else if (footstepsRight.Length > 0)
        {
            resonanceAudio.audioSource.PlayOneShot((AudioClip)footstepsRight[Random.Range(0, footstepsLeft.Length)], audioVolumeConcrete);

        }
    }

    public void SetWater(bool setTo)
    {
        movementScript.isOnWater = setTo;
    }
}
