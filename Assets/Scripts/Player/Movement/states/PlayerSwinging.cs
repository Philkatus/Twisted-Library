using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PlayerSwinging : PlayerSliding 
{
    PlayerMovementStateMachine pSM;
    ValuesScriptableObject stats;

    float SwingingDistance;
    public override void Initialize()
    {
        pSM = PlayerStateMachine;
        stats = pSM.valuesAsset;
        pSM.swingingPosition = 0;
        base.Initialize();

    }

    public override void Movement()
    {
       
        
        //base.Movement();
        Swing();
    }

    public override void Swing()
    {
        VertexPath path = pSM.closestShelf.pathCreator.path;
        Vector3 SwingingDirection = pSM.ladder.forward;

        
            pSM.playerVelocity += SwingingDirection* stats.SwingingAcceleration* pSM.swingingInput* Time.deltaTime* Time.deltaTime*10;
        

            //pSM.playerVelocity -= SwingingDirection * stats.SwingingAcceleration * pSM.swingingInput * Time.deltaTime * Time.deltaTime*10 ;
        

        pSM.playerVelocity +=  -SwingingDirection* stats.SwingingGravity*Mathf.Sin(SwingingDistance) * Time.deltaTime;



        float swingingVelocity = pSM.resultingSpeed(pSM.playerVelocity, SwingingDirection);
        pSM.swingingPosition -= swingingVelocity;
        swingingVelocity = Mathf.Clamp(swingingVelocity , -stats.maxSwingSpeed, stats.maxSwingSpeed);
        Quaternion targetRotation = Quaternion.AngleAxis(-swingingVelocity, pSM.ladder.right);
        pSM.ladder.rotation = targetRotation * pSM.ladder.rotation;

        
        pSM.swingingPosition = Mathf.Clamp(pSM.swingingPosition,0,360);
        SwingingDistance = Mathf.Rad2Deg*Mathf.Tan(pSM.swingingPosition) ;


    }



    public PlayerSwinging(PlayerMovementStateMachine playerStateMachine)
       : base(playerStateMachine)
    {

    }

}
    

