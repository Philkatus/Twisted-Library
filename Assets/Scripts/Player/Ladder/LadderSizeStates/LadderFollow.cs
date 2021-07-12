using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderFollow : State
{
    ValuesScriptableObject stats;
    bool isLerpGoing;
    float time = 0;

    public LadderFollow(LadderSizeStateMachine ladderSizeStateMachine) : base(ladderSizeStateMachine)
    {

    }

    public override void Initialize()
    {
        stats = PSM.stats;
        time = ExtensionMethods.Remap(LadderSizeStateMachine.ladderLength, stats.ladderLengthSmall, stats.ladderLengthBig, stats.foldingTime, 0);
        isLerpGoing = true;
        PSM.effects.OnStateChangedLadderPush();
        LadderSizeStateMachine.isFoldingUp = true;
        AudioManager.Instance.PlayRandom("LadderFold", LadderSizeStateMachine.transform.position + PSM.ladderDirection * stats.ladderLengthBig);
    }

    public override void Fold()
    {
        time += Time.deltaTime;

        if (isLerpGoing)
        {
            LadderSizeStateMachine.ladderLength = Mathf.Lerp(stats.ladderLengthBig, stats.ladderLengthSmall, time / stats.foldingTime);
            LadderSizeStateMachine.ladderParent.transform.localScale = new Vector3(LadderSizeStateMachine.ladderLength, 1, 1);

            if (time >= stats.foldingTime)
            {
                isLerpGoing = false;
            }
        }

        if (time >= stats.foldingTime + stats.extraFoldingTime)
        {
            LadderSizeStateMachine.isFoldingUp = false;
        }
    }

    public override void FollowLadderTarget()
    {
        LadderSizeStateMachine lSM = LadderSizeStateMachine;
        Transform followTarget = lSM.followTarget;

        lSM.transform.position = Vector3.Lerp(lSM.transform.position,followTarget.position, 3*Time.deltaTime);
        Quaternion quaternion = Quaternion.LookRotation(lSM.transform.position - PSM.transform.position, lSM.transform.position - followTarget.transform.position);
        //lSM.transform.rotation = Quaternion.Slerp(lSM.transform.rotation, quaternion, Time.deltaTime);
        lSM.transform.up = Vector3.Lerp(lSM.transform.up, lSM.transform.position - followTarget.position,3*Time.deltaTime);

    }

    public override IEnumerator Finish()
    {
        //also set false when changing state because that happens before the timer above is over
        LadderSizeStateMachine.isFoldingUp = false;

        yield break;
    }
}
