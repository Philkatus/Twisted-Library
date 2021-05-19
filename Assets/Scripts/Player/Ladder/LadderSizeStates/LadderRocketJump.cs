using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderRocketJump : State
{
    public LadderRocketJump(LadderSizeStateMachine ladderSizeStateMachine) : base(ladderSizeStateMachine)
    {

    }
    PlayerMovementStateMachine pSM;
    LadderSizeStateMachine lSM;

    bool isLerpGoing = true;
    float time;
    float distance;
    Vector3 target;
    Quaternion LadderLocalRotation;



    public override void Initialize()
    {
        lSM = LadderSizeStateMachine;
        pSM = lSM.playerStateMachine;
        target = pSM.ladderJumpTarget;
       
        LadderLocalRotation = lSM.ladderParent.localRotation;
        RotateLadder();

    }

    public override IEnumerator Finish()
    {
        pSM.ladder.SetParent(pSM.animController.spine);
        lSM.ladderParent.localRotation = LadderLocalRotation;
        Debug.Log("end");
        yield return null;
    }

    void RotateLadder()
    {
        pSM.ladder.SetParent(pSM.transform);
        lSM.ladderParent.transform.right = pSM.transform.position - target;
        distance = Vector3.Distance(target, pSM.transform.position);
        lSM.ladderParent.transform.localScale = new Vector3(distance, 1, 1);
        lSM.ladderLength = lSM.ladderLengthBig;
        //Debug.Log("hey");

    }
    public override void Fold()
    {

        if (isLerpGoing)
        {
            distance = Vector3.Distance(target, pSM.transform.position);
            lSM.ladderLength = Mathf.Clamp(distance, lSM.ladderLengthSmall, lSM.ladderLengthBig);
            lSM.ladderParent.transform.right = pSM.transform.position - target;
            lSM.ladderParent.transform.localScale = new Vector3(lSM.ladderLength, 1, 1);

            if (distance >= lSM.ladderLengthBig || pSM.playerVelocity.y <= 0)
            {
                isLerpGoing = false;
                pSM.ladder.transform.SetParent(pSM.transform);
                pSM.ladder.localPosition = pSM.ladderWalkingPosition;
                pSM.ladder.localRotation = pSM.ladderWalkingRotation;
                lSM.OnShrink();
            }
        }
    }
}
