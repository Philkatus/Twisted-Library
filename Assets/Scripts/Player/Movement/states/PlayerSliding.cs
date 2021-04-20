using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

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
    PlayerMovementStateMachine pSM;
    Transform ladder;
    LadderSizeStateMachine ladderSizeState;
    #endregion

    #region PRIVATE
    float dismountTimer;
    bool dismounting;
    bool dismountedHalfways;
    Vector3 dismountStartPos;
    Vector3 pathDirection;
    #endregion

    public override IEnumerator Initialize()
    {
        // Assign variables.
        pSM = PlayerStateMachine;
        ladderSizeState = pSM.ladderSizeStateMachine;
        ladderLength = ladderSizeState.ladderLengthBig;
        speed = pSM.OnLadderAcceleration;
        closestShelf = pSM.closestShelf;
        controller = pSM.controller;
        ladder = pSM.ladder;
        pathCreator = closestShelf.pathCreator;
        pSM.HeightOnLadder = -1;

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

        currentDistance = pathCreator.path.GetClosestDistanceAlongPath(startingPoint);
        ladder.transform.position = startingPoint;
        ladder.transform.forward = -pathCreator.path.GetNormalAtDistance(currentDistance);
        path = pSM.closestShelf.pathCreator.path;
        pathLength = path.cumulativeLengthAtEachVertex[path.cumulativeLengthAtEachVertex.Length - 1];
        pSM.currentDistance = path.GetClosestDistanceAlongPath(pSM.transform.position);

        // Place the character on ladder.
        ladder.transform.parent = null;
        Vector3 targetPosition = startingPoint + pSM.ladderDirection * ladderLength;
        targetPosition.y = Mathf.Clamp(targetPosition.y, pSM.ladderDirection.y * ladderLength, controller.transform.position.y);
        controller.transform.position = targetPosition;
        pSM.HeightOnLadder = -(startingPoint - targetPosition).magnitude / ladderLength;

        controller.transform.forward = -pathCreator.path.GetNormalAtDistance(currentDistance);
        controller.transform.parent = ladder.transform;
        pSM.ladderSizeStateMachine.OnGrow();

        pathDirection = pathCreator.path.GetDirectionAtDistance(currentDistance, EndOfPathInstruction.Stop);
        pSM.playerVelocity = pSM.resultingVelocity(pSM.playerVelocity, pathDirection);
        pSM.playerVelocity = pSM.playerVelocity.normalized * Mathf.Clamp(pSM.playerVelocity.magnitude, -pSM.maxSlidingSpeed, pSM.maxSlidingSpeed);

        yield return null;
    }

    public override IEnumerator Finish()
    {
        controller.transform.parent = null;
        ladder.transform.localPosition = new Vector3(4, 0, 0);
        ladder.transform.parent = controller.transform;
        pSM.ladderSizeStateMachine.OnShrink();
        yield break;
    }

    public override void Jump()
    {
        //Ein Sprung 
        //eine speed mitgeben????
        PlayerStateMachine.playerVelocity.y += PlayerStateMachine.jumpheight;
        PlayerStateMachine.OnFall();
    }

    public override void Movement()
    {
        if (!dismounting)
        {
            // Go up and down.
            pSM.HeightOnLadder += pSM.forwardInput * speed * Time.deltaTime;
            pSM.HeightOnLadder = Mathf.Clamp(pSM.HeightOnLadder, -1, 0);
            pSM.transform.position = ladder.transform.position + pSM.ladderDirection * ladderLength * pSM.HeightOnLadder;

            // Move horizontally.
            pathDirection = path.GetDirectionAtDistance(currentDistance);
            
            //playervelocity increased with input
            pSM.playerVelocity += pSM.sideWaysInput * pathDirection * Time.deltaTime * pSM.slidingAcceleration;
            //drag calculation
            
            float resultingSpeed = pSM.resultingSpeed(pSM.playerVelocity, pathDirection);
            //speed Clamp
            pSM.playerVelocity = pSM.playerVelocity.normalized * Mathf.Clamp(pSM.playerVelocity.magnitude *(100 - pSM.slidingDragPercentage)/100, -pSM.maxSlidingSpeed, pSM.maxSlidingSpeed);

            //moving of the object
            pSM.currentDistance += pSM.resultingSpeed(pSM.playerVelocity, pathDirection);
            pSM.ladder.position = path.GetPointAtDistance(pSM.currentDistance, EndOfPathInstruction.Stop);

            if (pSM.currentDistance <= 0 || pSM.currentDistance >= pathLength)
            {
                Vector3 endOfShelfDirection = new Vector3();
                if (pSM.currentDistance <= 0) //arriving at start of path
                {
                    endOfShelfDirection = pSM.closestShelf.transform.TransformPoint(pathCreator.bezierPath.GetPoint(0))
                                        - pSM.closestShelf.transform.TransformPoint(pathCreator.bezierPath.GetPoint(pathCreator.bezierPath.NumAnchorPoints)); //start - ende
                }
                else if (pSM.currentDistance >= pathLength) //arriving at end of path
                {
                    endOfShelfDirection = pSM.closestShelf.transform.TransformPoint(pathCreator.bezierPath.GetPoint(pathCreator.bezierPath.NumAnchorPoints))
                                        - pSM.closestShelf.transform.TransformPoint(pathCreator.bezierPath.GetPoint(0)); //ende - start
                }

                Plane shelfPlane = new Plane(endOfShelfDirection.normalized, Vector3.zero);

                if (shelfPlane.GetSide(Vector3.zero + pSM.playerVelocity)) //player moves in the direction of the end point (move left when going out at start, moves right when going out at end)
                {
                    if (pSM.CheckForNextClosestShelf(pSM.closestShelf))
                    {
                        pSM.OnSnap();
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

    void CheckIfReadyToDismount()
    {
        // Dismounting the ladder on top and bottom 
        if (pSM.HeightOnLadder == 0 && pSM.forwardInput != 0)
        {
            dismountTimer += Time.deltaTime;
            if (dismountTimer >= 0.3f)
            {
                dismountTimer = 0;
                dismountStartPos = pSM.transform.position;
                dismounting = true;
            }
        }
        else if (pSM.HeightOnLadder == -1 && pSM.forwardInput != 0)
        {
            dismountTimer += Time.deltaTime;
            if (dismountTimer >= 0.3f)
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
            pSM.HeightOnLadder += pSM.ladderDismountSpeed * Time.deltaTime;
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
            pSM.HeightOnLadder += pSM.ladderDismountSpeed * Time.deltaTime;
            pSM.transform.position = ladder.transform.position + pSM.controller.transform.forward * ladderLength * pSM.HeightOnLadder;
        }
        else if (dismountedHalfways)
        {
            pSM.OnLadderTop();
        }
    }

    public PlayerSliding(PlayerMovementStateMachine playerStateMachine)
        : base(playerStateMachine)
    {

    }
}
