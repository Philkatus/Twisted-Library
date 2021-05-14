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
    public SoundManager soundManager;
    public AudioManager audioManager;
    public Animator animator;
    public FootIK footIKScript;

    [Header("Movement Input")]
    float turnAmount;
    float forwardAmount;
    Vector3 move;

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
    bool isRocketJumping;
    float rocketJumpTimer;

    InputActionMap playerControlsMap;
    InputAction jumpAction;

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
        soundManager = GetComponent<SoundManager>();
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
        playerControlsMap.Enable();
        jumpAction = playerControlsMap.FindAction("Jump");

        jumpAction.performed += context => LadderJump();
    }

    void Update()
    {
        //ignoring the y velocity
        velocity = new Vector2(movementScript.playerVelocity.x, movementScript.playerVelocity.z).magnitude;
        animator.SetFloat(VelocityHash, velocity);
       
        foldJump = movementScript.animationControllerisFoldingJumped;
        wallJump = movementScript.isWallJumping;
        animator.SetBool("isFoldjumping", foldJump);
        animator.SetBool("isWalljumping", wallJump);
        if(animator.GetBool("isFoldjumping") == true)
        {
            movementScript.animationControllerisFoldingJumped = false;
        }



        float sideInput = movementScript.sideWaysInput;
        float forwardInput = movementScript.forwardInput;
        animator.SetFloat(SideInputHash, sideInput);
        animator.SetFloat(ForwardInputHash, forwardInput);
        float slideInput = movementScript.slidingInput;
        animator.SetFloat(SlideInputHash, slideInput);



        if(animator.GetBool("isJumping") == true)
        {
            jumpingTimer += Time.deltaTime;
            if (jumpingTimer > 0.1f)
            {
                animator.SetBool("isJumping", false);
                jumpingTimer = 0;
            }
        }

        GroundedCheck();
        Sliding();
        Swinging();
        Falling();
        HeadAim();
        FallImpact();
        DismountingTop();
        ladderStateChange();
        MovementParameters();
        RocketJump();

        if(useFeetIK && footIKScript != null)
        {
            CheckIK();
        }
    }
    void MovementParameters()
    {
        move = movementScript.playerVelocity;
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);
        forwardAmount = move.z;
        turnAmount = Mathf.Atan2(move.x, move.z);

        animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
        animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
    }

    void CheckIK()
    {
        if(movementScript.playerState == PlayerMovementStateMachine.PlayerState.sliding || movementScript.playerState == PlayerMovementStateMachine.PlayerState.inTheAir)
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
        if (ladderScript.isFoldingUp && movementScript.playerState == PlayerMovementStateMachine.PlayerState.sliding)
        {
            animator.SetBool("isFoldingUp", true);
            //Audio
            if (!foldAudioPlaying)
            {
                audioManager.Play("FoldLadder");
                foldAudioPlaying = true;
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
        if(airTimer >= 0.1f)
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
        if(airTimer >= timeForHardLanding)
        {
            canHardLand = true;
            canLand = false;
            canRoll = false;
        }
        if(canLand && controller.isGrounded)
        {
            canLand = false;
            audioManager.Play("LandingAfterJump");
            if(lastAirTime >= timeForLanding)
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
            audioManager.Play("LandingAfterJump");
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
            audioManager.Play("LandingAfterFall");
            playerControlsMap.Disable();
            StartCoroutine(ImpactInput());
        }
        else
        {
            animator.SetBool("isHardLanding", false);
        }
    }

    IEnumerator ImpactInput()
    {
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
                audioManager.Play("Falling");
                fallAudioPlaying = true;
            }


            if (fallTimer >= 0.2f)
            {
                animator.SetBool("FallDelay", true);
            }
        }
        else if(controller.isGrounded)
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

    void LadderJump()
    {
        animator.SetBool("isJumping", true);
        animator.SetBool("isClimbingLadder", false);
        rigBuilder.enabled = true;        


        //Audio
        audioManager.Play("JumpStart");
    }

    void RocketJump()
    {
        if (movementScript.isRocketJumping && !isRocketJumping)
        {
            animator.SetBool("isRocketJumping", true);
            isRocketJumping = true;         
        }
        if (isRocketJumping)
        {
            rocketJumpTimer += Time.deltaTime;
        }

        if(rocketJumpTimer >= 0.2)
        {
            animator.SetBool("isRocketJumping", false);
            rocketJumpTimer = 0;

        }

        if(!movementScript.isRocketJumping)
        {
            animator.SetBool("isRocketJumping", false);
            isRocketJumping = false;
        }
    }

    void Sliding()
    {
        if (movementScript.playerState == PlayerMovementStateMachine.PlayerState.sliding)
        {
            animator.SetBool("isClimbingLadder", true);
            armRig.weight = 0;

            //Slide Audio
            if (movementScript.slidingInput != 0 && !slideAudioPlaying)
            {
                audioManager.Play("Sliding");
                slideAudioPlaying = true;
            }
            if(movementScript.slidingInput == 0 && slideAudioPlaying)
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
                audioManager.Play("AttachLadder");
                attachAudioPlaying = true;
            }
        }
        else
        {
            animator.SetBool("isClimbingLadder", false);
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

    void Swinging()
    {
        if(movementScript.playerState == PlayerMovementStateMachine.PlayerState.swinging)
        {
            animator.SetBool("isSwingingLadder", true);
            armRig.weight = 0;

            //Slide Audio
            if (movementScript.slidingInput != 0 && !slideAudioPlaying)
            {
                audioManager.Play("Sliding");
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
                audioManager.Play("AttachLadder");
                attachAudioPlaying = true;
            }
        }
        else
        {
            animator.SetBool("isSlidingLadder", false);
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
