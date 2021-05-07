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
    public Animator animator;
    public FootIK footIKScript;

    [Header("Test")]
    public float turnAmount;
    public float forwardAmount;
    public Vector3 move;

    [Header("Use IK")]
    public bool useFeetIK = false;

    [Header("Animator")]
    float velocity = 0f;
    int VelocityHash;
    int SideInputHash;
    int ForwardInputHash;
    int SlideInputHash;
    [Space]

    [Header("Impact")]
    public float timeForRoll;
    bool canRoll;
    public float timeForLanding;
    bool canLand;
    float airTimer;

    [Header("Jumping")]
    float fallTimer = 0;
    float jumpingTimer = 0;
    bool foldJump;
    bool wallJump;

    InputActionMap playerControlsMap;
    InputAction jumpAction;
    InputAction moveAction;
    //InputAction snapAction;

    [Header("Rigs")]
    public Rig headRig;
    public Rig armRig;
    RigBuilder rigBuilder;

    [Header("Ladder")]
    public GameObject ladderVisual;
    public GameObject ladderVisualForCode;

    [Header("Adjusting Head Aim Rig")]
    public Transform headAimTarget;
    [Tooltip("1 means target is right in front of PC, -1 behind. Number specifies the limit to wich the head can turn. Suggestion about -0.65")]
    [Range(-0.5f, -0.9f)] public float headRotationValue = -0.65f;
    [Tooltip("Use Spine to slightly turn Chest with head when looking")]
    public Transform spine;
    //dotProduct used to determine where the HeadAimTarget is in relation to the players forward direction
    [HideInInspector] public float dotProduct;
    #endregion


    void Start()
    {
        animator = GetComponent<Animator>();
        footIKScript = GetComponent<FootIK>();
        footIKScript.enabled = false;
        //movementScript = GetComponent<PlayerMovementStateMachine>();
        //controller = GetComponent<CharacterController>();
        if(ladderVisual != null && ladderVisualForCode != null)
        {
            ladderVisualForCode.SetActive(false);
            ladderVisual.SetActive(true);
        }
        VelocityHash = Animator.StringToHash("Velocity");
        SideInputHash = Animator.StringToHash("SideInput");
        ForwardInputHash = Animator.StringToHash("ForwardInput");
        SlideInputHash = Animator.StringToHash("SlideInput");

        Cursor.lockState = CursorLockMode.Locked;

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
        Falling();
        HeadAim();
        FallImpact();
        DismountingTop();
        ladderStateChange();
        MovementParameters();

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
        if (ladderScript.isFoldingUp)
        {
            animator.SetBool("isFoldingUp", true);
        }
        else
        {
            animator.SetBool("isFoldingUp", false);
        }
    }

    void FallImpact()
    {
        if(airTimer >= timeForLanding)
        {
            canLand = true;
        }
        if (airTimer >= timeForRoll)
        {
            canRoll = true;
            canLand = false;
        }
        if(canLand && controller.isGrounded)
        {
            animator.SetBool("isLanding", true);
            canLand = false;
        }
        else
        {
            animator.SetBool("isLanding", false);
        }
        if (canRoll && controller.isGrounded)
        {
            animator.SetBool("isRolling", true);
            canRoll = false;
        }
        else
        {
            animator.SetBool("isRolling", false);
        }
    }

    void Falling()
    {
        //Falling
        if (!controller.isGrounded && animator.GetBool("isClimbingLadder") == false)
        {
            //animator.SetBool("isJumping", false);
            //animator.SetBool("isGrounded", false);

            //Added Falltimer to prevent Falling before climbing
            fallTimer += Time.deltaTime;
            if(fallTimer >= 0.2f)
            {
                animator.SetBool("FallDelay", true);
            }
        }
        else if(controller.isGrounded)
        {
            animator.SetBool("FallDelay", false);
            fallTimer = 0;
        }
    }

    void LadderJump()
    {
        animator.SetBool("isJumping", true);
        animator.SetBool("isClimbingLadder", false);
        rigBuilder.enabled = true;        
    }

    void Sliding()
    {
        if (movementScript.playerState == PlayerMovementStateMachine.PlayerState.sliding || movementScript.playerState == PlayerMovementStateMachine.PlayerState.swinging )
        {
            animator.SetBool("isClimbingLadder", true);
            //disables ladder holding IK
            //rigBuilder.enabled = false;
            armRig.weight = 0;

            
            if (ladderVisual != null && ladderVisualForCode != null)
            {
                ladderVisualForCode.SetActive(true);
                ladderVisual.SetActive(false);
            }
            
        }
        else
        {
            animator.SetBool("isClimbingLadder", false);
            //rigBuilder.enabled = true;
            armRig.weight = 1;
            
            if (ladderVisual != null && ladderVisualForCode != null)
            {
                ladderVisualForCode.SetActive(false);
                ladderVisual.SetActive(true);
            }
            
        }


        //Look Direction
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
