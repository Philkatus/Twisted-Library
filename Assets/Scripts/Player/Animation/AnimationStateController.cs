using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

public class AnimationStateController : MonoBehaviour
{
    #region [Grey] old variables
    [Header("References")]
    public PlayerMovementStateMachine playerSM;
    public LadderSizeStateMachine ladderSM;
    public CharacterController controller;
    public FootstepSoundManager soundManager;
    public AudioManager audioManager;
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
    public float strongImpactTimer;
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

    #region [Attributes] NEW VARS
    public Animator animator;

    private Vector2 velocity_vec;

    // Movement Attributes
    float velocityZ = 0f;
    float velocityX = 0f;
    float velocityY = 0f;

    [SerializeField] float acceleration = 2f;
    [SerializeField] float deceleration = 2f;
    #endregion

    void Awake()
    {
        ObjectManager.instance.animationStateController = this;
        headRig.weight = 1;
        armRig.weight = 1;
    }

    void Start()
    {
        #region OLD
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
        playerControlsMap = playerSM.actionAsset.FindActionMap("PlayerControls");
        stats = playerSM.stats;
        jumpAction = playerControlsMap.FindAction("Jump");
        // jumpAction.performed += context => Jump();
        #endregion

    }

    void Update()
    {
        // Time.timeScale = 0.3f;
        #region OLD
        // //ignoring the y velocity
        velocity_vec = new Vector2(playerSM.sideWaysInput, playerSM.forwardInput);
        // //animator.SetFloat(VelocityHash, velocity);

        // foldJump = playerSM.animationControllerisFoldingJumped;
        // wallJump = playerSM.isWallJumping;
        // animator.SetBool("isFoldjumping", foldJump);
        // animator.SetBool("isWalljumping", wallJump);
        // if (animator.GetBool("isFoldjumping") == true)
        // {
        //     playerSM.animationControllerisFoldingJumped = false;
        // }
        // float forwardInput = playerSM.forwardInput;
        // animator.SetFloat(ForwardInputHash, forwardInput);
        // float slideInput = playerSM.slidingInput;
        // animator.SetFloat(SlideInputHash, slideInput);

        // if (animator.GetBool("isJumping") == true)
        // {
        //     jumpingTimer += Time.deltaTime;
        //     if (jumpingTimer > 0.1f)
        //     {
        //         animator.SetBool("isJumping", false);
        //         jumpingTimer = 0;
        //     }
        // }

        // GroundedCheck();
        // //Sliding();
        // Swinging();
        // Falling();

        HeadAim();

        PlayerOnLadder();
        // ladderStateChange();
        AnimateMovement();
        PlayerAirborne();
        // LadderPush();

        // if (useFeetIK && footIKScript != null)
        // {
        //     CheckIK();
        // }
        #endregion
    }

    void LateUpdate()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    void AnimateMovement()
    {
        // run forward
        if (Mathf.Abs(velocity_vec.y) > 0f && velocityZ < 2f)
        {
            velocityZ += Time.deltaTime * acceleration;
        }

        // run left
        if (velocity_vec.x < 0 && velocityX > -1f)
        {
            velocityX -= Time.deltaTime * acceleration;
        }

        // run right
        if (velocity_vec.x > 0 && velocityX < 1f)
        {
            velocityX += Time.deltaTime * acceleration;
        }

        // decrease velocityZ
        if (velocity_vec.y == 0f && velocityZ > 0f)
        {
            velocityZ -= Time.deltaTime * deceleration;
        }

        // reset velocity Z
        if (velocity_vec.y == 0f && velocityZ < 0f)
        {
            velocityZ = 0f;
        }

        // decrease velocityX
        if (Mathf.Abs(velocity_vec.x) <= 0.4f && velocityX < 0f)
        {
            velocityX += Time.deltaTime * deceleration;
        }

        if (Mathf.Abs(velocity_vec.x) <= 0.4f && velocityX > 0f)
        {
            velocityX -= Time.deltaTime * deceleration;
        }

        if (Mathf.Abs(velocity_vec.x) <= 0.1f && velocityX != 0f && velocityX > -0.1f && velocityX < 0.1f)
        {
            velocityX = 0f;
        }

        animator.SetFloat("Velocity Z", velocityZ);
        animator.SetFloat("Velocity X", velocityX);


        // move = playerSM.playerVelocity;
        // if (move.magnitude > 1f) move.Normalize();
        // move = transform.InverseTransformDirection(move);

        // forwardAmount = move.z * antiDrag;
        // turnAmount = Mathf.Atan2(move.x, move.z);


        // animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
        // animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);

        // if(playerSM.playerState == PlayerMovementStateMachine.PlayerState.inTheAir){
        //     SetJumpPhase();
        // }
    }


    // EXIT/ENTER METHODS --------------------------------------------------------------------------------------
    #region[rgba(236,240,241,0.05)] 
    public void EnterWalkingState()
    {
        if (playerSM.CheckIfOnWater())
        {
            audioManager.PlayRandom("LandingAfterJumpWater");
        }
        else
        {
            audioManager.PlayRandom("LandingAfterJump");
        }
        animator.SetBool("Walking", true);
        //if (animator.GetBool("HardFall") && velocity_vec.magnitude < 0.1f)
        //{
        //    StartCoroutine(ImpactStun());
        //}
    }
    public void ExitWalkingState()
    {
        animator.SetBool("Walking", false);
    }

    public void EnterAirborneState()
    {
        SetFallPhase();
        animator.SetBool("Airborne", true);
        animator.SetBool("HardFall", false);
        airTimer = 0;
    }
    public void ExitAirborneState()
    {
        animator.SetBool("Airborne", false);
    }

    public void EnterOnLadderState()
    {
        animator.SetBool("OnLadder", true);

    }
    public void ExitOnLadderState()
    {
        animator.SetBool("OnLadder", false);

    }
    #endregion
    // EXIT/ENTER METHODS END -----------------------------------------------------------------------------------


    //JUMPING HELPER FUNCTIONS ----------------------------------------------------------------------------------
    #region[rgba(20,240,241,0.05)]
    void PlayerAirborne()
    {
        CheckFallImpact();
    }

    public void SetFallPhase()
    {
        animator.SetBool("IsFalling", true);
    }

    public void SetJump()
    {
        // animator.SetBool("Jump", true);
        animator.SetTrigger("JumpTrigger");
    }

    public void UnsetJump()
    {
        animator.SetBool("Jump", false);
    }

    public void TriggerDoubleJump()
    {
        Debug.Log("DOUBLE");
        animator.SetTrigger("DoubleJump");
    }

    void CheckFallImpact()
    {
        // if (airTimer >= 0.1f)
        // {
        //     canLand = true;

        // }
        // /*
        // if (airTimer >= timeForRoll)
        // {
        //     canRoll = true;
        //     canLand = false;
        // }
        // */

        if (playerSM.playerState == PlayerMovementStateMachine.PlayerState.inTheAir)
        {
            airTimer += Time.deltaTime;

        }
        if (airTimer >= strongImpactTimer)
        {
            // canHardLand = true;
            // canLand = false;
            // canRoll = false;
            animator.SetBool("HardFall", true);
        }

        if (false)
        {
            if (!animator.GetBool("HardFall") && controller.isGrounded)
            {
                // if (lastAirTime >= timeForLanding)
                // {
                //     animator.SetBool("isLanding", true);
                // }
            }
            // else
            // {
            //     // animator.SetBool("isLanding", false);
            // }
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
            // if (animator.GetBool("HardFall") && controller.isGrounded && velocity_vec.y > 0.1)
            // {
            //     // animator.SetBool("isRolling", true);
            //     // canHardLand = false;
            //     audioManager.PlayRandom("LandingAfterJump", playerSM.transform.position);
            // }
            // else
            // {
            //     animator.SetBool("isRolling", false);
            // }
            //HardImpact after fall if Input == 0
            // if (animator.GetBool("HardFall") && controller.isGrounded && velocity_vec.y < 0.1)
            // {
            //     animator.SetBool("isHardLanding", true);
            //     canHardLand = false;
            //     audioManager.PlayRandom("LandingAfterFall", playerSM.transform.position);
            //     StartCoroutine(ImpactInput());
            // }
            // else
            // {
            //     animator.SetBool("isHardLanding", false);
            // }
        }
    }

    IEnumerator ImpactStun()
    {
        playerControlsMap.Disable();
        yield return new WaitForSeconds(0.5f);
        playerControlsMap.Enable();
    }
    #endregion
    //JUMPING HELPER FUNCTIONS END -------------------------------------------------------------------------------

    // SNAP FUNCTIONS --------------------------------------------------------------------------------------------
    #region[rgba(12,70,255,0.09)] 
    public void Snap()
    {
        animator.SetTrigger("Snap");
    }
    #endregion
    // SNAP FUNCTIONS END ----------------------------------------------------------------------------------------


    //OnLadder ---------------------------------------------------------------------------------------------------
    #region[Methods] ONLADDER METHODS
    float timer = 0;
    void PlayerOnLadder()
    {
        if (animator.GetBool("OnLadder"))
        {
            DismountingTop();
            LadderClimb();
        }
    }

    void LadderClimb()
    {
        if (playerSM.HeightOnLadder < 0 && playerSM.HeightOnLadder > -.75f)
        {
            // if (Mathf.Abs(velocity_vec.y) > 0f)
            // climb up
            if (velocity_vec.y > 0f && velocityY < 1f)
            {
                velocityY += Time.deltaTime * acceleration * 5;
            }

            // climb down
            if (velocity_vec.y < 0f && velocityY > -1f)
            {
                velocityY -= Time.deltaTime * acceleration * 5;
            }

            // decrease velocityY
            if (Mathf.Abs(velocity_vec.y) <= 0.4f && velocityY < 0f)
            {
                velocityY += Time.deltaTime * deceleration;
            }
            if (Mathf.Abs(velocity_vec.y) <= 0.4f && velocityY > 0f)
            {
                velocityY -= Time.deltaTime * deceleration;
            }

            // reset velocityY
            if (Mathf.Abs(velocity_vec.y) <= 0.1f && velocityY != 0f && velocityY > -0.1f && velocityY < 0.1f)
            {
                velocityY = 0f;
            }
        }
        else
        {
            velocityY = 0f;
        }


        animator.SetFloat("ClimbingDirection", velocityY);
    }

    void DismountingTop()
    {
        if (playerSM.dismounting == true)
        {
            animator.SetBool("isDismounting", true);
        }
        else
        {
            animator.SetBool("isDismounting", false);
        }
    }

    public void DismountLadder()
    {
        animator.SetTrigger("Dismount");
    }
    #endregion
    //OnLadder END -----------------------------------------------------------------------------------------------





    void CheckIK()
    {
        // Im sorry if i fucked this up? - Maria
        if (playerSM.playerState == PlayerMovementStateMachine.PlayerState.swinging || playerSM.playerState == PlayerMovementStateMachine.PlayerState.inTheAir)
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
        if (playerSM.ladderState == PlayerMovementStateMachine.LadderState.LadderFold)
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
        if (ladderSM.isFoldingUp && playerSM.playerState == PlayerMovementStateMachine.PlayerState.swinging)
        {
            animator.SetBool("isFoldingUp", true);
            //Audio
            if (!foldAudioPlaying)
            {
                audioManager.PlayRandom("FoldLadder", ladderSM.transform.position);
                foldAudioPlaying = true;
            }
        }
        else
        {
            animator.SetBool("isFoldingUp", false);
            foldAudioPlaying = false;
        }
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
        if (playerSM.playerState == PlayerMovementStateMachine.PlayerState.walking || playerSM.playerState == PlayerMovementStateMachine.PlayerState.swinging)
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
        if (playerSM.didLadderPush && !isLadderPushing)
        {
            animator.SetBool("isRocketJumping", true);
            isLadderPushing = true;
            if (ladderPushSmoke != null)
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

        if (!playerSM.didLadderPush)
        {
            animator.SetBool("isRocketJumping", false);
            isLadderPushing = false;
        }
    }

    void Swinging()
    {
        if (playerSM.playerState == PlayerMovementStateMachine.PlayerState.swinging)
        {
            animator.SetBool("isSwingingLadder", true);
            armRig.weight = 0;

            //Slide Audio
            if (playerSM.slidingInput != 0 && !slideAudioPlaying)
            {
                audioManager.PlayRandom("Sliding");
                slideAudioPlaying = true;
            }
            if (playerSM.slidingInput == 0 && slideAudioPlaying)
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
                audioManager.PlayRandom("AttachLadder", ladderSM.transform.position);
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

}
