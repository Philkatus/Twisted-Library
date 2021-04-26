using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PlayerSwinging : PlayerSliding 
{
    PlayerMovementStateMachine pSM;
    ValuesScriptableObject stats;
    public override void Initialize()
    {
        pSM = PlayerStateMachine;
        stats = pSM.valuesAsset;
        pSM.swingingPosition = 0;
        base.Initialize();

    }

    public override void Movement()
    {
       
        
        base.Movement();
        Swing();
    }

    public override void Swing()
    {
        VertexPath path = pSM.closestShelf.pathCreator.path;
        Vector3 SwingingDirection = pSM.ladder.forward;
        pSM.playerVelocity -= SwingingDirection * stats.SwingingAcceleration * pSM.swingingInput*Time.deltaTime-SwingingDirection*pSM.swingingPosition*stats.SwingingDrag*Time.deltaTime;
        float swingingVelocity = pSM.resultingSpeed(pSM.playerVelocity, SwingingDirection);
        swingingVelocity = Mathf.Clamp(swingingVelocity, -stats.maxSwingSpeed, stats.maxSwingSpeed);
        Quaternion targetRotation = Quaternion.AngleAxis(swingingVelocity, pSM.ladder.right);
        pSM.ladder.rotation = targetRotation * pSM.ladder.rotation;

        
        pSM.swingingPosition = Mathf.Sin(Vector3.Dot(pSM.ladder.up,path.GetNormalAtDistance(pSM.currentDistance)));



    }



    public PlayerSwinging(PlayerMovementStateMachine playerStateMachine)
       : base(playerStateMachine)
    {

    }

}
    

