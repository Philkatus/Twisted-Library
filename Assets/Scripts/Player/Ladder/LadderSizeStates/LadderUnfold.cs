using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderUnfold : State
{
    #region INHERITED
    PlayerMovementStateMachine pSM;
    #endregion

    bool isLerpGoing;
    float time = 0;

    public LadderUnfold(LadderSizeStateMachine ladderSizeStateMachine) : base(ladderSizeStateMachine)
    {

    }

    public override void Initialize()
    {
        pSM = PlayerStateMachine;
        time = ExtensionMethods.Remap(LadderSizeStateMachine.ladderLength,LadderSizeStateMachine.ladderLengthSmall,LadderSizeStateMachine.ladderLengthBig,0, LadderSizeStateMachine.foldSpeed);
        isLerpGoing = true;
        if (time==0)
        {
            LadderSizeStateMachine.isUnFolding = true;
        }
    }

    public override void Fold()
    {
        if (isLerpGoing)
        {
            time += Time.deltaTime;
            LadderSizeStateMachine.ladderLength = Mathf.Lerp(LadderSizeStateMachine.ladderLengthSmall, LadderSizeStateMachine.ladderLengthBig, time / LadderSizeStateMachine.foldSpeed);
            LadderSizeStateMachine.ladderParent.transform.localScale = new Vector3(LadderSizeStateMachine.ladderLength, 1, 1);

            if (time >= LadderSizeStateMachine.foldSpeed)
            {
                isLerpGoing = false;
                LadderSizeStateMachine.ladderLength = LadderSizeStateMachine.ladderLengthBig;
                LadderSizeStateMachine.ladderParent.transform.localScale = new Vector3(LadderSizeStateMachine.ladderLength, 1, 1);
            }
        }
        if (time >= LadderSizeStateMachine.foldSpeed + LadderSizeStateMachine.extraFoldJumpTimer)
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
