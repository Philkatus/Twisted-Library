using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalking : PlayerState
{

    public PlayerWalking(PlayerMovementStateMachine playerStateMachine) : base(playerStateMachine)
    {
    }

    public override IEnumerator initialize()
    {

        yield return null;
    }

    public override void Movement()
    {
        Vector3 direction = new Vector3(PlayerStateMachine.FowardInput, 0, PlayerStateMachine.SideWardsInput);
        PlayerStateMachine.controller.Move(direction * Time.deltaTime * PlayerStateMachine.speed);
        if (direction != Vector3.zero)
        {
            PlayerStateMachine.controller.transform.parent.transform.forward = direction;
        }
        PlayerStateMachine.CheckForShelf();
    }

    public override void Jump()
    {
        while (true)
        {
            if (Input.GetButtonDown("Jump") && PlayerStateMachine.controller.isGrounded)
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
