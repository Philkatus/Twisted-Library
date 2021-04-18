using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class LadderSliding : State
{
    PlayerMovementStateMachine pSM;
    VertexPath path;
    float pathLength;


    public LadderSliding(LadderStateMachine ladderStateMachine) : base(ladderStateMachine)
    {

    }

    public override IEnumerator Initialize()
    {
        pSM = LadderStateMachine.playerStateMachine;
        path = pSM.closestShelf.pathCreator.path;
        pathLength = path.cumulativeLengthAtEachVertex[path.cumulativeLengthAtEachVertex.Length - 1];
        //pSM.LadderVelocity = pSM.controller.velocity.magnitude;
        pSM.currentDistance = path.GetClosestDistanceAlongPath(pSM.transform.position);

        yield return null;
    }

    public override void Movement()
    {
        pSM.LadderVelocity += pSM.SideWaysInput * pSM.slidingSpeed;
        if (pSM.LadderVelocity > 0)
        {
            pSM.LadderVelocity -= pSM.ladderDrag * Time.deltaTime;
        }
        if (pSM.LadderVelocity < 0)
        {
            pSM.LadderVelocity += pSM.ladderDrag * Time.deltaTime;
        }
        pSM.LadderVelocity = Mathf.Clamp(pSM.LadderVelocity, -pSM.maximumSpeedOnLadder, pSM.maximumSpeedOnLadder);
        pSM.currentDistance += pSM.LadderVelocity * Time.deltaTime;
        pSM.currentDistance = Mathf.Clamp(pSM.currentDistance, 0, pathLength);
        LadderStateMachine.transform.position = path.GetPointAtDistance(pSM.currentDistance, EndOfPathInstruction.Stop);
        if (pSM.currentDistance == 0 || pSM.currentDistance == pathLength)
        {
            pSM.OnFall();
        }


    }

}
