using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInTheAir : State
{
    CharacterController controller;
    private Vector3 playerVelocity;

    public PlayerInTheAir(PlayerMovementStateMachine playerStateMachine) : base(playerStateMachine)
    {

    }

    public override IEnumerator Initialize()
    {
        controller = PlayerStateMachine.controller;
        yield return null;
    }

    public override void Movement()
    {
        // Air Movement
        Transform cam = Camera.main.transform;
        PlayerMovementStateMachine pSM = PlayerStateMachine;
        Vector3 directionForward = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        Vector3 directionRight = new Vector3(cam.right.x, 0, cam.right.z).normalized;
        Vector3 direction = directionForward * pSM.ForwardInput + directionRight * pSM.SideWaysInput; ;
        pSM.moveDirection = direction;

        if (direction != Vector3.zero)
        {
            controller.transform.forward = direction;
        }

        pSM.playerVelocity.y -= PlayerStateMachine.gravity * Time.deltaTime;
        controller.Move(pSM.playerVelocity * Time.deltaTime
            + direction * Time.deltaTime * pSM.movementSpeed / 1.4f * pSM.momentum);
        pSM.momentum = Mathf.Max(0, pSM.momentum - 2 * Time.deltaTime);

        if (controller.isGrounded)
        {
            pSM.OnLand();
        }
    }

    public override void Jump()
    {

    }

    public override IEnumerator Snap()
    {
        PlayerStateMachine.momentum = controller.velocity.magnitude / 8;
        PlayerStateMachine.OnSnap();

        yield return null;

    }

    public override IEnumerator Finish()
    {
        yield return null;
    }
}
