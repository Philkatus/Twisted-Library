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
    ValuesScriptableObject values;
    bool firstMovement = true;
    #endregion

    public override void Initialize()
    {
       
        // Assign variables.
        pSM = PlayerStateMachine;
        values = pSM.valuesAsset;
        
        ladderSizeState = pSM.ladderSizeStateMachine;
        ladderLength = ladderSizeState.ladderLength;
        speed = values.climbingSpeedOnLadder;
        closestShelf = pSM.closestShelf;
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
        pSM.playerVelocity = pSM.resultingVelocity(pSM.playerVelocity, pathDirection);
        pSM.playerVelocity = pSM.playerVelocity.normalized * Mathf.Clamp(pSM.playerVelocity.magnitude, -values.maxSlidingSpeed, values.maxSlidingSpeed);
        
    }

    public override IEnumerator Finish()
    {
        Debug.Log("on Finish " + pSM.transform.position.y);

        yield break;
    }

    public override void Jump()
    {
        PlayerStateMachine.playerVelocity.y += values.jumpHeight;
        PlayerStateMachine.OnFall();
    }

    public override void Movement()
    {
        if (!dismounting)
        {
           

            // Go up and down.
                pSM.HeightOnLadder += pSM.forwardInput * speed * Time.deltaTime;
                pSM.HeightOnLadder = Mathf.Clamp(pSM.HeightOnLadder, -1, 0);
                pSM.transform.position = ladder.transform.position + pSM.ladderDirection * ladderSizeState.ladderLength * pSM.HeightOnLadder; //pos on ladder

            if (firstMovement)
            {
                Debug.Log("duringFirstMovement" + pSM.transform.position.y);
            }
            // Move horizontally.
            pathDirection = path.GetDirectionAtDistance(currentDistance);

            // Get sideways input, no input if both buttons held down.
            float input = 0;
            if (pSM.slideLeftAction.phase == InputActionPhase.Started && pSM.slideAction.phase == InputActionPhase.Started)
            {
                pSM.playerVelocity = Vector3.zero;
                input = 0;
            }
            else
            {
                input = pSM.slideLeftAction.ReadValue<float>();
                input = input * -1;
                if (input == 0)
                {
                    input = pSM.slideAction.ReadValue<float>();
                }
            }

            //playervelocity increased with input
            pSM.playerVelocity += input * pathDirection * Time.deltaTime * values.slidingAcceleration;

            //drag calculation
            float resultingSpeed = pSM.resultingSpeed(pSM.playerVelocity, pathDirection);

            //speed Clamp
            pSM.playerVelocity = pSM.playerVelocity.normalized * Mathf.Clamp(pSM.playerVelocity.magnitude * (100 - values.slidingDragPercentage) / 100, -values.maxSlidingSpeed, values.maxSlidingSpeed);

            //moving the object
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
        
        if(pSM.isPerformedFold)
        {
            Debug.Log("trying to fold");
            ladderSizeState.OnFold();
        }
    }

    void CheckIfReadyToDismount()
    {
        // Dismounting the ladder on top and bottom 
        if (pSM.HeightOnLadder == 0 && pSM.forwardInput < 0)
        {
            dismountTimer += Time.deltaTime;
            if (dismountTimer >= values.ladderDismountTimer)
            {
                dismountTimer = 0;
                dismountStartPos = pSM.transform.position;
                dismounting = true;
            }
        }
        else if (pSM.HeightOnLadder == -1 && pSM.forwardInput > 0)
        {
            dismountTimer += Time.deltaTime;
            if (dismountTimer >= values.ladderDismountTimer)
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
            pSM.HeightOnLadder += values.ladderDismountSpeed * Time.deltaTime;
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
            pSM.HeightOnLadder += values.ladderDismountSpeed * Time.deltaTime;
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
