using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalking : PlayerState
{
    CharacterController controller;

    public PlayerWalking(PlayerMovementStateMachine playerStateMachine) : base(playerStateMachine)
    {

    }

    public override IEnumerator initialize()
    {
        controller = PlayerStateMachine.controller;
        yield return null;
    }

    public override void Movement()
    {
        Vector3 direction = new Vector3(PlayerStateMachine.FowardInput, 0, PlayerStateMachine.SideWardsInput);
        controller.Move(direction * Time.deltaTime * PlayerStateMachine.movementSpeed);
        if (direction != Vector3.zero)
        {
            controller.transform.parent.transform.forward = direction;
        }
    }

    public override void Jump()
    {
        while (true)
        {
            if (Input.GetButtonDown("Jump") && controller.isGrounded)
            {
                //onFall.trigger
            }
        }
    }

    public override IEnumerator Snap()
    {
        while (true)
        {
            if (Input.GetButtonDown("Interact"))
            {

            }
            yield return null;
        }
    }

    public override IEnumerator Finish()
    {
        yield return null;
    }
}
