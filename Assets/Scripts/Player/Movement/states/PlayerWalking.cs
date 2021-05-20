using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
        controller.transform.SetParent(PlayerStateMachine.myParent);
        PlayerStateMachine.ladder.transform.localScale = new Vector3(1, 1, 1);
        controller.transform.localScale = new Vector3(1, 1, 1);
        PlayerStateMachine.ladder.transform.SetParent(PlayerStateMachine.animController.spine);
        PlayerStateMachine.ladder.localPosition = PlayerStateMachine.ladderWalkingPosition;
        PlayerStateMachine.ladder.localRotation = PlayerStateMachine.ladderWalkingRotation;

        values = PlayerStateMachine.valuesAsset;
    }

    public override void Movement()
    {
        Transform cam = Camera.main.transform;
        PlayerMovementStateMachine pSM = PlayerStateMachine;
        Vector3 directionForward = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        Vector3 directionRight = new Vector3(cam.right.x, 0, cam.right.z).normalized;
        Vector3 direction = directionForward * pSM.forwardInput + directionRight * pSM.sideWaysInput;

        /* Philips snapping
        if(pSM.slidingInput!=0 || pSM.swingingInput !=0)
        {
            pSM.TryToSnapToShelf();
        }
        */

        if (direction != Vector3.zero)
        {
            controller.transform.forward = Vector3.Lerp(controller.transform.forward, direction, 20 * Time.fixedDeltaTime);
        }

        pSM.baseVelocity += direction * Time.fixedDeltaTime * values.movementAcceleration;
        #region apply drag when no input is applied
        if (pSM.forwardInput == 0)
        {
            Vector3 currentDragForward = values.movementDrag * pSM.resultingVelocity(pSM.baseVelocity, directionForward);
            pSM.baseVelocity -= currentDragForward * Time.fixedDeltaTime;

        }
        if (pSM.sideWaysInput == 0)
        {
            Vector3 currentDragSideways = values.movementDrag * pSM.resultingVelocity(pSM.baseVelocity, directionRight);
            pSM.baseVelocity -= currentDragSideways * Time.fixedDeltaTime;
        }
        #endregion

        #region rounding the play velocity down if close to 0
        if (pSM.baseVelocity.x >= -.1f && pSM.baseVelocity.x <= .1f)
        {
            pSM.baseVelocity.x = 0;
        }
        if (pSM.baseVelocity.z >= -.1f && pSM.baseVelocity.z <= .1f)
        {
            pSM.baseVelocity.z = 0;
        }

        #endregion
        /*
        float currentDrag = pSM.movementDrag + pSM.playerVelocity.magnitude * .999f;
        pSM.playerVelocity.x = pSM.playerVelocity.normalized.x * Mathf.Clamp(pSM.playerVelocity.magnitude - currentDrag * Time.deltaTime, 0, pSM.maximumSpeed);
        pSM.playerVelocity.z = pSM.playerVelocity.normalized.z * Mathf.Clamp(pSM.playerVelocity.magnitude - currentDrag * Time.deltaTime, 0, pSM.maximumSpeed);
        */


        PlayerStateMachine.baseVelocity.y -= values.gravity * Time.fixedDeltaTime;
        pSM.baseVelocity = pSM.ClampPlayerVelocity(pSM.baseVelocity, Vector3.down, values.maxFallingSpeed);
        pSM.baseVelocity = pSM.baseVelocity.normalized * Mathf.Clamp(pSM.baseVelocity.magnitude, 0, values.maximumMovementSpeed);
        pSM.looseBonusVelocityPercentage(values.walkingBonusVelocityDrag);
        controller.Move(pSM.playerVelocity * Time.fixedDeltaTime * values.movementVelocityFactor);

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
            coyoteTime += Time.fixedDeltaTime;
        }
        return coyoteTimer < coyoteTime;
    }

    public override void Jump()
    {
        PlayerStateMachine.baseVelocity.y = values.jumpHeight;
        PlayerStateMachine.OnFall();
    }

    public override void Snap()
    {
        PlayerStateMachine.OnSnap();

    }
}
