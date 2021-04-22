using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AnimationStateController : MonoBehaviour
{
    #region variables
    //[Header("References")]
    [HideInInspector] public PlayerMovementStateMachine movementScript;
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public Animator animator;

    [Header("Animator")]
    float velocityZ = 0f;
    float velocityX = 0f;
    float velocity = 0f;
    public float playerVelocity;
    [Space]

    #region old but still needed?
    public float acceleration, deceleration = 2f;
    public float maxWalkVelocity = 0.5f;
    public float maxRunVelocity = 2f;
    //Increase Performance by Refactoring
    int VelocityZHash;
    int VelocityXHash;
    #endregion

    int VelocityHash;
    public bool isAirborne = false;

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

    void Start()
    {
        animator = GetComponent<Animator>();
        movementScript = GetComponent<PlayerMovementStateMachine>();
        controller = GetComponent<CharacterController>();

        #region Old but dont delete
        
        VelocityZHash = Animator.StringToHash("VelocityZ");
        VelocityXHash = Animator.StringToHash("VelocityX");
        
        #endregion
        VelocityHash = Animator.StringToHash("Velocity");

        Cursor.lockState = CursorLockMode.Locked;

        rigBuilder = GetComponent<RigBuilder>();
    }

    void Update()
    {
        playerVelocity = movementScript.playerVelocity.magnitude;

        #region old code (dont delete)
        
        velocity = playerVelocity;

        bool forwardPressed = Input.GetKey(KeyCode.W);
        bool backwardPressed = Input.GetKey(KeyCode.S);
        bool leftPressed = Input.GetKey(KeyCode.A);
        bool rightPressed = Input.GetKey(KeyCode.D);
        bool runPressed = Input.GetKey(KeyCode.LeftShift);

        
        //set current maxVelocity (if runPressed:  =true             =false)
        float currentMaxVelocity = runPressed ? maxRunVelocity : maxWalkVelocity;


       // changeVelocity(forwardPressed, backwardPressed, leftPressed, rightPressed, runPressed, currentMaxVelocity);
       // lockOrResetVelocity(forwardPressed, backwardPressed, leftPressed, rightPressed, runPressed, currentMaxVelocity);

        //set the parameters to our local variable values
        animator.SetFloat(VelocityZHash, velocityZ);
        animator.SetFloat(VelocityXHash, velocityX);
        
        #endregion

        animator.SetFloat(VelocityHash, velocity);

        Jump();
        LadderAnims();
        HeadAim();
    }

    void HeadAim()
    {
        Vector3 toTarget = (headAimTarget.position - transform.position).normalized;
        dotProduct = Vector3.Dot(toTarget, transform.forward);

        //Decreases Rigs weight on the head if look target is too far behind PC
        if (Vector3.Dot(toTarget, transform.forward) < headRotationValue && headRig.weight > 0.1f)
        {
            headRig.weight -=Time.deltaTime * 3;  
        }
        //Increases weigth again after dotproduct has crossed threshold
        else if (Vector3.Dot(toTarget, transform.forward) > headRotationValue && headRig.weight < 1f)
        {
            headRig.weight += Time.deltaTime * 3;
        }
    }

    void LadderAnims()
    {
        if(movementScript.closestShelf != null)
        {
            animator.SetBool("isClimbingLadder", true);
            rigBuilder.enabled = false;
        } 
        /*
        else
        {
            animator.SetBool("isClimbingLadder", false);
            rigBuilder.enabled = true;
        }
        */
        if (controller.isGrounded)
        {
            animator.SetBool("isClimbingLadder", false);
            rigBuilder.enabled = true;
        }
        
    }

    void Jump()
    {
        //Falling
        if (controller.isGrounded)
        {
            animator.SetBool("isGrounded", true);
        }
        if (!controller.isGrounded)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isGrounded", false);
        }

        //start Jump
        if (controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            animator.SetBool("isJumping", true);
            animator.SetBool("isClimbingLadder", false);
            rigBuilder.enabled = true;
        }

    }

    //WASD Hardcoded BUT with strafing DONT DELETE
    #region old
    //handles acceleration and deceleration
    void changeVelocity(bool forwardPressed, bool backwardPressed, bool leftPressed, bool rightPressed, bool runPressed, float currentMaxVelocity)
    {
        //Accelerate forward
        if (forwardPressed && velocityZ < currentMaxVelocity)
        {
            velocityZ += Time.deltaTime * acceleration;
        }
        //Accelerate "backwards"
        if (backwardPressed && velocityZ > -currentMaxVelocity)
        {
            velocityZ -= Time.deltaTime * acceleration;
        }
        //Accelerate left
        if (leftPressed && velocityX > -currentMaxVelocity)
        {
            velocityX -= Time.deltaTime * acceleration;
        }
        //Accelerate right
        if (rightPressed && velocityX < currentMaxVelocity)
        {
            velocityX += Time.deltaTime * acceleration;
        }
        //Decelerate Z forward to standstill
        if (!forwardPressed && velocityZ > 0f)
        {
            velocityZ -= Time.deltaTime * deceleration;
        }
        //"Decelerate" Z backward to standstill
        if (!backwardPressed && velocityZ < 0f)
        {
            velocityZ += Time.deltaTime * deceleration;
        }
        //Decelerate left
        if (!leftPressed && velocityX < 0f)
        {
            velocityX += Time.deltaTime * deceleration;
        }
        //Decelerate right
        if (!rightPressed && velocityX > 0f)
        {
            velocityX -= Time.deltaTime * deceleration;
        }
    }

    void lockOrResetVelocity(bool forwardPressed, bool backwardPressed, bool leftPressed, bool rightPressed, bool runPressed, float currentMaxVelocity)
    {

        if(velocityZ == 0f && velocityX != 0f)
        {
            velocityZ = velocityX;
        }

        //reset velocityX
        if (!leftPressed && !rightPressed && velocityX != 0f && (velocityX > -0.05f && velocityX < 0.05f))
        {
            velocityX = 0f;
        }

        //Max forward speed
        if (forwardPressed && runPressed && velocityZ > currentMaxVelocity)
        {
            velocityZ = currentMaxVelocity;
        }

        //Max backward speed
        if (backwardPressed && runPressed && velocityZ < -currentMaxVelocity)
        {
            velocityZ = -currentMaxVelocity;
        }

        //decelerate to max walking speed after letting go of sprint
        else if (forwardPressed && velocityZ > currentMaxVelocity)
        {
            velocityZ -= Time.deltaTime * deceleration;
            //round to the currentMaxVelocity if within offset
            if (velocityZ > currentMaxVelocity && velocityZ < (currentMaxVelocity + 0.05))
            {
                velocityZ = currentMaxVelocity;
            }
        }
        //round to the currentMaxVelocity if within offset
        else if (forwardPressed && velocityZ < currentMaxVelocity && velocityZ > (currentMaxVelocity - 0.05f))
        {
            velocityZ = currentMaxVelocity;
        }

        //
        //locking left
        if(leftPressed && runPressed && velocityX < -currentMaxVelocity)
        {
            velocityX = -currentMaxVelocity;
        }
        //decelerate to the maximum walk velocity
        else if(leftPressed && velocityX < -currentMaxVelocity)
        {
            velocityX += Time.deltaTime * deceleration;
            // round to the currentMaxVelocity if within offset
            if(velocityX < -currentMaxVelocity && velocityX > (-currentMaxVelocity - 0.05f))
            {
                velocityX = currentMaxVelocity;
            }
        }
        // round to the currentMaxVelocity if within offset
        else if (rightPressed && velocityX < currentMaxVelocity && velocityX > (currentMaxVelocity - 0.05f))
        {
            velocityX = currentMaxVelocity;
        }
        //

    }
    #endregion

}
