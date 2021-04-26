using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalking : State
{
    CharacterController controller;
    ValuesScriptableObject values;
    float coyoteTime = 0.1f;
    float coyoteTimer = 0;

    public PlayerWalking(PlayerMovementStateMachine playerStateMachine) : base(playerStateMachine)
    {

    }

    public override void Initialize()
    {
        controller = PlayerStateMachine.controller;
        controller.transform.parent = PlayerStateMachine.myParent;
        PlayerStateMachine.ladder.transform.parent = controller.transform;
        PlayerStateMachine.ladder.localPosition = PlayerStateMachine.ladderWalkingPosition;
        PlayerStateMachine.ladder.localRotation = PlayerStateMachine.ladderWalkingRotation;
        
        PlayerStateMachine.playerVelocity.y = -1f;
        values = PlayerStateMachine.valuesAsset;
    }

    public override void Movement()
    {
        Transform cam = Camera.main.transform;
        PlayerMovementStateMachine pSM = PlayerStateMachine;
        Vector3 directionForward = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        Vector3 directionRight = new Vector3(cam.right.x, 0, cam.right.z).normalized;
        Vector3 direction = directionForward * pSM.forwardInput + directionRight * pSM.sideWaysInput;

        if (direction != Vector3.zero)
        {
            controller.transform.forward = Vector3.Lerp(controller.transform.forward, direction, 20 * Time.deltaTime);
        }

        pSM.playerVelocity += direction * Time.deltaTime * values.movementAcceleration;
        #region apply drag when no input is applied
        if (pSM.forwardInput == 0)
        {
            Vector3 currentDragForward = values.movementDrag * pSM.resultingVelocity(pSM.playerVelocity, directionForward);
            pSM.playerVelocity -= currentDragForward * Time.deltaTime;

        }
        if (pSM.sideWaysInput == 0)
        {
            Vector3 currentDragSideways = values.movementDrag * pSM.resultingVelocity(pSM.playerVelocity, directionRight);
            pSM.playerVelocity -= currentDragSideways * Time.deltaTime;
        }
        #endregion

        #region rounding the play velocity down if close to 0
        if (pSM.playerVelocity.x >= -.1f && pSM.playerVelocity.x <= .1f)
        {
            pSM.playerVelocity.x = 0;
        }
        if (pSM.playerVelocity.z >= -.1f && pSM.playerVelocity.z <= .1f)
        {
            pSM.playerVelocity.z = 0;
        }

        #endregion
        /*
        float currentDrag = pSM.movementDrag + pSM.playerVelocity.magnitude * .999f;
        pSM.playerVelocity.x = pSM.playerVelocity.normalized.x * Mathf.Clamp(pSM.playerVelocity.magnitude - currentDrag * Time.deltaTime, 0, pSM.maximumSpeed);
        pSM.playerVelocity.z = pSM.playerVelocity.normalized.z * Mathf.Clamp(pSM.playerVelocity.magnitude - currentDrag * Time.deltaTime, 0, pSM.maximumSpeed);
        */
        pSM.playerVelocity = pSM.playerVelocity.normalized * Mathf.Clamp(pSM.playerVelocity.magnitude, 0, values.maximumMovementSpeed);
        controller.Move(pSM.playerVelocity * Time.deltaTime);
        PlayerStateMachine.playerVelocity.y -= values.gravity * Time.deltaTime;
        if (isGroundedWithCoyoteTime())
        {
            pSM.OnFall();
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
        PlayerStateMachine.playerVelocity.y = values.jumpHeight;


        PlayerStateMachine.OnFall();
    }

    public override IEnumerator Snap()
    {
        PlayerStateMachine.OnSnap();
        yield return null;
    }
}
