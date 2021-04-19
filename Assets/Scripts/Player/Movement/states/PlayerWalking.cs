using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalking : State
{
    CharacterController controller;
    float coyoteTime = 0.1f;
    float coyoteTimer = 0;

    public PlayerWalking(PlayerMovementStateMachine playerStateMachine) : base(playerStateMachine)
    {

    }

    public override IEnumerator Initialize()
    {

        controller = PlayerStateMachine.controller;
        yield return null;
    }

    public override void Movement()
    {
        Transform cam = Camera.main.transform;
        PlayerMovementStateMachine pSM = PlayerStateMachine;
        Vector3 directionForward = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        Vector3 directionRight = new Vector3(cam.right.x, 0, cam.right.z).normalized;
        Vector3 direction = directionForward * pSM.ForwardInput + directionRight * pSM.sideWaysInput;
        if (direction != Vector3.zero)
        {
            controller.transform.forward = Vector3.Lerp(controller.transform.forward,direction,.2f);
        }

        
        pSM.playerVelocity += direction * Time.deltaTime*pSM.movementAcceleration;
        pSM.playerVelocity.x = pSM.playerVelocity.normalized.x * Mathf.Clamp(pSM.playerVelocity.magnitude - pSM.movementDrag*Time.deltaTime, 0, pSM.maximumSpeed);
        pSM.playerVelocity.z = pSM.playerVelocity.normalized.z * Mathf.Clamp(pSM.playerVelocity.magnitude - pSM.movementDrag*Time.deltaTime, 0, pSM.maximumSpeed);
        controller.Move(pSM.playerVelocity * Time.deltaTime);
           // + direction * Time.deltaTime * pSM.movementAcceleration);// Mathf.Clamp( pSM.playerVelocity.magnitude,0,pSM.maximumSpeed));

        if (isGroundedWithCoyoteTime())
        {
            pSM.playerVelocity.y -= PlayerStateMachine.gravity * Time.deltaTime;
            PlayerStateMachine.OnFall();
        }
    }

    bool isGroundedWithCoyoteTime()
    {
        if (controller.isGrounded)
        {
            coyoteTime = 0;
        }
        else
        {
            coyoteTime += Time.deltaTime;
        }
        return coyoteTimer < coyoteTime;
    }

    public override void Jump()
    {
        if (controller.isGrounded)
        {
            PlayerStateMachine.playerVelocity.y = PlayerStateMachine.jumpheight;
            PlayerStateMachine.OnFall();
        }
    }

    public override IEnumerator Snap()
    {
        PlayerStateMachine.OnSnap();

        yield return null;

    }

    public override IEnumerator Finish()
    {
        yield return null;
    }
}
