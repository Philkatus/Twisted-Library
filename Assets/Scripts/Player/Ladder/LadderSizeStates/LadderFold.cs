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
        pSM = PlayerStateMachine;
        time = 0;
        isLerpGoing = true;
    }

    public override void Fold()
    {
       // if(pSM.isPerformedFold && isLerpGoing == false)
       // {
        ////    isLerpGoing = true;
        //    time = 0;
       // }

        if(isLerpGoing)
        {
            time += Time.deltaTime;
            LadderSizeStateMachine.ladderLength = Mathf.Lerp(LadderSizeStateMachine.ladderLengthBig, LadderSizeStateMachine.ladderLengthSmall, time / LadderSizeStateMachine.foldSpeed);
            LadderSizeStateMachine.ladderParent.transform.localScale = new Vector3(LadderSizeStateMachine.ladderLength, 1, 1);

            if (time >= LadderSizeStateMachine.foldSpeed)
            {
                isLerpGoing = false;
            }
        }
    }

}
