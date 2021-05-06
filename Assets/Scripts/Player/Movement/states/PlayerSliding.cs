using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using UnityEngine.InputSystem;

public class PlayerSliding : State
{
    #region INHERITED
    float currentDistance;
    float speed;
    float ladderLength;
    float pathLength;
    VertexPath path;
    Shelf closestShelf;
    CharacterController controller;
    PathCreator pathCreator;
    protected PlayerMovementStateMachine pSM;
    Transform ladder;
    LadderSizeStateMachine ladderSizeState;
    #endregion

    #region PRIVATE
    float dismountTimer;
    bool dismountedHalfways;
    bool stopping;
    Vector3 dismountStartPos;
    Vector3 pathDirection;
    protected ValuesScriptableObject stats;
    #endregion

    public override void Initialize()
    {
        // Assign variables.
        pSM = PlayerStateMachine;
        stats = pSM.valuesAsset;

        ladderSizeState = pSM.ladderSizeStateMachine;
        ladderLength = ladderSizeState.ladderLength;
        speed = stats.climbingSpeedOnLadder;
        closestShelf = pSM.closestRail;
        controller = pSM.controller;
        ladder = pSM.ladder;
        pathCreator = closestShelf.pathCreator;
        path = pathCreator.path;
        //pSM.HeightOnLadder = -1;

        // Place the ladder on the path.
        Vector3 startingPoint = Vector3.zero;
        if (closestShelf != null)
        {
            startingPoint = pathCreator.path.GetClosestPointOnPath(pSM.transform.position);
        }
        else
        {
            Debug.LogError("Shelf is null!");
        }

        currentDistance = path.GetClosestDistanceAlongPath(startingPoint);
        ladder.transform.position = startingPoint;
        ladder.transform.forward = -path.GetNormalAtDistance(currentDistance);

        pathLength = path.cumulativeLengthAtEachVertex[path.cumulativeLengthAtEachVertex.Length - 1];
        pSM.currentDistance = path.GetClosestDistanceAlongPath(startingPoint);

        // Place the character on ladder.
        ladder.transform.parent = pSM.myParent;
        Vector3 targetPosition = startingPoint - pSM.ladderDirection * ladderLength;
        targetPosition.y = Mathf.Clamp(controller.transform.position.y, targetPosition.y, startingPoint.y);
        pSM.HeightOnLadder = -(startingPoint - targetPosition).magnitude / ladderLength;
        pSM.HeightOnLadder = Mathf.Clamp(pSM.HeightOnLadder, -1, 0);
        pSM.transform.position = ladder.transform.position + pSM.ladderDirection * ladderLength * pSM.HeightOnLadder;

        controller.transform.forward = -pathCreator.path.GetNormalAtDistance(currentDistance);
        controller.transform.parent = ladder.transform;
        pSM.ladderSizeStateMachine.OnGrow();

        pathDirection = pathCreator.path.GetDirectionAtDistance(currentDistance, EndOfPathInstruction.Stop);
        if (stats.preservesVelocityOnSnap)
        {
            pSM.playerVelocity = pSM.ClampPlayerVelocity(pSM.playerVelocity, pathDirection, stats.maxSlidingSpeed);

        }
        else
        {
            pSM.playerVelocity = pSM.ClampPlayerVelocity(pSM.playerVelocity, pathDirection, 0);
        }


        pSM.stopSlidingAction.started += context => stopping = true;
        pSM.stopSlidingAction.canceled += context => stopping = false;
        Time.fixedDeltaTime = 0.002f;
    }

    public override void ReInitialize()
    {
        // Assign variables.
        pSM = PlayerStateMachine;
        stats = pSM.valuesAsset;

        ladderSizeState = pSM.ladderSizeStateMachine;
        ladderLength = ladderSizeState.ladderLength;
        speed = stats.climbingSpeedOnLadder;
        closestShelf = pSM.closestRail;
        controller = pSM.controller;
        ladder = pSM.ladder;
        pathCreator = closestShelf.pathCreator;
        path = pathCreator.path;

        // Place the ladder on the path.
        Vector3 startingPoint = Vector3.zero;
        if (closestShelf != null)
        {
            startingPoint = pathCreator.path.GetClosestPointOnPath(pSM.transform.position);
        }
        else
        {
            Debug.LogError("Shelf is null!");
        }

        currentDistance = path.GetClosestDistanceAlongPath(startingPoint);
        ladder.transform.position = startingPoint;
        ladder.transform.forward = -path.GetNormalAtDistance(currentDistance);

        pathLength = path.cumulativeLengthAtEachVertex[path.cumulativeLengthAtEachVertex.Length - 1];
        pSM.currentDistance = path.GetClosestDistanceAlongPath(startingPoint);

    }

    public override IEnumerator Finish()
    {
        
        Time.fixedDeltaTime = 0.02f;
        yield break;
    }

    public override void Jump()
    {
        if (ladderSizeState.isFoldingUp)
        {
            PlayerStateMachine.playerVelocity.y += (pSM.transform.position.y - ladderSizeState.startFoldingUpPos.y) * ladderSizeState.foldJumpMultiplier;
            Debug.Log("fold jump : " + (pSM.transform.position.y - ladderSizeState.startFoldingUpPos.y) * ladderSizeState.foldJumpMultiplier);
            PlayerStateMachine.OnFall();
            pSM.animationControllerisFoldingJumped = true;
        }
        else
        {
            if (stats.wallJump != Vector3.zero) //just that it doesn't bug for the others TODO: put it the if statement away, only use wallJump
            {
                Vector3 fromWallVector = (Quaternion.AngleAxis(90, Vector3.up) * pathDirection).normalized;
                fromWallVector = fromWallVector * stats.wallJump.z;
                Vector3 fromWallValued = new Vector3(fromWallVector.x, stats.wallJump.y, fromWallVector.z);
                PlayerStateMachine.playerVelocity += fromWallValued;
                PlayerStateMachine.isWallJumping = true;
            }
            else
            {
                PlayerStateMachine.playerVelocity.y += stats.jumpHeight;
            }

            Debug.Log("Normal slide jump");
            PlayerStateMachine.OnFall();
            pSM.animationControllerisFoldingJumped = false;
        }
    }

    public override void Movement()
    {
        if (!pSM.dismounting)
        {
            // Go up and down.
            if (!CheckForCollisionCharacter(pSM.forwardInput * pSM.ladderDirection))
            {
                pSM.HeightOnLadder += pSM.forwardInput * speed * Time.fixedDeltaTime;
                pSM.HeightOnLadder = Mathf.Clamp(pSM.HeightOnLadder, -1, 0);
                pSM.transform.position = ladder.transform.position + pSM.ladderDirection * ladderSizeState.ladderLength * pSM.HeightOnLadder; //pos on ladder
            }

            // Move horizontally.
            pathDirection = path.GetDirectionAtDistance(currentDistance);

            // Get sideways input, no input if both buttons held down.
            if (pSM.slideAction.triggered && pSM.slideAction.ReadValue<float>() == 0)
            {
                pSM.playerVelocity -= pSM.resultingVelocity(pSM.playerVelocity, pathDirection);

            }

            //playervelocity increased with input
            float slidingAcceleration = ExtensionMethods.Remap(ladderSizeState.ladderLength, ladderSizeState.ladderLengthSmall, ladderSizeState.ladderLengthBig, stats.slidingAcceleration * stats.slidingSpeedSizeFactor, stats.slidingAcceleration);
            pSM.playerVelocity += pSM.slidingInput * pathDirection * Time.fixedDeltaTime * slidingAcceleration;

            //drag calculation
            float resultingSpeed = pSM.resultingSpeed(pSM.playerVelocity, pathDirection);

            //speed Clamp (dependant on ladder size)
            float maxSlidingSpeed = ExtensionMethods.Remap(ladderSizeState.ladderLength, ladderSizeState.ladderLengthSmall, ladderSizeState.ladderLengthBig, stats.maxSlidingSpeed * stats.slidingSpeedSizeFactor, stats.maxSlidingSpeed);
            pSM.playerVelocity -= pathDirection * Mathf.Clamp(resultingSpeed * stats.slidingDragPercentage / 100, -maxSlidingSpeed, maxSlidingSpeed);

            //moving the object
            if (!CheckForCollisionCharacter(pSM.playerVelocity) && !stopping && !CheckForCollisionLadder(pSM.playerVelocity))
            {
                pSM.currentDistance += pSM.resultingSpeed(pSM.playerVelocity, pathDirection) * stats.slidingVelocityFactor;
                pSM.ladder.position = path.GetPointAtDistance(pSM.currentDistance, EndOfPathInstruction.Stop);
                //pSM.ladder.forward  = -path.GetNormalAtDistance(pSM.currentDistance);
               
                    Quaternion targetRotation = Quaternion.LookRotation(-path.GetNormalAtDistance(pSM.currentDistance), pSM.ladderDirection);
                    pSM.ladder.rotation = targetRotation;
                
                
                // Debug.Log(pSM.resultingSpeed(pSM.playerVelocity, pathDirection) * values.slidingVelocityFactor + " " + values.slidingVelocityFactor);

            }
            else
            {
                pSM.playerVelocity = pSM.ClampPlayerVelocity(pSM.playerVelocity, pathDirection, 0);
            }

            //End Of Path, continue sliding with ReSnap or Fall from Path
            if (pSM.currentDistance <= 0 || pSM.currentDistance >= pathLength)
            {

                Vector3 endOfShelfDirection = new Vector3();
                if (pSM.currentDistance <= 0) //arriving at start of path
                {
                    endOfShelfDirection = pSM.closestRail.transform.TransformPoint(pathCreator.bezierPath.GetPoint(0))
                                        - pSM.closestRail.transform.TransformPoint(pathCreator.bezierPath.GetPoint(pathCreator.bezierPath.NumAnchorPoints)); //start - ende
                }
                else if (pSM.currentDistance >= pathLength) //arriving at end of path
                {
                    endOfShelfDirection = pSM.closestRail.transform.TransformPoint(pathCreator.bezierPath.GetPoint(pathCreator.bezierPath.NumAnchorPoints))
                                        - pSM.closestRail.transform.TransformPoint(pathCreator.bezierPath.GetPoint(0)); //ende - start
                }

                Plane shelfPlane = new Plane(endOfShelfDirection.normalized, Vector3.zero);

                if (/* pSM.resultingSpeed( pSM.playerVelocity, pathDirection) >0   )*/shelfPlane.GetSide(Vector3.zero + pSM.playerVelocity)) //player moves in the direction of the end point (move left when going out at start, moves right when going out at end)
                {
                    if (pSM.CheckForNextClosestRail(pSM.closestRail))
                    {
                        pSM.OnResnap();
                    }
                    else
                    {
                        pSM.OnFall();
                    }
                }
            }
            CheckIfReadyToDismount();
        }
        else
        {
            Dismount();
        }


    }

    bool CheckForCollisionCharacter(Vector3 moveDirection)
    {
        RaycastHit hit;
        Vector3 p1 = pSM.transform.position + controller.center + Vector3.up * -controller.height / 1.5f;
        Vector3 p2 = p1 + Vector3.up * controller.height;

        if (Physics.CapsuleCast(p1, p2, controller.radius, moveDirection, out hit, 0.2f, LayerMask.GetMask("SlidingObstacle")))
        {
            return true;
        }
        return false;
    }

    bool CheckForCollisionLadder(Vector3 moveDirection)
    {
        RaycastHit hit;
        Vector3 boxExtents = new Vector3(pSM.ladderMesh.localScale.x * 0.5f, pSM.ladderMesh.localScale.y * 0.5f, pSM.ladderMesh.localScale.z * 0.5f);

        if (Physics.BoxCast(pSM.ladder.position, pSM.ladderMesh.localScale, moveDirection, out hit, Quaternion.identity, 0.1f, LayerMask.GetMask("SlidingObstacle")))
        {
            return true;
        }
        return false;
    }

    void CheckIfReadyToDismount()
    {
        // Dismounting the ladder on top and bottom 
        if (pSM.HeightOnLadder == 0 && pSM.forwardInput > 0)
        {
            dismountTimer += Time.fixedDeltaTime;
            RaycastHit hit;
            Vector3 boxExtents = new Vector3(1.540491f * 0.5f, 0.4483852f * 0.5f, 1.37359f * 0.5f);

            if (dismountTimer >= stats.ladderDismountTimer
            && !Physics.BoxCast(controller.transform.position + Vector3.up * 1.5f + controller.transform.forward * -1, boxExtents,
            controller.transform.forward, out hit, controller.transform.rotation, 4f, LayerMask.GetMask("SlidingObstacle", "Environment")))
            {
                if (hit.collider != controller.gameObject)
                {
                    dismountTimer = 0;
                    dismountStartPos = pSM.transform.position;
                    pSM.dismounting = true;
                }
            }
        }
        else if (pSM.HeightOnLadder == -1 && pSM.forwardInput < 0)
        {
            dismountTimer += Time.fixedDeltaTime;
            if (dismountTimer >= stats.ladderDismountTimer)
            {
                dismountTimer = 0;
                controller.transform.forward = -pathCreator.path.GetNormalAtDistance(currentDistance);
                PlayerStateMachine.OnLadderBottom();
            }
        }
        else if (dismountTimer != 0)
        {
            dismountTimer = 0;
        }
    }

    void Dismount()
    {
        // 1 is how much units the player needs to move up to be on top of the shelf.
        if ((pSM.transform.position - dismountStartPos).magnitude <= 1 && !dismountedHalfways)
        {
            pSM.HeightOnLadder += stats.ladderDismountSpeed * Time.fixedDeltaTime;
            pSM.transform.position = ladder.transform.position + pSM.ladderDirection * ladderLength * pSM.HeightOnLadder;
        }
        else if (!dismountedHalfways)
        {
            dismountStartPos = pSM.transform.position;
            dismountedHalfways = true;
        }

        // Make one step forward on the shelf before changing to walking state.
        if ((pSM.transform.position - dismountStartPos).magnitude <= 0.1f && dismountedHalfways)
        {
            pSM.HeightOnLadder += stats.ladderDismountSpeed * Time.fixedDeltaTime;
            pSM.transform.position = ladder.transform.position + pSM.controller.transform.forward * ladderLength * pSM.HeightOnLadder;
        }
        else if (dismountedHalfways)
        {
            pSM.dismounting = false;
            pSM.OnLadderTop();
        }
    }

    public PlayerSliding(PlayerMovementStateMachine playerStateMachine)
        : base(playerStateMachine)
    {

    }
}
