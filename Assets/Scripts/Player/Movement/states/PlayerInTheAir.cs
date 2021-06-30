using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PlayerInTheAir : State
{
    CharacterController controller;
    ValuesScriptableObject stats;

    float wallJumpingTime;
    float initialAirMovementTimer;
    float floatingTimer;
    public PlayerInTheAir(PlayerMovementStateMachine playerStateMachine) : base(playerStateMachine)
    {

    }

    public override void Initialize()
    {
        InitializeVariables();
        float y = Mathf.Clamp(PSM.baseVelocity.y, -stats.MaxFallingSpeed, stats.MaxJumpingSpeedUp);
        PSM.baseVelocity.y = 0;
        Vector3 startBaseVelocity = PSM.baseVelocity;
        PSM.baseVelocity = PSM.baseVelocity.normalized * Mathf.Clamp(PSM.baseVelocity.magnitude, 0, stats.MaxJumpingSpeedForward);
        PSM.bonusVelocity += startBaseVelocity - PSM.baseVelocity;
        PSM.baseVelocity.y = y;
        initialAirMovementTimer = 0;

        // PLEASE DO NOT COMMENT OUT OR TALK TO LILA IF THIS BREAKS ANYTHING ELSE!
        CameraController.instance.SwitchToPlayerCam();
    }

    public override void Movement()
    {
        Transform cam = Camera.main.transform;
        Vector3 directionForward = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        Vector3 directionRight = new Vector3(cam.right.x, 0, cam.right.z).normalized;
        Vector3 direction = directionForward * PSM.forwardInput + directionRight * PSM.sideWaysInput;

        controller.transform.rotation = Quaternion.AngleAxis(-Mathf.Abs(Vector3.SignedAngle(controller.transform.up, Vector3.up, controller.transform.right) * Time.fixedDeltaTime * 4), controller.transform.right) * controller.transform.rotation;
        if (direction != Vector3.zero)
        {
            controller.transform.forward = Vector3.Lerp(controller.transform.forward, direction, 20 * Time.fixedDeltaTime);
        }

        #region Drag When No Input
        if (PSM.forwardInput == 0)
        {
            Vector3 currentDragForward = stats.JumpingDrag * ExtensionMethods.resultingVelocity(PSM.baseVelocity, directionForward);
            PSM.baseVelocity -= currentDragForward * Time.fixedDeltaTime;
            currentDragForward = stats.bonusVelocityDrag * ExtensionMethods.resultingVelocity(PSM.bonusVelocity, directionForward);
            PSM.bonusVelocity -= currentDragForward * Time.fixedDeltaTime;
        }
        if (PSM.sideWaysInput == 0)
        {
            Vector3 currentDragSideway = stats.JumpingDrag * ExtensionMethods.resultingVelocity(PSM.baseVelocity, directionRight);
            PSM.baseVelocity -= currentDragSideway * Time.fixedDeltaTime;
            currentDragSideway = stats.bonusVelocityDrag * ExtensionMethods.resultingVelocity(PSM.bonusVelocity, directionRight);
            PSM.bonusVelocity -= currentDragSideway * Time.fixedDeltaTime;
        }
        #endregion

        if (initialAirMovementTimer < stats.initialAirMovementTime)
        {
            PSM.baseVelocity += direction * Time.fixedDeltaTime * stats.InitialAirMovementAcceleration;
            initialAirMovementTimer += Time.deltaTime;
        }
        else
        {
            PSM.baseVelocity += direction * Time.fixedDeltaTime * stats.AirMovementAcceleration;
        }

        //when wall jump occured, set the isWallJumping to false after 1 sec
        wallJumpingTime += Time.fixedDeltaTime;
        if (wallJumpingTime >= 1)
        {
            PSM.isWallJumping = false;
        }
        GravityAndClamp();

        controller.Move(PSM.playerVelocity * Time.fixedDeltaTime / stats.AirVelocityFactor);
        if (HeadCollision())
        {
            PSM.baseVelocity.y -= PSM.baseVelocity.y * .9f * Time.fixedDeltaTime;
            PSM.bonusVelocity.y = PSM.bonusVelocity.y * .9f * Time.fixedDeltaTime;
        }

        if (controller.isGrounded)
        {
            base.PSM.didLadderPush = false;

            PSM.OnLand();
        }
    }

    void GravityAndClamp()
    {

        PSM.bonusVelocity.y -= stats.Gravity * Time.fixedDeltaTime;
        if (PSM.bonusVelocity.y <= 0)
        {
            if (PSM.baseVelocity.y >= 0 || floatingTimer >= stats.floatTime)
            {
                PSM.baseVelocity.y += PSM.bonusVelocity.y;
                PSM.bonusVelocity.y = 0;
            }
            else
            {
                PSM.bonusVelocity.y = 0;
                PSM.baseVelocity.y = 0;
                floatingTimer += Time.deltaTime;
            }
        }

        float ClampedVelocityY = Mathf.Clamp(PSM.baseVelocity.y, -stats.MaxFallingSpeed, stats.MaxJumpingSpeedUp);
        PSM.baseVelocity.y = 0;
        PSM.baseVelocity = PSM.baseVelocity.normalized * Mathf.Clamp(PSM.baseVelocity.magnitude, 0, stats.MaxJumpingSpeedForward);
        PSM.baseVelocity.y = ClampedVelocityY;
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
            if (stats.jumpFromLadderDirection != Vector3.zero) //just that it doesn't bug for the others TODO: put it the if statement away, only use wallJump
            {
                Vector3 fromWallVector = (Quaternion.AngleAxis(90, Vector3.up) * pathDirection).normalized;
                fromWallVector = fromWallVector * stats.jumpFromLadderDirection.z;
                Vector3 fromWallValued = new Vector3(fromWallVector.x, stats.jumpFromLadderDirection.y, fromWallVector.z);
                PSM.playerVelocity += fromWallValued;
                PSM.baseVelocity.y += stats.JumpHeight;
                PSM.isWallJumping = true;
            }
            else
            {
                PSM.baseVelocity.y += stats.JumpHeight;
            }

            PSM.coyoteTimer = stats.slidingCoyoteTime;
            PSM.animationControllerisFoldingJumped = false;
            PSM.jumpInputBool = false;
        }
        
    }

    public override void LadderPush()
    {
        if (stats.canLadderPush)
        {
            float sphereRadius = .2f;
            float maxHeight = stats.ladderLengthBig - sphereRadius;
            float acceleration = stats.LadderPushAcceleration;

            Vector3 origin = PSM.transform.position;
            LayerMask mask = LayerMask.GetMask("Environment");
            List<RaycastHit> hits = new List<RaycastHit>();
            #region CastDown
            if (!PSM.didLadderPush)
            {
                hits.AddRange(Physics.SphereCastAll(origin + Vector3.up * .5f, 1f, Vector3.down, maxHeight, mask, QueryTriggerInteraction.Ignore));
            }
            float closestDistance = Mathf.Infinity;
            RaycastHit closestHit;
            Vector3 target = Vector3.zero;
            for (int i = 0; i < hits.Count; i++)
            {
                float distance = hits[i].distance;
                if (distance < closestDistance &&
                    Vector3.Dot(hits[i].normal, Vector3.up) >= .5f &&
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
                hits.AddRange(Physics.SphereCastAll(origin, sphereRadius, Vector3.down + Vector3.forward + Vector3.right, maxHeight, mask, QueryTriggerInteraction.Ignore));
                hits.AddRange(Physics.SphereCastAll(origin, sphereRadius, Vector3.down + Vector3.forward + Vector3.left, maxHeight, mask, QueryTriggerInteraction.Ignore));
                hits.AddRange(Physics.SphereCastAll(origin, sphereRadius, Vector3.down + Vector3.back + Vector3.right, maxHeight, mask, QueryTriggerInteraction.Ignore));
                hits.AddRange(Physics.SphereCastAll(origin, sphereRadius, Vector3.down + Vector3.back + Vector3.left, maxHeight, mask, QueryTriggerInteraction.Ignore));
                //hits.AddRange(Physics.SphereCastAll(origin, sphereRadius, Vector3.down, maxHeight, mask, QueryTriggerInteraction.Ignore));
                for (int i = 0; i < hits.Count; i++)
                {
                    float distance = hits[i].distance;
                    if (distance < closestDistance) // && !PlayerStateMachine.didLadderPushInThisState)// && Vector3.Dot(hits[i].normal, Vector3.up) <= .93f)
                    {
                        closestHit = hits[i];
                        closestDistance = distance;
                        target = closestHit.point;
                        Debug.DrawLine(PSM.transform.position, hits[i].point, Color.red, 2);
                    }
                }
            }

            #endregion
            if (target != Vector3.zero)
            {

                Vector3 directionToWall = (PSM.transform.position - target).normalized;
                if (Vector3.Angle(directionToWall, Vector3.up) < 45 && !PSM.didLadderPush)
                {
                    PSM.didLadderPush = true;
                    PSM.ladderJumpTarget = target;
                    PSM.baseVelocity.y = 0;
                    PSM.foldInputBool = false;
                    PSM.jumpInputBool = false;
                    //pSM.baseVelocity = pSM.resultingVelocity(pSM.playerVelocity, (pSM.transform.position - target).normalized);
                    PSM.bonusVelocity += directionToWall * acceleration;
                    floatingTimer = 0;
                    //Debug.DrawLine(PlayerStateMachine.transform.position, target, Color.white, 5);
                    PSM.ladderSizeStateMachine.OnLadderPush();
                    PlayerFollowTarget.instance.DoAdjustY(true);

                }
                else if (Vector3.Angle(directionToWall, Vector3.up) >= 45)
                {

                    PSM.ladderJumpTarget = target;
                    PSM.baseVelocity.y = 0;
                    PSM.foldInputBool = false;
                    PSM.jumpInputBool = false;
                    Vector3 tempDirection1 = Mathf.Clamp(ExtensionMethods.resultingSpeed(PSM.playerVelocity, -directionToWall), 0, Mathf.Infinity) * -directionToWall;
                    Vector3 tempDirection2 = PSM.playerVelocity - tempDirection1;
                    floatingTimer = 0;
                    if (tempDirection2.magnitude < stats.ladderPushVelocityThreshhold)
                    {
                        tempDirection2 = directionToWall;
                    }
                    Vector3 targetDirection = (directionToWall + tempDirection2.normalized * stats.ladderPushCurrentVelocityFactor).normalized;
                    Vector3 targetVelocity = targetDirection * acceleration;
                    /*
                    Vector3 targetVelocityXZ = new Vector3(targetVelocity.x, 0, targetVelocity.z);
                    float y = targetVelocity.normalized.y * Mathf.Clamp(targetVelocity.y, 0, stats.maxJumpingSpeed);
                    PSM.baseVelocity = targetVelocityXZ.normalized * Mathf.Clamp(targetVelocityXZ.magnitude, 0, stats.maxJumpingSpeedForward);
                    PSM.baseVelocity.y = y;
                    targetVelocity -= PSM.baseVelocity;
                    */
                    PSM.bonusVelocity = targetVelocity;
                    //Debug.DrawLine(PlayerStateMachine.transform.position, target, Color.white, 5);
                    PSM.ladderSizeStateMachine.OnLadderPush();
                    PlayerFollowTarget.instance.DoAdjustY(true);

                }
            }
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
