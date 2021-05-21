using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderFold : State
{
    #region INHERITED
    PlayerMovementStateMachine pSM;
    #endregion

    bool isLerpGoing;
    float time = 0;

    public LadderFold(LadderSizeStateMachine ladderSizeStateMachine) : base(ladderSizeStateMachine)
    {

    }

    public override void Initialize()
    {
        pSM = LadderSizeStateMachine.playerStateMachine;
        time = ExtensionMethods.Remap(LadderSizeStateMachine.ladderLength, LadderSizeStateMachine.ladderLengthSmall, LadderSizeStateMachine.ladderLengthBig, LadderSizeStateMachine.foldSpeed,0);
        isLerpGoing = true;
        LadderSizeStateMachine.isFoldingUp = true;
    }

    public override void Fold()
    {
        time += Time.deltaTime;

        if(isLerpGoing)
        {
            LadderSizeStateMachine.ladderLength = Mathf.Lerp(LadderSizeStateMachine.ladderLengthBig, LadderSizeStateMachine.ladderLengthSmall, time / LadderSizeStateMachine.foldSpeed);
            LadderSizeStateMachine.ladderParent.transform.localScale = new Vector3(LadderSizeStateMachine.ladderLength, 1, 1);

            if (time >= LadderSizeStateMachine.foldSpeed)
            {
                isLerpGoing = false; 
            }
        }

        if(time >= LadderSizeStateMachine.foldSpeed + LadderSizeStateMachine.extraFoldJumpTimer)
        {
            LadderSizeStateMachine.isFoldingUp = false;
        }
    }

    public override IEnumerator Finish()
    {
        //also set false when changing state because that happens before the timer above is over
        LadderSizeStateMachine.isFoldingUp = false;

        yield break;
    }


}
