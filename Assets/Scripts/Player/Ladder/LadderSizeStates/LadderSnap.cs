using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderSnap : State
{
    ValuesScriptableObject stats;
    Vector3 lastPlayerPosition;
    Vector3 currentPlayerPosition;
    bool isFirstFrame = true;
    bool hasExpanded;

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
        bool isFullyUnfolded = LadderSizeStateMachine.ladderLength >= stats.ladderLengthBig;
        if (!isFullyUnfolded && !hasExpanded)
        {
            if ((isSwingingUp && isOverRail) || (!isSwingingUp && !isOverRail))
            {
                PSM.expandAfterSnap = true;
                //LadderSizeStateMachine.ladderLength += Mathf.Clamp(Vector3.Distance(PSM.ladder.transform.position, currentPlayerPosition) + 0.1f, stats.ladderLengthSmall, stats.ladderLengthBig);
                LadderSizeStateMachine.ladderLength = Mathf.Clamp(Vector3.Distance(PSM.ladder.transform.position, currentPlayerPosition), LadderSizeStateMachine.ladderLength, stats.ladderLengthBig);
                Debug.Log("expansion!" + LadderSizeStateMachine.ladderLength);
                LadderSizeStateMachine.ladderParent.transform.localScale = new Vector3(LadderSizeStateMachine.ladderLength, 1, 1);
            }
            else
            {
                LadderSizeStateMachine.ladderLength = Mathf.Clamp(Vector3.Distance(PSM.ladder.transform.position, currentPlayerPosition), LadderSizeStateMachine.ladderLength, stats.ladderLengthBig);
                LadderSizeStateMachine.ladderParent.transform.localScale = new Vector3(LadderSizeStateMachine.ladderLength, 1, 1);
                Debug.Log("jfjkkjkfkdjfdjk");
                PSM.expandAfterSnap = false;
            }
        }
        else
        {
            if (!hasExpanded)
            {
                LadderSizeStateMachine.ladderLength = stats.ladderLengthBig;
                hasExpanded = true;
                LadderSizeStateMachine.ladderParent.transform.localScale = new Vector3(LadderSizeStateMachine.ladderLength, 1, 1);
                PSM.expandAfterSnap = false;
                PSM.playerVelocity = Vector3.zero;
                PSM.baseVelocity = Vector3.zero;
                PSM.bonusVelocity = Vector3.zero;
            }
        }

        lastPlayerPosition = currentPlayerPosition;
    }


    public override IEnumerator Finish()
    {
        LadderSizeStateMachine.isUnFolding = false;
        return base.Finish();
    }
}
