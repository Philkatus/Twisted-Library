using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWalking : State
{
    CharacterController controller;
    PlayerMovementStateMachine pSM;
    Transform ladder;
    ValuesScriptableObject stats;
    float coyoteTime = 0.1f,
        coyoteTimer = 0,
        speedDeadzone = 0.1f;

    public PlayerWalking(PlayerMovementStateMachine playerStateMachine) : base(playerStateMachine)
    {

    }

    public override void Initialize()
    {
        pSM = PSM;
        ladder = pSM.ladder;
        controller = pSM.controller;

        controller.transform.SetParent(PSM.myParent);
        ladder.transform.localScale = new Vector3(1, 1, 1);
        controller.transform.localScale = new Vector3(1, 1, 1);
        ladder.transform.SetParent(PSM.animController.spine);
        ladder.localPosition = pSM.ladderWalkingPosition;
        ladder.localRotation = pSM.ladderWalkingRotation;

        stats = PSM.stats;
    }

    public override void Movement()
    {
        //  Forward aus der Kamera bekommen
        Transform cam = Camera.main.transform;
        Vector3 directionForward = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        Vector3 directionRight = new Vector3(cam.right.x, 0, cam.right.z).normalized;

        Vector3 direction = directionForward * pSM.forwardInput + directionRight * pSM.sideWaysInput;

        if (direction != Vector3.zero)
        {
            controller.transform.forward = Vector3.Lerp(controller.transform.forward, direction, 20 * Time.fixedDeltaTime);
        }
        pSM.baseVelocity += direction * Time.fixedDeltaTime * stats.movementAcceleration;

        #region Drag When No Input
        if (pSM.forwardInput == 0)
        {
            Vector3 currentDragForward = stats.movementDrag * ExtensionMethods.resultingVelocity(pSM.baseVelocity, directionForward);
            pSM.baseVelocity -= currentDragForward * Time.fixedDeltaTime;
        }
        if (pSM.sideWaysInput == 0)
        {
            Vector3 currentDragSideways = stats.movementDrag * ExtensionMethods.resultingVelocity(pSM.baseVelocity, directionRight);
            pSM.baseVelocity -= currentDragSideways * Time.fixedDeltaTime;
        }
        #endregion

        #region Speed Deadzone

        if (pSM.baseVelocity.x >= -speedDeadzone && pSM.baseVelocity.x <= speedDeadzone)
        {
            pSM.baseVelocity.x = 0;
        }
        if (pSM.baseVelocity.z >= -speedDeadzone && pSM.baseVelocity.z <= speedDeadzone)
        {
            pSM.baseVelocity.z = 0;
        }

        #endregion

        pSM.baseVelocity.y -= stats.gravity * Time.fixedDeltaTime;
        pSM.baseVelocity = ExtensionMethods.ClampPlayerVelocity(pSM.baseVelocity, Vector3.down, stats.maxFallingSpeed);
        pSM.baseVelocity = pSM.baseVelocity.normalized * Mathf.Clamp(pSM.baseVelocity.magnitude, 0, stats.maximumMovementSpeed);
        pSM.LoseBonusVelocityPercentage(stats.walkingBonusVelocityDrag);
        controller.Move(pSM.playerVelocity * Time.fixedDeltaTime * stats.movementVelocityFactor);

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
        PSM.baseVelocity.y = stats.jumpHeight;
        PSM.jumpInputBool = false;
        PSM.OnFall();
    }

    public override void Snap()
    {
        PSM.OnSnap();

    }

}
