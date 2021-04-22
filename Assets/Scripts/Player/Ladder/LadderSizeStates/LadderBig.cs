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
        LadderSizeStateMachine.ladderParent.transform.localScale = new Vector3(LadderSizeStateMachine.ladderLengthBig, 1, 1);

    }

    public override IEnumerator ChangeLadderSize()
    {
        LadderSizeStateMachine.OnShrink();
        yield return null;
    }
}
