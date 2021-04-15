using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderBig : State
{
    public LadderBig(LadderSizeStateMachine ladderSizeStateMachine) : base(ladderSizeStateMachine)
    {

    }

    public override IEnumerator Initialize()
    {
        yield return null;
    }

    public override IEnumerator Finish()
    {
        LadderSizeStateMachine.ladderParent.transform.localScale -= new Vector3(0.6f, 0, 0);
        LadderSizeStateMachine.OnShrink();
        yield return null;
    }
}
