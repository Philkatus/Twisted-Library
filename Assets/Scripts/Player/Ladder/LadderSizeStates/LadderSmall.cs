using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderSmall : State
{
    public LadderSmall(LadderSizeStateMachine ladderSizeStateMachine) : base(ladderSizeStateMachine)
    {

    }

    public override void Initialize()
    {
        LadderSizeStateMachine.ladderParent.transform.localScale = new Vector3(PSM.stats.ladderLengthSmall, 1, 1);
        LadderSizeStateMachine.ladderLength = PSM.stats.ladderLengthSmall;
    }

    public override IEnumerator ChangeLadderSize()
    {
        LadderSizeStateMachine.OnGrow();
        yield return null;
    }
}
