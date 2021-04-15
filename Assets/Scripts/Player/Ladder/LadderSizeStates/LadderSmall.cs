using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderSmall : State
{
    public LadderSmall(LadderSizeStateMachine ladderSizeStateMachine) : base(ladderSizeStateMachine)
    {

    }

    public override IEnumerator Initialize()
    {
        LadderSizeStateMachine.ladderParent.transform.localScale -= new Vector3(0.6f, 0, 0);

        yield return null;
    }

    public override IEnumerator ChangeLadderSize()
    {
        LadderSizeStateMachine.OnGrow();
        yield return null;
    }
}
