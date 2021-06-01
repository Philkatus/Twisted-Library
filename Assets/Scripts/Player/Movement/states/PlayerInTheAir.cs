using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PlayerInTheAir : State
{
    CharacterController controller;
    ValuesScriptableObject stats;

    float wallJumpingTime;
    bool didSkewLadderPushThisState;

    public PlayerInTheAir(PlayerMovementStateMachine playerStateMachine) : base(playerStateMachine)
    {

    }

    public override void Initialize()
    {
        InitializeVariables();
        float y = Mathf.Clamp(PSM.baseVelocity.y, -stats.maxFallingSpeed, stats.maxJumpingSpeedUp);
        PSM.baseVelocity.y = 0;
        Vector3 startBaseVelocity = PSM.baseVelocity;
        PSM.baseVelocity = PSM.baseVelocity.normalized * Mathf.Clamp(PSM.baseVelocity.magnitude, 0, stats.maxJumpingSpeedForward);
        PSM.bonusVelocity += startBaseVelocity - PSM.baseVelocity;
        PSM.baseVelocity.y = y;
    }

    public override void Movement()
    {
        Transform cam = Camera.main.transform;
        Vector3 directionForward = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        Vector3 directionRight = new Vector3(cam.right.x, 0, cam.right.z).normalized;
        Vector3 direction = directionForward * PSM.forwardInput + directionRight * PSM.sideWaysInput;

        if (direction != Vector3.zero)
        {
            controller.transform.forward = direction;
        }
        #region Drag When No Input
        if (PSM.forwardInput == 0)
        {
            Vector3 currentDragForward = stats.jumpingDrag * ExtensionMethods.resultingVelocity(PSM.baseVelocity, directionForward);
            PSM.baseVelocity -= currentDragForward * Time.fixedDeltaTime;
        }
        if (PSM.sideWaysInput == 0)
        {
            Vector3 currentDragSideways = stats.jumpingDrag * ExtensionMethods.resultingVelocity(PSM.baseVelocity, directionRight);
            PSM.baseVelocity -= currentDragSideways * Time.fixedDeltaTime;
        }
        #endregion
        PSM.baseVelocity += direction * Time.fixedDeltaTime * stats.movementAcceleration * stats.airMovementFactor;

        //when wall jump occured, set the isWallJumping to false after 1 sec
        wallJumpingTime += Time.fixedDeltaTime;
        if (wallJumpingTime >= 1)
        {
            PSM.isWallJumping = false;
        }

        PSM.baseVelocity.y -= stats.gravity * Time.fixedDeltaTime;
        float ClampedVelocityY = Mathf.Clamp(PSM.baseVelocity.y, -stats.maxFallingSpeed, stats.maxJumpingSpeedUp);
        PSM.baseVelocity.y = 0;
        PSM.baseVelocity = PSM.baseVelocity.normalized * Mathf.Clamp(PSM.baseVelocity.magnitude, 0, stats.maxJumpingSpeedForward);
        PSM.baseVelocity.y = ClampedVelocityY;

        controller.Move(PSM.playerVelocity * Time.fixedDeltaTime * stats.jumpVelocityFactor);
        if (HeadCollision())
        {
            PSM.baseVelocity.y -= PSM.baseVelocity.y * .9f * Time.fixedDeltaTime;
            PSM.bonusVelocity.y -= PSM.bonusVelocity.y * .9f * Time.fixedDeltaTime;
        }

        if (controller.isGrounded)
        {
            base.PSM.didLadderPush = false;
            PSM.OnLand();
        }
    }

    public override void Snap()
    {
        PSM.didLadderPush = false;
        PSM.OnSnap();
    }

    public override void Jump()
    {
        if (PSM.coyoteTimer < stats.slidingCoyoteTime && PSM.closestRail != null)
        {
            Vector3 pathDirection = PSM.closestRail.pathCreator.path.GetDirectionAtDistance(PSM.currentDistance, EndOfPathInstruction.Stop);
            if (stats.wallJump != Vector3.zero) //just that it doesn't bug for the others TODO: put it the if statement away, only use wallJump
            {
                Vector3 fromWallVector = (Quaternion.AngleAxis(90, Vector3.up) * pathDirection).normalized;
                fromWallVector = fromWallVector * stats.wallJump.z;
                Vector3 fromWallValued = new Vector3(fromWallVector.x, stats.wallJump.y, fromWallVector.z);
                PSM.playerVelocity += fromWallValued;
                PSM.baseVelocity.y += stats.jumpHeight;
                PSM.isWallJumping = true;
            }
            else
            {
                PSM.baseVelocity.y += stats.jumpHeight;
            }

            PSM.coyoteTimer = stats.slidingCoyoteTime;
            PSM.animationControllerisFoldingJumped = false;
        }
        PSM.jumpInputBool = false;
    }

    public override void LadderPush()
    {
        float sphereRadius = .2f;
        float maxHeight = PSM.ladderSizeStateMachine.ladderLengthBig - sphereRadius;
        float acceleration = stats.rocketJumpAcceleration;

        Vector3 origin = PSM.transform.position;
        LayerMask mask = LayerMask.GetMask("Environment");
        List<RaycastHit> hits = new List<RaycastHit>();
        #region CastDown
        if (!PSM.didLadderPush)
        {
            hits.AddRange(Physics.SphereCastAll(origin, 1f, Vector3.down, maxHeight, mask, QueryTriggerInteraction.Ignore));
        }
        float closestDistance = Mathf.Infinity;
        RaycastHit closestHit;
        Vector3 target = Vector3.zero;
        for (int i = 0; i < hits.Count; i++)
        {
            float distance = hits[i].distance;
            if (distance < closestDistance &&
                Vector3.Dot(hits[i].normal, Vector3.up) >= .9f &&
                hits[i].point != Vector3.zero)
            {
                closestHit = hits[i];
                closestDistance = distance;
                target = closestHit.point;

                // Debug.Log(hits[i].normal);
                // Debug.DrawLine(PlayerStateMachine.transform.position, hits[i].point,Color.black,2);
            }
        }
        #endregion
        #region SideWaysCast

        if (target == Vector3.zero)
        {
            hits = new List<RaycastHit>();
            hits.AddRange(Physics.SphereCastAll(origin, sphereRadius, Vector3.down + Vector3.forward, maxHeight, mask, QueryTriggerInteraction.Ignore));
            hits.AddRange(Physics.SphereCastAll(origin, sphereRadius, Vector3.down + Vector3.back, maxHeight, mask, QueryTriggerInteraction.Ignore));
            hits.AddRange(Physics.SphereCastAll(origin, sphereRadius, Vector3.down + Vector3.right, maxHeight, mask, QueryTriggerInteraction.Ignore));
            hits.AddRange(Physics.SphereCastAll(origin, sphereRadius, Vector3.down + Vector3.left, maxHeight, mask, QueryTriggerInteraction.Ignore));
            hits.AddRange(Physics.SphereCastAll(origin, sphereRadius, Vector3.down, maxHeight, mask, QueryTriggerInteraction.Ignore));
            for (int i = 0; i < hits.Count; i++)
            {
                float distance = hits[i].distance;
                if (distance < closestDistance && !didSkewLadderPushThisState) // && !PlayerStateMachine.didLadderPushInThisState)// && Vector3.Dot(hits[i].normal, Vector3.up) <= .93f)
                {
                    closestHit = hits[i];
                    closestDistance = distance;
                    target = closestHit.point;
                    didSkewLadderPushThisState = true;
                    // Debug.DrawLine(PlayerStateMachine.transform.position, hits[i].point, Color.red, 2);
                }
            }
        }
        else
        {
            PSM.didLadderPush = true;
        }
        #endregion
        if (target != Vector3.zero)
        {
            PlayerMovementStateMachine pSM = PSM;
            pSM.ladderJumpTarget = target;
            pSM.baseVelocity.y = 0;
            pSM.foldInputBool = false;
            //pSM.baseVelocity = pSM.resultingVelocity(pSM.playerVelocity, (pSM.transform.position - target).normalized);
            pSM.bonusVelocity = (pSM.transform.position - target).normalized * acceleration;
            //Debug.DrawLine(PlayerStateMachine.transform.position, target, Color.white, 5);
            pSM.ladderSizeStateMachine.OnLadderPush();
        }
    }
    private void InitializeVariables()
    {
        controller = PSM.controller;
        controller.transform.SetParent(PSM.myParent);
        PSM.ladder.transform.localScale = new Vector3(1, 1, 1);
        controller.transform.localScale = new Vector3(1, 1, 1);
        PSM.ladder.transform.SetParent(PSM.animController.spine);
        PSM.ladder.localPosition = PSM.ladderWalkingPosition;
        PSM.ladder.localRotation = PSM.ladderWalkingRotation;
        stats = PSM.stats;

        controller = PSM.controller;
        wallJumpingTime = 0;
    }
    bool HeadCollision()
    {
        if (controller.collisionFlags == CollisionFlags.Above)
        {
            return true;
        }


        return false;
    }
}
