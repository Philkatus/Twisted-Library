using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderPush : State
{
    ValuesScriptableObject stats;
    PlayerMovementStateMachine pSM;
    LadderSizeStateMachine lSM;
    Vector3 target;
    Quaternion LadderLocalRotation;
    Vector3 startingPosition;
    Vector3 startingRotation;
    Vector3 velocity;
    Quaternion startingRotation2;
    Quaternion playerStartingRotation;

    bool isLerpGoing = true;
    float time;
    float startDuration = 0.04f;
    float distance;
    bool rotated;

    public LadderPush(LadderSizeStateMachine ladderSizeStateMachine,Vector3 velocity) : base(ladderSizeStateMachine)
    {
        this.velocity = velocity;
    }

    public override void Initialize()
    {
        lSM = LadderSizeStateMachine;
        pSM = lSM.playerStateMachine;
        target = pSM.ladderJumpTarget;
        stats = pSM.stats;
        LadderLocalRotation = lSM.ladderParent.localRotation;
        startingPosition = lSM.transform.position;
        startingRotation = lSM.transform.up;
        startingRotation2 = lSM.transform.rotation;
        playerStartingRotation = pSM.transform.rotation;
        isLerpGoing = true;
        lSM.anim.SetTrigger("Ladder Push");

    }

    public override IEnumerator Finish()
    {

        lSM.transform.SetParent(null);
        yield return null;
    }

    void RotateLadder()
    {
        Vector3 targetDirection;
        Vector3 TargetLadderPosition;
        distance = Vector3.Distance(target, pSM.transform.position);
        targetDirection = (target - PSM.transform.position).normalized;
        TargetLadderPosition = PSM.ladderPushTransform.position + targetDirection * stats.ladderLengthSmall;

        Plane plane = new Plane(targetDirection, target);
        Vector3 secondPoint = plane.ClosestPointOnPlane(target + new Vector3(1,1,1));
        Vector3 forward = secondPoint - target;

        lSM.transform.up = Vector3.Lerp(startingRotation, -targetDirection,time/startDuration);

        //lSM.transform.rotation = Quaternion.Slerp(startingRotation2, Quaternion.LookRotation(forward, targetDirection),time/startDuration);
        lSM.transform.position = Vector3.Lerp(startingPosition, TargetLadderPosition, time / startDuration);
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        //PSM.transform.rotation = Quaternion.Slerp(playerStartingRotation, targetRotation, time / startDuration);
        if (time >= startDuration) 
        {
            rotated = true;

            pSM.ladder.SetParent(pSM.transform);
            PSM.bonusVelocity = velocity;
        }


    }

    public override void Fold()
    {
        time += Time.deltaTime;
        if (!rotated)
        {
            RotateLadder();
        }
        else if (isLerpGoing)
        {
            distance = Vector3.Distance(target, pSM.transform.position);
            
            if (distance >= stats.ladderLengthBig || pSM.playerVelocity.y <= 0)
            {
                isLerpGoing = false;
            }
        }
        else
        {
            lSM.OnShrink();
        }
        
    }
}
