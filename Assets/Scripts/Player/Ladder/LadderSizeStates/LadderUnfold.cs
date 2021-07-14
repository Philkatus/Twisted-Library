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
        time = LadderSizeStateMachine.anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        LadderSizeStateMachine.ladderLength = Mathf.Lerp(stats.ladderLengthSmall, stats.ladderLengthBig, time % 1);

    }

    public override void Fold()
    {
        if (isLerpGoing)
        {
            LadderSizeStateMachine.ladderLength = Vector3.Distance(PSM.ladderBottom.position, PSM.LadderTop.position);
            if (LadderSizeStateMachine.ladderLength>=stats.ladderLengthBig-.2f)
            {
                isLerpGoing = false;
                LadderSizeStateMachine.ladderLength = stats.ladderLengthBig;
            }
        }
        time += Time.deltaTime;
        if (time >= stats.extraFoldingTime)
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
