using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderUnfold : State
{
    ValuesScriptableObject stats;
    bool isLerpGoing=true;
    float time = 0;

    public LadderUnfold(LadderSizeStateMachine ladderSizeStateMachine) : base(ladderSizeStateMachine)
    {

    }

    public override void Initialize()
    {
        stats = PSM.stats;
        isLerpGoing = true;
        LadderSizeStateMachine.isUnFolding = true;
        
        AudioManager.Instance.PlayRandom("LadderUnFold");

        LadderSizeStateMachine.anim.SetTrigger("Ladder Extent");
        
    }

    public override void Fold()
    {
        if (isLerpGoing)
        {
            time = LadderSizeStateMachine.anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
            LadderSizeStateMachine.ladderLength = Mathf.Lerp(stats.ladderLengthSmall, stats.ladderLengthBig,time % 1);
            if (LadderSizeStateMachine.ladderLength>=stats.ladderLengthBig-.2f)
            {
                isLerpGoing = false;
                LadderSizeStateMachine.ladderLength = stats.ladderLengthBig;
            }
        }
        if (time >= stats.foldingTime + stats.extraFoldingTime)
        {
            LadderSizeStateMachine.isUnFolding = false;
        }
    }
    public override IEnumerator Finish()
    {
        LadderSizeStateMachine.isUnFolding = false;
        return base.Finish();
    }
}
