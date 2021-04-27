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
    public CharacterController controller;
    public Animator animator;

    [Header("Animator")]
    float velocity = 0f;
    int VelocityHash;
    int SideInputHash;
    int ForwardInputHash;
    [Space]

    [Header("ImpactRolling")]
    public float airTimer;
    public float timeForRoll;
    [Space]


    InputActionMap playerControlsMap;
    InputAction jumpAction;
    InputAction moveAction;
    //InputAction snapAction;

    [Header("Arm Rigs")]
    public RigBuilder rigBuilder;

    [Header("Adjusting Head Aim Rig")]
    public Transform headAimTarget;
    [Tooltip("1 means target is right in front of PC, -1 behind. Number specifies the limit to wich point the head can turn. Suggestion about -0.65")]
    [Range(-0.5f, -0.9f)] public float headRotationValue = -0.65f;
    [Tooltip("Use Spine to slightly turn Chest with head when looking")]
    public Transform spine;
    public Rig headRig;
    //dotProduct used to determine where the HeadAimTarget is in relation to the players forward direction
    [HideInInspector] public float dotProduct;
    #endregion

    float fallTimer = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
        //movementScript = GetComponent<PlayerMovementStateMachine>();
        //controller = GetComponent<CharacterController>();

        VelocityHash = Animator.StringToHash("Velocity");
        SideInputHash = Animator.StringToHash("SideInput");
        ForwardInputHash = Animator.StringToHash("ForwardInput");

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


        float sideInput = movementScript.sideWaysInput;
        float forwardInput = movementScript.forwardInput;
        animator.SetFloat(SideInputHash, sideInput);
        animator.SetFloat(ForwardInputHash, forwardInput);

        Sliding();
        GroundedCheck();
        Falling();
        HeadAim();
        FallImpact();       
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
                airTimer -= Time.deltaTime * 2;
            }
        }

        if (!controller.isGrounded)
        {
            animator.SetBool("isGrounded", false);
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

    void FallImpact()
    {
        if (airTimer >= timeForRoll && controller.isGrounded)
        {
            animator.SetBool("isRolling", true);
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
            animator.SetBool("isJumping", false);
            animator.SetBool("isGrounded", false);

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
        //start Jump
        if (controller.isGrounded)
        {
            animator.SetBool("isJumping", true);
            animator.SetBool("isClimbingLadder", false);
            rigBuilder.enabled = true;
        }
    }

    void Sliding()
    {
        if (movementScript.playerState == PlayerMovementStateMachine.PlayerState.sliding)
        {
            animator.SetBool("isClimbingLadder", true);
            //disables ladder holding IK
            rigBuilder.enabled = false;
        }
        else
        {
            animator.SetBool("isClimbingLadder", false);
            rigBuilder.enabled = false;
        }
    }   
}
