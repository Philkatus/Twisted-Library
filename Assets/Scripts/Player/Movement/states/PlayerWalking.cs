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
        PlayerFollowTarget.instance.FollowPlayer();
        PlayerFollowTarget.instance.DoAdjustY(false);
        ObjectManager.instance.animationStateController.EnterWalkingState();
        PSM.lastRail = null;

        ladder = PSM.ladder;
        controller = PSM.controller;

        controller.transform.SetParent(PSM.myParent);
        ladder.transform.localScale = new Vector3(1, 1, 1);
        controller.transform.localScale = new Vector3(1, 1, 1);
        ladder.transform.SetParent(PSM.myParent);

        controller.transform.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(controller.transform.up, Vector3.up, controller.transform.right), controller.transform.right) * controller.transform.rotation;
        //ladder.position = PSM.LadderWalkingPosition;
        //ladder.rotation = PSM.LadderWalkingRotation;
        stats = PSM.stats;
    }

    public override void Movement()
    {
        //  Forward aus der Kamera bekommen
        Transform cam = Camera.main.transform;
        Vector3 directionForward = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        Vector3 directionRight = new Vector3(cam.right.x, 0, cam.right.z).normalized;
        bool isSnapping = PSM.snappingStep != PlayerMovementStateMachine.SnappingStep.Finished;

        Vector3 direction = (directionForward * PSM.forwardInput + directionRight * PSM.sideWaysInput).normalized;
        float angle = 0;
        if (!isSnapping && direction != Vector3.zero)
        {
            controller.transform.forward = Vector3.Lerp(controller.transform.forward, direction, 10 * Time.fixedDeltaTime);
            angle = Vector3.Angle(controller.transform.forward, direction);
            if (angle > 90)
            {
                // ObjectManager.instance.animationStateController.TriggerTurn();
            }
        }
        //controller.transform.up = Vector3.Lerp(controller.transform.up, Vector3.up, 20 * Time.fixedDeltaTime);
        if (!isSnapping)
        {
            PSM.baseVelocity += direction * Time.fixedDeltaTime * stats.MovementAcceleration;
        }

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
        if (PSM.forwardInput == 0 && PSM.sideWaysInput == 0)
        {
            if (VoiceManager.Instance != null)
            {
                VoiceManager.Instance.resetWalkTimer();
                VoiceManager.Instance.TryToIdle();
            }

        }
        else
        {
            if (VoiceManager.Instance != null)
            {
                VoiceManager.Instance.resetIdleTimer();
                VoiceManager.Instance.TryToWalkSound();
            }

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
        if (isSnapping && PSM.closestRail!=null)
        {
            /*
            Vector3 StartingPoint = PSM.closestRail.pathCreator.path.GetPointAtDistance(PSM.currentDistance);
            Vector3 upwardsVelocity = ExtensionMethods.resultingVelocity(PSM.baseVelocity, Vector3.up);
            Vector3 forwardVelocity = ExtensionMethods.resultingVelocity(PSM.baseVelocity, PSM.transform.position - StartingPoint);
            PSM.baseVelocity = upwardsVelocity + forwardVelocity;
            */
            Vector3 PathDirection = PSM.closestRail.pathCreator.path.GetDirectionAtDistance(PSM.currentDistance);
            Vector3 SideWaysVelocity = ExtensionMethods.resultingVelocity(PSM.baseVelocity, PathDirection);
            PSM.baseVelocity -= SideWaysVelocity;
        }
        PSM.LoseBonusVelocityPercentage(stats.WalkingBonusVelocityDrag);
        controller.Move(PSM.playerVelocity * Time.fixedDeltaTime / stats.movementVelocityFactor);

        if (isGroundedWithCoyoteTime() && !isSnapping)
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
        if (PSM.snappingStep == PlayerMovementStateMachine.SnappingStep.Finished)
        {
            if (Mathf.Abs(PSM.controller.velocity.y) < 0.2f)
            {
                PlayerFollowTarget.instance.OnSimpleJump();
            }
            PSM.baseVelocity.y = stats.JumpHeight;
            PSM.jumpInputBool = false;
            if (VoiceManager.Instance != null)
                VoiceManager.Instance.TryToJumpSound();
            PSM.OnFall();
        }
    }

    public override void Snap()
    {
        PSM.OnSnap();
    }

    public override IEnumerator Finish()
    {
        ObjectManager.instance.animationStateController.ExitWalkingState();
        yield return null;
    }
}
