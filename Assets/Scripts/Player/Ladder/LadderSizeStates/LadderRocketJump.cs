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

        lSM.ladderParent.localRotation = LadderLocalRotation;
        Debug.Log("end");
        yield return null;
    }

    void RotateLadder()
    {
        lSM.ladderParent.transform.right = pSM.transform.position - target;
        distance = Vector3.Distance(target, pSM.transform.position);
        lSM.ladderParent.transform.localScale = new Vector3(distance, 1, 1);
        lSM.ladderLength = lSM.ladderLengthBig;
        Debug.Log("hey");

    }
    public override void Fold()
    {

        if (isLerpGoing)
        {
            distance = Vector3.Distance(target, pSM.transform.position);
            lSM.ladderLength = Mathf.Clamp(distance, lSM.ladderLengthSmall, lSM.ladderLengthBig);

            lSM.ladderParent.transform.localScale = new Vector3(lSM.ladderLength, 1, 1);

            if (distance >= lSM.ladderLengthBig)
            {
                isLerpGoing = false;
                pSM.ladder.transform.parent = pSM.transform;
                pSM.ladder.localPosition = pSM.ladderWalkingPosition;
                pSM.ladder.localRotation = pSM.ladderWalkingRotation;
                lSM.OnShrink();
            }
        }
    }
}
