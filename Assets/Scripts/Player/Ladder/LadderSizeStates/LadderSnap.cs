using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderSnap : State
{
    ValuesScriptableObject stats;
    Vector3 lastPlayerPosition;
    Vector3 currentPlayerPosition;
    bool isFirstFrame = true;

    public LadderSnap(LadderSizeStateMachine ladderSizeStateMachine) : base(ladderSizeStateMachine)
    {

    }

    public override void Initialize()
    {
        stats = PSM.stats;
    }

    public override void Fold()
    {
        currentPlayerPosition = PSM.transform.position;
        if (isFirstFrame) 
        {
            lastPlayerPosition = currentPlayerPosition;
            isFirstFrame = false;
            return;
        }

        Vector3 pivot = PSM.closestRail.pathCreator.path.GetPointAtDistance(PSM.currentDistance);
        bool isSwingingUp = currentPlayerPosition.y > lastPlayerPosition.y;
        bool isOverRail = currentPlayerPosition.y > pivot.y;
        bool isFullyUnfolded = LadderSizeStateMachine.ladderLength > stats.ladderLengthBig;
        if (!isFullyUnfolded)
        {
            if ((isSwingingUp && isOverRail) || (!isSwingingUp && !isOverRail))
            {
                LadderSizeStateMachine.ladderLength += Vector3.Distance(lastPlayerPosition, currentPlayerPosition)*.3f;
                LadderSizeStateMachine.ladderParent.transform.localScale = new Vector3(LadderSizeStateMachine.ladderLength, 1, 1);
            }
            else 
            {
                LadderSizeStateMachine.ladderLength += Vector3.Distance(lastPlayerPosition, currentPlayerPosition) * .2f;
                LadderSizeStateMachine.ladderParent.transform.localScale = new Vector3(LadderSizeStateMachine.ladderLength, 1, 1);
            }
        }
        else 
        {
            LadderSizeStateMachine.ladderLength = stats.ladderLengthBig;
        }

        lastPlayerPosition = currentPlayerPosition;
        
    }


    public override IEnumerator Finish()
    {
        LadderSizeStateMachine.isUnFolding = false;
        return base.Finish();
    }
}
