using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PlayerSwinging : PlayerSliding
{
    PlayerMovementStateMachine pSM;
    ValuesScriptableObject stats;
    Shelf closestShelf;

    float SwingingDistance;
    float currentMovementForce;
    int currentMovementDirection; // 1 or -1
    float previousInput;
    public override void Initialize()
    {
        pSM = PlayerStateMachine;
        stats = pSM.valuesAsset;
        pSM.swingingPosition = 0;
        base.Initialize();
        closestShelf = pSM.closestShelf;
        currentMovementForce = 1;
        currentMovementDirection = 1;
        float previousInput = 1;
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
        /*
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

        */
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
        #region MariaTry 

        //nimmt den y Wert der Leiter im Einheitskreis (wert liegt zwischen -1 & 1)
        float relativeHeight = -pSM.ladderDirection.normalized.y;

        //remap relative height from 1 -> (-1) to 1 -> 0.1 
        relativeHeight = (relativeHeight - 1) / (-1 - 1) * (0.1f - 1) + 1;

        //relative height liegt zwischen 0.1 & 1, Gravity zwischen 0 & 1: Je höher die Relative Höhe, umso langsamer die Gesamgtgeschwindigkeit
        float gravityMultiplier = relativeHeight * stats.SwingingGravity;

        //nimmt aus der Geradigen Bewegung einen float force: wie viel Kraft hat die Bewegung gerade?
        currentMovementForce = pSM.playerVelocity.magnitude / (SwingingDirection.magnitude*Time.deltaTime);
        Debug.Log(pSM.playerVelocity.magnitude + " / " + SwingingDirection.magnitude + " = " + currentMovementForce);
        //currentMovementForce = Mathf.Clamp(currentMovementForce, 0, stats.maxSwingSpeed);

        //Wenn es vorne oder hinten nicht mehr weitergeht WIP
        float thisInput = pSM.swingingInput;
        if (thisInput == 0 && previousInput != 0 || thisInput == 1 && previousInput != 1)
        {
            //Wechsele Schwingrichtung aus und pass auf, dass sie nicht sofort zurückwechselt?
            currentMovementDirection *= -1;
        }
        //Bei einem Input 
        pSM.playerVelocity = SwingingDirection * pSM.swingingInput * stats.SwingingAcceleration;

        Vector3 toRestingPoint = Vector3.down - -pSM.ladderDirection.normalized;
        toRestingPoint = toRestingPoint.normalized * 0.1f;
        //Bei keinem Input
        if (pSM.swingingInput == 0)
        {
            //Deceleration
            pSM.playerVelocity += (toRestingPoint * stats.SwingingDeceleration);
            Debug.DrawRay(pSM.transform.position, toRestingPoint * stats.SwingingDeceleration, Color.red); //Deceleration Force
        }
        //Gravity 
        pSM.playerVelocity +=  toRestingPoint * gravityMultiplier;

        // PlayerForce & Bewegung in die Richtige Richtung
        pSM.playerVelocity += currentMovementDirection*-10 * SwingingDirection; // * currentMovementForce;

        //Debugs
        Debug.DrawRay(pSM.transform.position, pSM.playerVelocity, Color.blue); //playerVelocity
        Debug.DrawRay(pSM.transform.position, (currentMovementDirection*-10) * SwingingDirection , Color.green); //Constant Movement* currentMovementForce
        Debug.DrawRay(pSM.transform.position, SwingingDirection * pSM.swingingInput * stats.SwingingAcceleration, Color.black); //Input Movement
        Debug.DrawRay(pSM.transform.position, toRestingPoint * gravityMultiplier, Color.yellow); //Gravity Movement

        // * Time Deltatime so it doesnt get lost on accident
        pSM.playerVelocity *= Time.deltaTime;

        //apply the rotation to the ladder
        Vector3 axis = pSM.closestShelf.pathCreator.bezierPath.GetPoint(0) - pSM.closestShelf.pathCreator.bezierPath.GetPoint(1);
        Vector3 outwardVector = pSM.ladderMesh.transform.position - pSM.ladder.transform.position;
        float swingingVelocity = Vector3.SignedAngle(outwardVector, outwardVector + pSM.playerVelocity, axis);

        //remap swingingVelocity from -180 -> 180 to .stats.maxSwingSpeed -> stats.maxSwingSpeed
        swingingVelocity = (swingingVelocity + 180) / (180 + 180) * (stats.maxSwingSpeed + stats.maxSwingSpeed) - stats.maxSwingSpeed;
        Quaternion targetRotation = Quaternion.AngleAxis(swingingVelocity, -pSM.ladder.right);
        pSM.ladder.rotation = targetRotation * pSM.ladder.rotation;

        //
        previousInput = thisInput;

        #endregion
    }



    public PlayerSwinging(PlayerMovementStateMachine playerStateMachine)
       : base(playerStateMachine)
    {

    }

}


