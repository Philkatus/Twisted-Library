using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderBig : State
{
    public LadderBig(LadderSizeStateMachine ladderSizeStateMachine) : base(ladderSizeStateMachine)
    {

    }

    public override void Initialize()
    {
        LadderSizeStateMachine.ladderParent.transform.localScale = new Vector3(PSM.stats.ladderLengthBig, 1, 1);
        LadderSizeStateMachine.ladderLength = PSM.stats.ladderLengthBig;
    }

    public override IEnumerator ChangeLadderSize()
    {
        LadderSizeStateMachine.OnShrink();
        yield return null;
    }
}
