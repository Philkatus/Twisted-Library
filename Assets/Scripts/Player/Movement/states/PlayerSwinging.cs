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
        Vector3 SwingingDirection = pSM.ladderMesh.forward;
        #region first try

        pSM.playerVelocity -= SwingingDirection * stats.SwingingGravity * Mathf.Sin(SwingingDistance) * Time.deltaTime;
            


        //pSM.playerVelocity -= SwingingDirection * stats.SwingingAcceleration * pSM.swingingInput * Time.deltaTime * Time.deltaTime*10 ;
        Debug.Log(SwingingDirection + " "+pSM.playerVelocity.normalized +" "+ pSM.resultingSpeed(pSM.playerVelocity.normalized, SwingingDirection));
        if (pSM.resultingSpeed(pSM.playerVelocity.normalized, SwingingDirection) <= 0)
        {
            pSM.playerVelocity -= SwingingDirection * stats.SwingingAcceleration * pSM.swingingInput * Time.deltaTime;
                

        }
        //pSM.playerVelocity += SwingingDirection * stats.SwingingDeceleration * Mathf.Sin(SwingingDistance) * Time.deltaTime;

        float swingingVelocity = pSM.resultingSpeed(pSM.playerVelocity, Vector3.up);
        Debug.DrawRay(pSM.transform.position, pSM.resultingVelocity(pSM.playerVelocity, Vector3.up),Color.black);
        pSM.playerVelocity -= pSM.resultingVelocity(pSM.playerVelocity,Vector3.up);
        swingingVelocity = Mathf.Clamp(swingingVelocity * (10000-stats.SwingingDeceleration)/10000, -stats.maxSwingSpeed, stats.maxSwingSpeed);
        pSM.playerVelocity += swingingVelocity * Vector3.up;
        pSM.swingingPosition -= swingingVelocity;
        Quaternion targetRotation = Quaternion.AngleAxis(-swingingVelocity, pSM.ladder.right);
        pSM.ladder.rotation = targetRotation * pSM.ladder.rotation;

        
        pSM.swingingPosition = Mathf.Repeat(pSM.swingingPosition,360);
        SwingingDistance = Vector3.Distance(pSM.ladder.transform.position+ pSM.ladderDirection, pSM.ladder.transform.position+ Vector3.up) ;

        
        #endregion

        #region second try
        /*
        float PotentialEnergy=0;
        float KinetikEnergy = 0;
        float TotalEnergy = 0;
        float maximumSwingEnergy=20;
        TotalEnergy += pSM.swingingInput * stats.SwingingAcceleration * Time.deltaTime;
        TotalEnergy = Mathf.Clamp(TotalEnergy, 0, maximumSwingEnergy);
        KinetikEnergy = TotalEnergy - PotentialEnergy;

        if (pSM.SwinginForwards) 
        {
            pSM.playerVelocity -= SwingingDirection *stats.SwingingAcceleration  * Time.deltaTime;

            if (pSM.resultingSpeed(SwingingDirection, Vector3.down) >= 0)
            {
                KinetikEnergy += stats.SwingingAcceleration * Time.deltaTime;
                PotentialEnergy = TotalEnergy - KinetikEnergy;
            }
            else if (pSM.resultingSpeed(SwingingDirection, Vector3.down) < 0)
            {
                PotentialEnergy += stats.SwingingAcceleration * Time.deltaTime;
                KinetikEnergy = TotalEnergy - PotentialEnergy;
            }

        }
        else  
        {
            pSM.playerVelocity += SwingingDirection *stats.SwingingAcceleration  * Time.deltaTime;

            if (pSM.resultingSpeed(SwingingDirection, Vector3.down) < 0)
            {
                KinetikEnergy += stats.SwingingAcceleration * Time.deltaTime;
                PotentialEnergy = TotalEnergy - KinetikEnergy;
            }
            else if (pSM.resultingSpeed(SwingingDirection, Vector3.down) >= 0)
            {
                PotentialEnergy += stats.SwingingAcceleration * Time.deltaTime;
                KinetikEnergy = TotalEnergy - PotentialEnergy;
            }
        }
        

        if (KinetikEnergy <= 0) 
        {
            pSM.SwinginForwards = !pSM.SwinginForwards;
        }

        float swingingVelocity = pSM.resultingSpeed(pSM.playerVelocity, SwingingDirection);
        pSM.swingingPosition -= swingingVelocity;
        swingingVelocity = Mathf.Clamp(swingingVelocity, -stats.maxSwingSpeed, stats.maxSwingSpeed);
        Quaternion targetRotation = Quaternion.AngleAxis(-swingingVelocity, pSM.ladder.right);
        pSM.ladder.rotation = targetRotation * pSM.ladder.rotation;

        pSM.swingingPosition = Mathf.Repeat(pSM.swingingPosition, 360);
        

        */
        #endregion
    }



    public PlayerSwinging(PlayerMovementStateMachine playerStateMachine)
       : base(playerStateMachine)
    {

    }

}
    

