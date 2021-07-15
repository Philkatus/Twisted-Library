using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderFollow : State
{
    ValuesScriptableObject stats;
    bool isLerpGoing;
    float time = 0;
    float noisetimer;
    Vector3 ladderVisualLocalposition;
    Quaternion ladserVisualLocalRotation;

    public LadderFollow(LadderSizeStateMachine ladderSizeStateMachine) : base(ladderSizeStateMachine)
    {

    }

    public override void Initialize()
    {
        stats = PSM.stats;
        time = ExtensionMethods.Remap(LadderSizeStateMachine.ladderLength, stats.ladderLengthSmall, stats.ladderLengthBig, stats.foldingTime, 0);
        isLerpGoing = true;
        ladderVisualLocalposition = LadderSizeStateMachine.LadderVisuals.localPosition;
        ladserVisualLocalRotation = LadderSizeStateMachine.LadderVisuals.localRotation;

        LadderSizeStateMachine.anim.SetTrigger("Ladder Retract");
    }

    public override void Fold()
    {
        
    }

    public override void FollowLadderTarget()
    {
        LadderSizeStateMachine lSM = LadderSizeStateMachine;
        Vector3 followTarget;
        noisetimer += Time.deltaTime/2;
        float yOffset = Mathf.PerlinNoise(1, noisetimer);
        yOffset = ExtensionMethods.Remap(yOffset, 0f, 1, -.4f, .4f);
        float xOffset = Mathf.PerlinNoise(200, noisetimer);
        xOffset = ExtensionMethods.Remap(xOffset, 0, 1, -.4f, .4f);
        float zOffset = Mathf.PerlinNoise(3000, noisetimer);
        zOffset = ExtensionMethods.Remap(zOffset, 0, 1, -.4f, .4f);


        followTarget = new Vector3(lSM.followTarget.position.x +xOffset ,lSM.followTarget.position.y+yOffset , lSM.followTarget.position.z+zOffset);
        lSM.transform.position = Vector3.Lerp(lSM.transform.position,lSM.followTarget.position, 3.5f*Time.deltaTime);

        if (Vector3.Distance(lSM.transform.position, followTarget) > 1.8f)
        {
            lSM.transform.up = Vector3.Lerp(lSM.transform.up, (-lSM.LadderVisuals.position + followTarget).normalized, 4.8f* Time.deltaTime);
            lSM.LadderVisuals.forward = Vector3.Lerp(lSM.LadderVisuals.forward, (-lSM.LadderVisuals.position + followTarget).normalized, 4.8f * Time.deltaTime);
        }
        else
        {
            Vector3 followDirection = new Vector3(PSM.transform.forward.x + xOffset, PSM.transform.forward.y + yOffset, PSM.transform.forward.z + zOffset).normalized;
            lSM.transform.up = Vector3.Lerp(lSM.transform.up, PSM.transform.forward, 2 * Time.deltaTime);
            lSM.LadderVisuals.forward = Vector3.Lerp(lSM.LadderVisuals.forward, followDirection,3 * Time.deltaTime);
            lSM.LadderVisuals.localPosition = ladderVisualLocalposition+new Vector3(zOffset/2,xOffset/2,yOffset/2);
        }

    }

    public override IEnumerator Finish()
    {
        //also set false when changing state because that happens before the timer above is over
        LadderSizeStateMachine.isFoldingUp = false;
        LadderSizeStateMachine.LadderVisuals.transform.localPosition = ladderVisualLocalposition;
        LadderSizeStateMachine.LadderVisuals.transform.localRotation = ladserVisualLocalRotation;

        yield break;
    }
}
