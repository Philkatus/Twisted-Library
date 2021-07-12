using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderSnap : State
{
    ValuesScriptableObject stats;
    bool hasSnapped;

    public LadderSnap(LadderSizeStateMachine ladderSizeStateMachine) : base(ladderSizeStateMachine)
    {

    }

    public override void Initialize()
    {
        stats = PSM.stats;
    }

    public override void Fold()
    {
        
    }

    public override void FollowLadderTarget()
    {
        LadderSizeStateMachine lSM = LadderSizeStateMachine;
        Vector3 followTarget = PSM.closestRail.pathCreator.path.GetPointAtDistance(PSM.currentDistance);

        lSM.transform.position = Vector3.Lerp(lSM.transform.position, lSM.followTarget.position, 3.5f * Time.deltaTime);
        if (Vector3.Distance(lSM.transform.position, followTarget) > 0) 
        {
            lSM.transform.up = Vector3.Lerp(lSM.transform.up, (PSM.transform.forward).normalized, 4.8f * Time.deltaTime);
        }
       

    }


    public override IEnumerator Finish()
    {
        LadderSizeStateMachine.isUnFolding = false;
        return base.Finish();
    }
}
