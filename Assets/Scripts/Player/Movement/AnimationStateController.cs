using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    public PlayerMovementStateMachine movementScript;
    public CharacterController controller;

    public Animator animator;
    float velocityZ = 0f;
    float velocityX = 0f;
    public float acceleration = 2f;
    public float deceleration = 2f;
    public float maxWalkVelocity = 0.5f;
    public float maxRunVelocity = 2f;

    //Increase Performance by Refactoring
    int VelocityZHash;
    int VelocityXHash;

    public bool isAirborne = false;

    #region Tutorial 1 variables
    /*
    int isWalkingHash;
    int isRunningHash;
    */
    #endregion
    #region Tutorial 1.2 variables
    /*
    float velocity = 0f;
    public float acceleration = 0.1f;
    int VelocityHash;
    public float deceleration = 0.5f;
    */
    #endregion



    void Start()
    {
        animator = GetComponent<Animator>();
        movementScript = GetComponent<PlayerMovementStateMachine>();
        controller = GetComponent<CharacterController>();

        VelocityZHash = Animator.StringToHash("VelocityZ");
        VelocityXHash = Animator.StringToHash("VelocityX");

        Cursor.lockState = CursorLockMode.Locked;
        #region Tutorial 1 Start
        /*
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        #endregion
        #region Tutorial 1.2 Start
        VelocityHash = Animator.StringToHash("Velocity");
        */
        #endregion
    }
    //handles acceleration and deceleration
    void changeVelocity(bool forwardPressed, bool leftPressed, bool rightPressed, bool runPressed, float currentMaxVelocity)
    {
        //Accelerate forward
        if (forwardPressed && velocityZ < currentMaxVelocity)
        {
            velocityZ += Time.deltaTime * acceleration;
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
        //Decelerate Z
        if (!forwardPressed && velocityZ > 0f)
        {
            velocityZ -= Time.deltaTime * deceleration;
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
        if (controller.isGrounded && Input.GetKey(KeyCode.Space))
        {
            animator.SetBool("isJumping", true);
        }

    }

    void lockOrResetVelocity(bool forwardPressed, bool leftPressed, bool rightPressed, bool runPressed, float currentMaxVelocity)
    {
        //lock negative speed
        if (!forwardPressed && velocityZ < 0f)
        {
            velocityZ = 0f;
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
            velocityX += Time.deltaTime + deceleration;
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
    void Update()
    {
        #region Tutorial 1 Update
        /*
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);
        bool forwardPress = Input.GetKey(KeyCode.W);
        bool runPressed = Input.GetKey(KeyCode.LeftShift);

        if (!isWalking && forwardPress)
        {
            animator.SetBool(isWalkingHash, true);
        }

        if (isWalking && !forwardPress)
        {
            animator.SetBool(isWalkingHash, false);
        }

        if(!isRunning && (forwardPress && runPressed))
        {
            animator.SetBool(isRunningHash, true);
        }

        if (isRunning && (!forwardPress || !runPressed))
        {
            animator.SetBool(isRunningHash, false);
        }
        */
        #endregion
        #region Tutorial 1.2 Update
        /*
        bool forwardPress = Input.GetKey(KeyCode.W);
        bool runPressed = Input.GetKey(KeyCode.LeftShift);

        if (forwardPress && velocity < 1f)
        {
            velocity += Time.deltaTime * acceleration;
        }

        if (!forwardPress && velocity > 0f)
        {
            velocity -= Time.deltaTime * deceleration;
        }

        if (!forwardPress && velocity < 0f)
        {
            velocity = 0f;
        }

        animator.SetFloat(VelocityHash, velocity);
        */
        #endregion

        bool forwardPressed = Input.GetKey(KeyCode.W);
        bool leftPressed = Input.GetKey(KeyCode.A);
        bool rightPressed = Input.GetKey(KeyCode.D);
        bool runPressed = Input.GetKey(KeyCode.LeftShift);

        //set current maxVelocity (if runPressed:  =true             =false)
        float currentMaxVelocity = runPressed ? maxRunVelocity : maxWalkVelocity;


        changeVelocity(forwardPressed, leftPressed, rightPressed, runPressed, currentMaxVelocity);
        lockOrResetVelocity(forwardPressed, leftPressed, rightPressed, runPressed, currentMaxVelocity);

        //set the parameters to our local variable values
        animator.SetFloat(VelocityZHash, velocityZ);
        animator.SetFloat(VelocityXHash, velocityX);

        Jump();
    }
}
