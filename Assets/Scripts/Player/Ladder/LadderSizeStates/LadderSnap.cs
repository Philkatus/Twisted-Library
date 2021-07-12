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

        
        //start the animation 

    }

    public override void Fold()
    {
       
    }

    public override void FollowLadderTarget()
    {

        //move ladder to the rail
        LadderSizeStateMachine lSM = LadderSizeStateMachine;
        Vector3 followTarget = PSM.closestRail.pathCreator.path.GetPointAtDistance(PSM.currentDistance);

        lSM.transform.position = Vector3.Lerp(lSM.transform.position, lSM.followTarget.position, 3.5f * Time.deltaTime);
        if (Vector3.Distance(lSM.transform.position, followTarget) > 0) 
        {
            lSM.transform.up = Vector3.Lerp(lSM.transform.up, (PSM.transform.forward).normalized, 4.8f * Time.deltaTime);
        }
        // rotate the ladder to the right rotation
        Vector3 RailPosition = Vector3.zero;
        float animationTime=0;
        float maxAnimationTime = ExtensionMethods.Remap(Vector3.Distance(PSM.transform.position,RailPosition),1.25f,5f,0,1);

        if (animationTime > maxAnimationTime) 
        {
            //start swingingstate and folding down
            LadderSizeStateMachine.OnGrow();
        }

    }

    //Calculates the duration of the animation at which it has the apropiat length
    
    public override IEnumerator Finish()
    {
        LadderSizeStateMachine.isUnFolding = false;
        return base.Finish();
    }
}
