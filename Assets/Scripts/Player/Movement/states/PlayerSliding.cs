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


    //Wie macht man parent Swap (Braucht die Leiter und die Player dings)

    public override IEnumerator Initialize()
    {
        // Zuweisungen
        pSM = PlayerStateMachine;
        ladderSizeState = pSM.ladderSizeStateMachine;
        ladderLength = ladderSizeState.ladderLengthBig;
        speed = pSM.OnLadderAcceleration;
        closestShelf = pSM.closestShelf;
        controller = pSM.controller;
        ladder = pSM.ladder;
        pathCreator = closestShelf.pathCreator;
        pSM.HeightOnLadder = -1;


        //Leiter auf den path setzen
        Vector3 startingPoint = Vector3.zero;
        if (closestShelf != null)
        {
            startingPoint = pathCreator.path.GetClosestPointOnPath(pSM.transform.position);
        }
        else
        {
            Debug.LogError("null1");
        }

        currentDistance = pathCreator.path.GetClosestDistanceAlongPath(startingPoint);
        ladder.transform.position = startingPoint;
        ladder.transform.forward = -pathCreator.path.GetNormalAtDistance(currentDistance);
        path = pSM.closestShelf.pathCreator.path;
        pathLength = path.cumulativeLengthAtEachVertex[path.cumulativeLengthAtEachVertex.Length - 1];

        pSM.currentDistance = path.GetClosestDistanceAlongPath(pSM.transform.position);


        // PC auf Leiter setzen 
        ladder.transform.parent = null;
        Vector3 targetPosition = startingPoint + pSM.ladderDirection * ladderLength;
        targetPosition.y = Mathf.Clamp(targetPosition.y, pSM.ladderDirection.y * ladderLength, controller.transform.position.y);
        controller.transform.position = targetPosition;
        pSM.HeightOnLadder = -(startingPoint - targetPosition).magnitude / ladderLength;

        controller.transform.forward = -pathCreator.path.GetNormalAtDistance(currentDistance);
        controller.transform.parent = ladder.transform;
        pSM.ladderSizeStateMachine.OnGrow();

        pathDirection = pathCreator.path.GetDirectionAtDistance(currentDistance,EndOfPathInstruction.Stop);
        //pSM.LadderVelocity = AngleDirection(pSM.controller.transform.forward, pSM.moveDirection, pSM.controller.transform.up) * pSM.momentum * 10;
        //pSM.maximumSpeedOnLadder;
        pSM.playerVelocity = pSM.resultingVelocity(pSM.playerVelocity,pathDirection);
        pSM.playerVelocity = pSM.playerVelocity.normalized * Mathf.Clamp(pSM.playerVelocity.magnitude, -pSM.maxSlidingSpeed, pSM.maxSlidingSpeed);
        
        // Parent Swap () => Leiter ist Parent

        yield return null;
    }

    public override IEnumerator Finish()
    {
        // Parent Swap () => Player ist Parent
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
        PlayerStateMachine.playerVelocity.y = PlayerStateMachine.jumpheight;
        PlayerStateMachine.OnFall();
        //OnFall.trigger
        //OnLadderShrink.trigger

        //yield break;
    }

    // forward is the direction, the targetDir is left or right of
    //returns -1 when to the left, 1 to the right, and 0 for forward/backward
    public float AngleDirection(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0.0f)
        {
            Debug.Log("right");
            return 1.0f;
        }
        else if (dir < 0.0f)
        {
            Debug.Log("left");
            return -1.0f;
        }
        else
        {
            Debug.Log("in front");
            return 0.0f;
        }
    }



    public override void Movement()
    {
        if (!dismounting)
        {
            //An der Leiter Hoch und runter bewegen
            pSM.HeightOnLadder += pSM.ForwardInput * speed * Time.deltaTime;
            pSM.HeightOnLadder = Mathf.Clamp(pSM.HeightOnLadder, -1, 0);
            pSM.transform.position = ladder.transform.position + pSM.ladderDirection * ladderLength * pSM.HeightOnLadder;

            //Leiter horizontale Bewegung
            pathDirection = path.GetDirectionAtDistance(currentDistance);
            pSM.playerVelocity += pSM.sideWaysInput * pathDirection * Time.deltaTime * pSM.slidingAcceleration;
            pSM.currentDistance += pSM.resultingSpeed(pSM.playerVelocity, pathDirection);

        //Leiter horizontale Bewegung
        pathDirection = path.GetDirectionAtDistance(currentDistance);
        pSM.playerVelocity += pSM.sideWaysInput * pathDirection * Time.deltaTime * pSM.slidingAcceleration;
        pSM.playerVelocity = pSM.playerVelocity.normalized * Mathf.Clamp(pSM.playerVelocity.magnitude, -pSM.maxSlidingSpeed, pSM.maxSlidingSpeed);
        pSM.currentDistance += pSM.resultingSpeed(pSM.playerVelocity, pathDirection);
            pSM.ladder.position = path.GetPointAtDistance(pSM.currentDistance, EndOfPathInstruction.Stop);
            if (pSM.currentDistance <= 0 || pSM.currentDistance >= pathLength)
            {
                pSM.OnFall();
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
        if (pSM.HeightOnLadder == 0 && pSM.ForwardInput != 0)
        {
            dismountTimer += Time.deltaTime;
            if (dismountTimer >= 0.3f)
            {
                dismountTimer = 0;
                dismountStartPos = pSM.transform.position;
                dismounting = true;
            }
        }
        else if (pSM.HeightOnLadder == -1 && pSM.ForwardInput != 0)
        {
            dismountTimer += Time.deltaTime;
            if (dismountTimer >= 0.3f)
            {
                dismountTimer = 0;
                controller.transform.forward = -pathCreator.path.GetNormalAtDistance(currentDistance);
                PlayerStateMachine.OnFall();
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
            pSM.OnLand();
        }
    }

    public PlayerSliding(PlayerMovementStateMachine playerStateMachine)
        : base(playerStateMachine)
    {

    }
}
