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
    Vector3 startingLocalPosition;
    Quaternion startingLocalRotation;

    bool isLerpGoing = true;
    float time;
    float distance;

    public LadderPush(LadderSizeStateMachine ladderSizeStateMachine) : base(ladderSizeStateMachine)
    {

    }

    public override void Initialize()
    {
        lSM = LadderSizeStateMachine;
        pSM = lSM.playerStateMachine;
        target = pSM.ladderJumpTarget;
        stats = pSM.stats;
        LadderLocalRotation = lSM.ladderParent.localRotation;
        RotateLadder();
    }

    public override IEnumerator Finish()
    {
        if (PSM.playerState != PlayerMovementStateMachine.PlayerState.swinging)
        {
            pSM.ladder.SetParent(pSM.animController.spine);
        }
        lSM.ladderParent.localRotation = LadderLocalRotation;

        yield return null;
    }

    void RotateLadder()
    {
        pSM.ladder.SetParent(pSM.transform);
        lSM.ladderParent.transform.right = pSM.transform.position - target;
        distance = Vector3.Distance(target, pSM.transform.position);
        lSM.ladderParent.transform.localScale = new Vector3(distance, 1, 1);
        lSM.ladderLength = stats.ladderLengthBig;
    }

    public override void Fold()
    {
        if (isLerpGoing)
        {
            distance = Vector3.Distance(target, pSM.transform.position);
            lSM.ladderLength = Mathf.Clamp(distance, stats.ladderLengthSmall, stats.ladderLengthBig);
            lSM.ladderParent.transform.right = pSM.transform.position - target;
            lSM.ladderParent.transform.localScale = new Vector3(lSM.ladderLength, 1, 1);

            if (distance >= stats.ladderLengthBig || pSM.playerVelocity.y <= 0)
            {
                isLerpGoing = false;

                startingLocalPosition = pSM.ladder.localPosition;
                startingLocalRotation = pSM.ladder.localRotation;
            }
        }
        else
        {
            time += Time.deltaTime;

            pSM.ladder.localPosition = Vector3.Lerp(startingLocalPosition, pSM.ladderWalkingPosition, time / stats.foldingTime);
            pSM.ladder.localRotation = Quaternion.Lerp(startingLocalRotation, pSM.ladderWalkingRotation, time / stats.foldingTime);

            pSM.ladder.localPosition = pSM.ladderWalkingPosition;
            pSM.ladder.localRotation = pSM.ladderWalkingRotation;
            lSM.OnShrink();
        }
    }
}
