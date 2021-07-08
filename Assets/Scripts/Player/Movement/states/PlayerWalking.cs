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
        // PLEASE DO NOT COMMENT OUT OR TALK TO LILA IF THIS BREAKS ANYTHING ELSE!
        // PlayerFollowTarget.instance.FollowPlayer();
        PlayerFollowTarget.instance.DoAdjustY(false);

        ladder = PSM.ladder;
        controller = PSM.controller;

        controller.transform.SetParent(PSM.myParent);
        ladder.transform.localScale = new Vector3(1, 1, 1);
        controller.transform.localScale = new Vector3(1, 1, 1);
        ladder.transform.SetParent(PSM.animController.spine);

        controller.transform.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(controller.transform.up, Vector3.up, controller.transform.right), controller.transform.right) * controller.transform.rotation;
        ladder.localPosition = PSM.ladderWalkingPosition;
        ladder.localRotation = PSM.ladderWalkingRotation;
        Debug.Log(ladder.localRotation.eulerAngles);
        stats = PSM.stats;
    }

    public override void Movement()
    {
        //  Forward aus der Kamera bekommen
        Transform cam = Camera.main.transform;
        Vector3 directionForward = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        Vector3 directionRight = new Vector3(cam.right.x, 0, cam.right.z).normalized;

        Vector3 direction = (directionForward * PSM.forwardInput + directionRight * PSM.sideWaysInput).normalized;

        if (direction != Vector3.zero)
        {
            controller.transform.forward = Vector3.Lerp(controller.transform.forward, direction, 20 * Time.fixedDeltaTime);
        }
        //controller.transform.up = Vector3.Lerp(controller.transform.up, Vector3.up, 20 * Time.fixedDeltaTime);
        PSM.baseVelocity += direction * Time.fixedDeltaTime * stats.MovementAcceleration;

        #region Drag When No Input
        if (PSM.forwardInput == 0)
        {
            Vector3 currentDragForward = stats.MovementDrag * ExtensionMethods.resultingVelocity(PSM.baseVelocity, directionForward);
            PSM.baseVelocity -= currentDragForward * Time.fixedDeltaTime;
        }
        if (PSM.sideWaysInput == 0)
        {
            Vector3 currentDragSideways = stats.MovementDrag * ExtensionMethods.resultingVelocity(PSM.baseVelocity, directionRight);
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

        PSM.baseVelocity.y -= stats.Gravity * Time.fixedDeltaTime;
        PSM.baseVelocity = ExtensionMethods.ClampPlayerVelocity(PSM.baseVelocity, Vector3.down, stats.MaxFallingSpeed);
        float y = PSM.baseVelocity.y;
        PSM.baseVelocity.y = 0;
        PSM.baseVelocity = PSM.baseVelocity.normalized * Mathf.Clamp(PSM.baseVelocity.magnitude, 0, stats.MaximumMovementSpeed);
        PSM.baseVelocity.y = y;
        PSM.LoseBonusVelocityPercentage(stats.WalkingBonusVelocityDrag);
        controller.Move(PSM.playerVelocity * Time.fixedDeltaTime / stats.movementVelocityFactor);

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
        if (Mathf.Abs(PSM.controller.velocity.y) < 0.2f)
        {
            PlayerFollowTarget.instance.OnSimpleJump();
        }
        PSM.baseVelocity.y = stats.JumpHeight;
        PSM.jumpInputBool = false;
        PSM.OnFall();
    }

    public override void Snap()
    {
        PSM.OnSnap();
    }

    public override IEnumerator Finish()
    {
        yield return null;
    }

}
