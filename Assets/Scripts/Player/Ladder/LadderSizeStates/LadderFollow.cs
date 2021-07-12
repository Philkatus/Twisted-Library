using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderFollow : State
{
    ValuesScriptableObject stats;
    bool isLerpGoing;
    float time = 0;
    float noisetimer;

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
        Vector3 followTarget = lSM.followTarget.position;
        noisetimer += Time.deltaTime;
        float yOffset = Mathf.PerlinNoise(1, noisetimer);
        yOffset = ExtensionMethods.Remap(yOffset, -.1f, 1, 0f, .2f);
        float xOffset = Mathf.PerlinNoise(200, noisetimer);
        yOffset = ExtensionMethods.Remap(xOffset, 0, 1, -.3f, .3f);
        float zOffset = Mathf.PerlinNoise(3000, noisetimer);
        yOffset = ExtensionMethods.Remap(zOffset, 0, 1, -.3f, .3f);


        followTarget = new Vector3(lSM.followTarget.position.x +xOffset ,lSM.followTarget.position.y+yOffset , lSM.followTarget.position.z+zOffset);
        lSM.transform.position = Vector3.Lerp(lSM.transform.position,lSM.followTarget.position, 3.5f*Time.deltaTime);
        Quaternion quaternion = Quaternion.LookRotation(lSM.transform.position - PSM.transform.position, lSM.transform.position - followTarget);
        //lSM.transform.rotation = Quaternion.Slerp(lSM.transform.rotation, quaternion, Time.deltaTime);
        lSM.transform.up = Vector3.Lerp(lSM.transform.up, (-lSM.transform.position+ followTarget).normalized,4.8f*Time.deltaTime);

    }

    public override IEnumerator Finish()
    {
        //also set false when changing state because that happens before the timer above is over
        LadderSizeStateMachine.isFoldingUp = false;

        yield break;
    }
}
