using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

public class AnimationStateController : MonoBehaviour
{
    #region variables
    [Header("References")]
    public PlayerMovementStateMachine movementScript;
    public LadderSizeStateMachine ladderScript;
    public CharacterController controller;
    public FootstepSoundManager soundManager;
    public AudioManager audioManager;
    public Animator animator;
    public FootIK footIKScript;

    public ParticleSystem ladderPushSmoke;

    [Header("Movement Input")]
    float turnAmount;
    float forwardAmount;
    Vector3 move;
    [Tooltip("Walk>>Run blend is determined by velocity.normalized etc, changing PlayerVariable stats e.g." +
        " drag or speed can change these normalized values and therefore change the blend. " +
        "This variable can be used to manually fix this. Dirty, but works for now ")]
    public float antiDrag = 1.65f;

    [Header("Use IK")]
    public bool useFeetIK = false;

    [Header("Animator")]
    float velocity = 0f;
    int VelocityHash;
    int SideInputHash;
    int ForwardInputHash;
    int SlideInputHash;

    [Header("Impact")]
    public float timeForLanding;
    public float timeForRoll;
    public float timeForHardLanding;
    bool canLand;
    bool canRoll;
    bool canHardLand;
    [HideInInspector]
    public float airTimer;
    float lastAirTime;

    [Header("Jumping")]
    float fallTimer = 0;
    float jumpingTimer = 0;
    bool foldJump;
    bool wallJump;
    bool isLadderPushing;
    float ladderPushTimer;

    InputActionMap playerControlsMap;
    InputAction jumpAction;

    ValuesScriptableObject stats;

    [Header("Rigs")]
    public Rig headRig;
    public Rig armRig;
    RigBuilder rigBuilder;


    [Header("Adjusting Head Aim Rig")]
    public Transform headAimTarget;
    [Tooltip("1 means target is right in front of PC, -1 behind. Number specifies the limit to wich the head can turn. Suggestion about -0.65")]
    [Range(-0.5f, -0.9f)] public float headRotationValue = -0.65f;
    [Tooltip("Use Spine to slightly turn Chest with head when looking")]
    public Transform spine;
    //dotProduct used to determine where the HeadAimTarget is in relation to the players forward direction
    float dotProduct;


    [Header("Audio Uses")]
    bool fallAudioPlaying;
    bool attachAudioPlaying;
    bool foldAudioPlaying;
    bool slideAudioPlaying;
    #endregion


    void Start()
    {
        animator = GetComponent<Animator>();
        footIKScript = GetComponent<FootIK>();
        soundManager = GetComponent<FootstepSoundManager>();
        footIKScript.enabled = false;
        audioManager = FindObjectOfType<AudioManager>();
        //movementScript = GetComponent<PlayerMovementStateMachine>();
        //controller = GetComponent<CharacterController>();

        VelocityHash = Animator.StringToHash("Velocity");
        SideInputHash = Animator.StringToHash("SideInput");
        ForwardInputHash = Animator.StringToHash("ForwardInput");
        SlideInputHash = Animator.StringToHash("SlideInput");


        rigBuilder = GetComponent<RigBuilder>();

        // new Input System
        playerControlsMap = movementScript.actionAsset.FindActionMap("PlayerControls");
        stats = movementScript.stats;
        jumpAction = playerControlsMap.FindAction("Jump");
        jumpAction.performed += context => Jump();
    }

    void Update()
    {
        //ignoring the y velocity
        velocity = new Vector2(movementScript.playerVelocity.x, movementScript.playerVelocity.z).magnitude;
        //animator.SetFloat(VelocityHash, velocity);

        foldJump = movementScript.animationControllerisFoldingJumped;
        wallJump = movementScript.isWallJumping;
        animator.SetBool("isFoldjumping", foldJump);
        animator.SetBool("isWalljumping", wallJump);
        if (animator.GetBool("isFoldjumping") == true)
        {
            movementScript.animationControllerisFoldingJumped = false;
        }
        float forwardInput = movementScript.forwardInput;
        animator.SetFloat(ForwardInputHash, forwardInput);
        float slideInput = movementScript.slidingInput;
        animator.SetFloat(SlideInputHash, slideInput);

        if (animator.GetBool("isJumping") == true)
        {
            jumpingTimer += Time.deltaTime;
            if (jumpingTimer > 0.1f)
            {
                animator.SetBool("isJumping", false);
                jumpingTimer = 0;
            }
        }

        GroundedCheck();
        //Sliding();
        Swinging();
        Falling();
        HeadAim();
        FallImpact();
        DismountingTop();
        ladderStateChange();
        MovementParameters();
        LadderPush();

        if (useFeetIK && footIKScript != null)
        {
            CheckIK();
        }
    }
    void MovementParameters()
    {
        move = movementScript.playerVelocity;
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);

        forwardAmount = move.z * antiDrag;
        turnAmount = Mathf.Atan2(move.x, move.z);


        animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
        animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
    }

    void CheckIK()
    {
        // Im sorry if i fucked this up? - Maria
        if (movementScript.playerState == PlayerMovementStateMachine.PlayerState.swinging || movementScript.playerState == PlayerMovementStateMachine.PlayerState.inTheAir)
        {
            footIKScript.enabled = false;
        }
        else
        {
            footIKScript.enabled = true;
        }
    }

    void HeadAim()
    {
        Vector3 toTarget = (headAimTarget.position - transform.position).normalized;
        dotProduct = Vector3.Dot(toTarget, transform.forward);

        //Decreases Rigs weight on the head if look target is too far behind PC
        if (Vector3.Dot(toTarget, transform.forward) < headRotationValue && headRig.weight > 0.1f)
        {
            headRig.weight -= Time.deltaTime * 3;
        }
        //Increases weigth again after dotproduct has crossed threshold
        else if (Vector3.Dot(toTarget, transform.forward) > headRotationValue && headRig.weight < 1f)
        {
            headRig.weight += Time.deltaTime * 3;
        }
    }

    void LadderFold()
    {
        if(movementScript.ladderState == PlayerMovementStateMachine.LadderState.LadderFold)
        {
            //do smol ladder stuff e.g. climb in different pose
        }
    }

    void GroundedCheck()
    {
        if (controller.isGrounded)
        {
            animator.SetBool("isGrounded", true);
            animator.SetBool("isClimbingLadder", false);

            if (airTimer > 0)
            {
                lastAirTime = airTimer;
                airTimer = 0;
            }
        }

        if (!controller.isGrounded)
        {
            animator.SetBool("isGrounded", false);
            //If PC is not on a ladder, increases airTimer
            if (!animator.GetBool("isClimbingLadder"))
            {
                airTimer += Time.deltaTime;
            }
            else
            {
                airTimer = 0;
            }
        }
    }

    void ladderStateChange()
    {
        //animations for retracting and extending ladder
        if (ladderScript.isFoldingUp && movementScript.playerState == PlayerMovementStateMachine.PlayerState.swinging)
        {
            animator.SetBool("isFoldingUp", true);
            //Audio
            if (!foldAudioPlaying)
            {
            //Audio
            if (!foldAudioPlaying)
            {
                audioManager.PlayRandom("FoldLadder",ladderScript.transform.position);
                foldAudioPlaying = true;
            }
            }
        }
        else
        {
            animator.SetBool("isFoldingUp", false);
            foldAudioPlaying = false;
        }
    }

    void FallImpact()
    {
        if (airTimer >= 0.1f)
        {
            canLand = true;

        }
        /*
        if (airTimer >= timeForRoll)
        {
            canRoll = true;
            canLand = false;
        }
        */
        if (airTimer >= timeForHardLanding)
        {
            canHardLand = true;
            canLand = false;
            canRoll = false;
        }
        if (canLand && controller.isGrounded)
        {
            canLand = false;
            audioManager.PlayRandom("LandingAfterJump",movementScript.transform.position);
            if (lastAirTime >= timeForLanding)
            {
                animator.SetBool("isLanding", true);
            }
        }
        else
        {
            animator.SetBool("isLanding", false);
        }
        /*
        if (canRoll && controller.isGrounded)
        {
            animator.SetBool("isRolling", true);
            canRoll = false;
            audioManager.Play("LandingAfterJump");
        }
        else
        {
            animator.SetBool("isRolling", false);
        }
        */
        //Rolling after fall if Input != 0
        if (canHardLand && controller.isGrounded && forwardAmount > 0.1)
        {
            animator.SetBool("isRolling", true);
            canHardLand = false;
            audioManager.PlayRandom("LandingAfterJump",movementScript.transform.position);
        }
        else
        {
            animator.SetBool("isRolling", false);
        }
        //HardImpact after fall if Input == 0
        if (canHardLand && controller.isGrounded && forwardAmount < 0.1)
        {
            animator.SetBool("isHardLanding", true);
            canHardLand = false;
            audioManager.PlayRandom("LandingAfterFall",movementScript.transform.position);
            
            StartCoroutine(ImpactInput());
        }
        else
        {
            animator.SetBool("isHardLanding", false);
        }
    }

    IEnumerator ImpactInput()
    {
        playerControlsMap.Disable();
        yield return new WaitForSeconds(1);
        playerControlsMap.Enable();
    }

    void Falling()
    {
        //Falling
        if (!controller.isGrounded && animator.GetBool("isClimbingLadder") == false)
        {
            //Added Falltimer to prevent Falling before climbing
            fallTimer += Time.deltaTime;


            //Audio
            if (!fallAudioPlaying)
            {
                audioManager.PlayRandom("Falling");
                fallAudioPlaying = true;
            }


            if (fallTimer >= 0.2f)
            {
                animator.SetBool("FallDelay", true);
            }
        }
        else if (controller.isGrounded)
        {
            animator.SetBool("FallDelay", false);
            fallTimer = 0;


            //Audio
            if (fallAudioPlaying)
            {
                audioManager.StopSound("Falling");
                fallAudioPlaying = false;
            }
        }
    }

    void Jump()
    {
        if (movementScript.playerState == PlayerMovementStateMachine.PlayerState.walking || movementScript.playerState == PlayerMovementStateMachine.PlayerState.swinging)
        {
            animator.SetBool("isJumping", true);
            animator.SetBool("isClimbingLadder", false);
            rigBuilder.enabled = true;


            //Audio
            audioManager.PlayRandom("JumpStart");
        }
    }

    void LadderPush()
    {
        if (movementScript.didLadderPush && !isLadderPushing)
        {
            animator.SetBool("isRocketJumping", true);
            isLadderPushing = true;
            if(ladderPushSmoke != null)
            {
                //ladderPushSmoke.Play();
            }
        }
        if (isLadderPushing)
        {
            ladderPushTimer += Time.deltaTime;
        }

        if (ladderPushTimer >= 0.2)
        {
            animator.SetBool("isRocketJumping", false);
            ladderPushTimer = 0;

        }

        if (!movementScript.didLadderPush)
        {
            animator.SetBool("isRocketJumping", false);
            isLadderPushing = false;
        }
    }

    void Swinging()
    {
        if (movementScript.playerState == PlayerMovementStateMachine.PlayerState.swinging)
        {
            animator.SetBool("isSwingingLadder", true);
            armRig.weight = 0;
            
            //Slide Audio
            if (movementScript.slidingInput != 0 && !slideAudioPlaying)
            {
                audioManager.PlayRandom("Sliding");
                slideAudioPlaying = true;
            }
            if (movementScript.slidingInput == 0 && slideAudioPlaying)
            {
                audioManager.StopSound("Sliding");
                slideAudioPlaying = false;
            }
            
            //Fall Audio
            if (fallAudioPlaying)
            {
                audioManager.StopSound("Falling");
                fallAudioPlaying = false;
            }

            
            //Attach Audio
            if (!attachAudioPlaying)
            {
                audioManager.PlayRandom("AttachLadder",ladderScript.transform.position);
                attachAudioPlaying = true;
            }
            
        }
        else
        {
            animator.SetBool("isSwingingLadder", false);
            //rigBuilder.enabled = true;
            armRig.weight = 1;



            //Audio
            if (attachAudioPlaying)
            {
                audioManager.StopSound("AttachLadder");
                attachAudioPlaying = false;
            }

            audioManager.StopSound("Sliding");
            slideAudioPlaying = false;
        }        
    }

    void DismountingTop()
    {
        if (movementScript.dismounting == true)
        {
            animator.SetBool("isDismounting", true);
        }
        else
        {
            animator.SetBool("isDismounting", false);
        }
    }
}
