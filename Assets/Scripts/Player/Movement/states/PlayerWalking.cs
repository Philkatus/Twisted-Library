using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalking : PlayerState
{
    CharacterController controller;

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
        Vector3 direction = directionForward * pSM.ForwardInput + directionRight * pSM.SideWaysInput;
        controller.Move(direction * Time.fixedDeltaTime * pSM.movementSpeed);
        if (direction != Vector3.zero)
        {
            controller.transform.forward = direction;
        }
    }

    public override void Jump()
    {
        /*
        while (true)
        {
            if (Input.GetButtonDown("Jump") && controller.isGrounded)
            {
                //onFall.trigger
            }
        }
        */
        
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
