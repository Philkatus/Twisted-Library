using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderFold : State
{
    ValuesScriptableObject stats;
    bool isLerpGoing;
    float time = 0;

    public LadderFold(LadderSizeStateMachine ladderSizeStateMachine) : base(ladderSizeStateMachine)
    {

    }

    public override void Initialize()
    {
        stats = PSM.stats;
        time = ExtensionMethods.Remap(LadderSizeStateMachine.ladderLength, stats.ladderLengthSmall, stats.ladderLengthBig, stats.foldingTime, 0);
        isLerpGoing = true;
        PSM.effects.OnStateChangedLadderPush();
        LadderSizeStateMachine.isFoldingUp = true;
        AudioManager.Instance.PlayRandom("LadderFold",LadderSizeStateMachine.transform.position+PSM.ladderDirection*stats.ladderLengthBig);

        LadderSizeStateMachine.anim.SetTrigger("LadderRetract");
    }

    public override void Fold()
    {
        /*
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
        */
    }

    public override IEnumerator Finish()
    {
        //also set false when changing state because that happens before the timer above is over
        LadderSizeStateMachine.isFoldingUp = false;

        yield break;
    }


}
