using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderFold : State
{
    ValuesScriptableObject stats;
    bool isLerpGoing;
    float time = 0;

    public LadderFold(LadderSizeStateMachine ladderSizeStateMachine) : base(ladderSizeStateMachine)
    {

    }

    public override void Initialize()
    {
        stats = PSM.stats;
        isLerpGoing = true;
        PSM.effects.OnStateChangedLadderPush();
        LadderSizeStateMachine.isFoldingUp = true;
        AudioManager.Instance.PlayRandom("LadderFold");

        LadderSizeStateMachine.anim.SetTrigger("Ladder Retract");
    }

    public override void Fold()
    {
        if (isLerpGoing)
        {
            LadderSizeStateMachine.ladderLength = Vector3.Distance(PSM.ladderBottom.position, PSM.LadderTop.position);
            if (LadderSizeStateMachine.ladderLength <= stats.ladderLengthSmall + .2f) 
            {
                isLerpGoing = false;
                LadderSizeStateMachine.ladderLength = stats.ladderLengthSmall;
            }
        }
        time += Time.deltaTime;
        if (time >= stats.extraFoldingTime)
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
