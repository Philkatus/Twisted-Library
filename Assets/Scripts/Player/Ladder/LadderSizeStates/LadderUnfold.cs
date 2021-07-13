using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderUnfold : State
{
    ValuesScriptableObject stats;
    bool isLerpGoing;
    float time = 0;

    public LadderUnfold(LadderSizeStateMachine ladderSizeStateMachine) : base(ladderSizeStateMachine)
    {

    }

    public override void Initialize()
    {
        stats = PSM.stats;
        time = ExtensionMethods.Remap(LadderSizeStateMachine.ladderLength, stats.ladderLengthSmall, stats.ladderLengthBig, 0, stats.foldingTime);
        isLerpGoing = true;
        if (time == 0)
        {
            LadderSizeStateMachine.isUnFolding = true;
        }
        AudioManager.Instance.PlayRandom("LadderUnFold");

        LadderSizeStateMachine.anim.SetTrigger("LadderExtent");
    }

    public override void Fold()
    {
        /*
        if (isLerpGoing)
        {
            time += Time.deltaTime;
            LadderSizeStateMachine.ladderLength = Mathf.Lerp(stats.ladderLengthSmall, stats.ladderLengthBig, time / stats.onSnapUnFoldingTime);
            LadderSizeStateMachine.ladderParent.transform.localScale = new Vector3(LadderSizeStateMachine.ladderLength, 1, 1);

            if (time >= stats.onSnapUnFoldingTime)
            {
                isLerpGoing = false;
                LadderSizeStateMachine.ladderLength = stats.ladderLengthBig;
                LadderSizeStateMachine.ladderParent.transform.localScale = new Vector3(LadderSizeStateMachine.ladderLength, 1, 1);
            }
        }
        if (time >= stats.foldingTime + stats.extraFoldingTime)
        {
            LadderSizeStateMachine.isUnFolding = false;
        }
        */


    }


    public override IEnumerator Finish()
    {
        LadderSizeStateMachine.isUnFolding = false;
        return base.Finish();
    }
}
