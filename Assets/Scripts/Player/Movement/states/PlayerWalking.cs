using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWalking : State
{
    CharacterController controller;
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
        ladder = PSM.ladder;
        controller = PSM.controller;

        controller.transform.SetParent(base.PSM.myParent);
        ladder.transform.localScale = new Vector3(1, 1, 1);
        controller.transform.localScale = new Vector3(1, 1, 1);
        ladder.transform.SetParent(base.PSM.animController.spine);
        ladder.localPosition = PSM.ladderWalkingPosition;
        ladder.localRotation = PSM.ladderWalkingRotation;

        stats = base.PSM.stats;
    }

    public override void Movement()
    {
        //  Forward aus der Kamera bekommen
        Transform cam = Camera.main.transform;
        Vector3 directionForward = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        Vector3 directionRight = new Vector3(cam.right.x, 0, cam.right.z).normalized;

        Vector3 direction = directionForward * PSM.forwardInput + directionRight * PSM.sideWaysInput;

        if (direction != Vector3.zero)
        {
            controller.transform.forward = Vector3.Lerp(controller.transform.forward, direction, 20 * Time.fixedDeltaTime);
        }
        PSM.baseVelocity += direction * Time.fixedDeltaTime * stats.movementAcceleration;

        #region Drag When No Input
        if (PSM.forwardInput == 0)
        {
            Vector3 currentDragForward = stats.movementDrag * ExtensionMethods.resultingVelocity(PSM.baseVelocity, directionForward);
            PSM.baseVelocity -= currentDragForward * Time.fixedDeltaTime;
        }
        if (PSM.sideWaysInput == 0)
        {
            Vector3 currentDragSideways = stats.movementDrag * ExtensionMethods.resultingVelocity(PSM.baseVelocity, directionRight);
            PSM.baseVelocity -= currentDragSideways * Time.fixedDeltaTime;
        }
        #endregion

        #region Speed Deadzone

        if (PSM.baseVelocity.x >= -speedDeadzone && PSM.baseVelocity.x <= speedDeadzone)
        {
            PSM.baseVelocity.x = 0;
        }
        if (PSM.baseVelocity.z >= -speedDeadzone && PSM.baseVelocity.z <= speedDeadzone)
        {
            PSM.baseVelocity.z = 0;
        }

        #endregion

        PSM.baseVelocity.y -= stats.gravity * Time.fixedDeltaTime;
        PSM.baseVelocity = ExtensionMethods.ClampPlayerVelocity(PSM.baseVelocity, Vector3.down, stats.maxFallingSpeed);
        PSM.baseVelocity = PSM.baseVelocity.normalized * Mathf.Clamp(PSM.baseVelocity.magnitude, 0, stats.maximumMovementSpeed);
        PSM.loseBonusVelocityPercentage(stats.walkingBonusVelocityDrag);
        controller.Move(PSM.playerVelocity * Time.fixedDeltaTime * stats.movementVelocityFactor);

        if (isGroundedWithCoyoteTime())
        {
            PSM.OnFall();
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
        base.PSM.baseVelocity.y = stats.jumpHeight;
        base.PSM.jumpInputBool = false;
        base.PSM.OnFall();
    }

    public override void Snap()
    {
        base.PSM.OnSnap();

    }

}
