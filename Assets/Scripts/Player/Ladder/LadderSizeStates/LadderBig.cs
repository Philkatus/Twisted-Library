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
        if (LadderSizeStateMachine.ladderParent.transform.localScale.x == 0)
        {
            LadderSizeStateMachine.ladderParent.transform.localScale += new Vector3(LadderSizeStateMachine.ladderLengthBig, 0, 0);
        }
        else
        {
            LadderSizeStateMachine.ladderParent.transform.localScale += new Vector3(LadderSizeStateMachine.ladderLengthBig - LadderSizeStateMachine.ladderLengthSmall, 0, 0);
        }
        yield return null;
    }

    public override IEnumerator ChangeLadderSize()
    {
        LadderSizeStateMachine.OnShrink();
        yield return null;
    }
}
