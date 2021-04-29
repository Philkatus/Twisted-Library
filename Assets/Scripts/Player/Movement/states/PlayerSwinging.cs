using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PlayerSwinging : PlayerSliding
{
    PlayerMovementStateMachine pSM;
    ValuesScriptableObject stats;
    Shelf closestShelf;

    Vector3 swingingVelocity;

    float SwingingDistance;
    float currentMovementForce;
    int currentMovementDirection; // 1 or -1
    float previousInput;

    GameObject Pivot;
    GameObject Bob;

    float mass = 1f;
    float ropeLength = 2f;

    Vector3 bobStartingPosition;
    bool bobStartingPositionSet = false;

    // You could define these in the `PendulumUpdate()` loop 
    // But we want them in the class scope so we can draw gizmos `OnDrawGizmos()`
    private Vector3 gravityDirection;
    private Vector3 tensionDirection;

    private Vector3 tangentDirection;
    private Vector3 pendulumSideDirection;

    private float tensionForce = 0f;
    private float gravityForce = 0f;


    // Keep track of the current velocity
    Vector3 currentVelocity = new Vector3();

    // We use these to smooth between values in certain framerate situations in the `Update()` loop
    Vector3 currentStatePosition;
    Vector3 previousStatePosition;

    float ladderLength; 

    public override void Initialize()
    {
        pSM = PlayerStateMachine;
        stats = pSM.valuesAsset;
        pSM.swingingPosition = 0;
        base.Initialize();
        closestShelf = pSM.closestShelf;

        /*//new try
        ladderLength = pSM.ladder.GetComponent<LadderSizeStateMachine>().ladderLength;
        Pivot = pSM.ladder.gameObject; //ist ein gameObject, weil sich der Pivot ja verschiebt, wenn man slidet
        
        Bob = Pivot.transform.GetChild(1).gameObject;
        Bob.transform.position = pSM.ladder.transform.position + -pSM.ladderDirection * ladderLength;

        bobStartingPosition = Bob.transform.position;
        bobStartingPositionSet = true;
        PendulumInit();*/
    }

    float t = 0f;
    float dt = 0.01f;
    float currentTime = 0f;
    float accumulator = 0f;

    public override void Movement()
    {
        //base.Movement();
        Swing();
    }

    public override void Swing()
    {

        VertexPath path = pSM.closestShelf.pathCreator.path;
        Vector3 SwingingDirection = pSM.ladderMesh.forward;
        Vector3 swingingAxis = pSM.ladder.right;
        #region first try
        
        pSM.playerVelocity -= SwingingDirection * stats.SwingingGravity * Mathf.Sin(SwingingDistance) * Time.deltaTime;
            


        pSM.playerVelocity -= SwingingDirection * stats.SwingingAcceleration * pSM.swingingInput * Time.deltaTime * Time.deltaTime*10 ;
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
        
        
        #endregion

        #region MariaTry 
        /*
        //nimmt den y Wert der Leiter im Einheitskreis (wert liegt zwischen -1 & 1)
        float relativeHeight = -pSM.ladderDirection.normalized.y;

        //remap relative height from 1 -> (-1) to 1 -> 0.1 
        relativeHeight = (relativeHeight - 1) / (-1 - 1) * (0.1f - 1) + 1;

        //relative height liegt zwischen 0.1 & 1, Gravity zwischen 0 & 1: Je h�her die Relative H�he, umso langsamer die Gesamgtgeschwindigkeit
        float gravityMultiplier = relativeHeight * stats.SwingingGravity;

        //nimmt aus der Geradigen Bewegung einen float force: wie viel Kraft hat die Bewegung gerade?
        currentMovementForce = pSM.playerVelocity.magnitude / (SwingingDirection.magnitude * Time.deltaTime);
        Debug.Log(pSM.playerVelocity.magnitude + " / " + SwingingDirection.magnitude + " = " + currentMovementForce);
        //currentMovementForce = Mathf.Clamp(currentMovementForce, 0, stats.maxSwingSpeed);

        //Wenn es vorne oder hinten nicht mehr weitergeht WIP
        float thisInput = pSM.swingingInput;
        if (thisInput == 0 && previousInput != 0 || thisInput == 1 && previousInput != 1)
        {
            //Wechsele Schwingrichtung aus und pass auf, dass sie nicht sofort zur�ckwechselt?
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
        pSM.playerVelocity += toRestingPoint * gravityMultiplier;

        // PlayerForce & Bewegung in die Richtige Richtung
        pSM.playerVelocity += currentMovementDirection * -10 * SwingingDirection; // * currentMovementForce;

        //Debugs
        Debug.DrawRay(pSM.transform.position, pSM.playerVelocity, Color.blue); //playerVelocity
        Debug.DrawRay(pSM.transform.position, (currentMovementDirection * -10) * SwingingDirection, Color.green); //Constant Movement* currentMovementForce
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
        */
        #endregion
       /* #region PendulumTry

        //Wie viel Zeit vergeht zwischen 2 Frames? Warum nicht time.deltatime??
        float frameTime = Time.time - currentTime;
        currentTime = Time.time;

        accumulator += frameTime;

        //immer wenn accumulator 0.01f ist, startet er eine while schleife, die die neue current Position berechnet. Das macht er (dt/accumulator)-Mal
        while (accumulator >= dt)
        {
            previousStatePosition = currentStatePosition;
            currentStatePosition = PendulumUpdate(currentStatePosition, dt);
            //integrate(state, this.t, this.dt);
            accumulator -= dt;
            t += dt;
        }

        float alpha = accumulator / dt;

        Vector3 newPosition = currentStatePosition * alpha + previousStatePosition * (1f - alpha);
        //Nicht Bob.transform.position sondern die Leiter nach da rotieren
        //this.Bob.transform.position = newPosition;
        Vector3 axis = pSM.ladder.right;
        float rotateByAngle = Vector3.SignedAngle(previousStatePosition, newPosition + pSM.playerVelocity, axis);
        Quaternion targetRotation = Quaternion.AngleAxis(rotateByAngle, -pSM.ladder.right);
        pSM.ladder.rotation = targetRotation * pSM.ladder.rotation;
        #endregion */
    }
    void PendulumInit()
    {
        // Get the initial rope length from how far away the bob is now
        this.ropeLength = Vector3.Distance(Pivot.transform.position, Bob.transform.position);
        this.ResetPendulumForces();
    }

    void MoveBob(Vector3 resetBobPosition)
    {
        // Put the bob back in the place we first saw it at in `Start()`
        this.Bob.transform.position = resetBobPosition;

        // Set the transition state
        this.currentStatePosition = resetBobPosition;
    }


    Vector3 PendulumUpdate(Vector3 currentStatePosition, float deltaTime)
    {
        // Erstellt eine Gravity und addiert sie auf die currentVelocity
        gravityForce = mass * stats.SwingingGravity;
        gravityDirection = Physics.gravity.normalized;
        currentVelocity += gravityDirection * gravityForce * deltaTime;

        Vector3 pivot_p = Pivot.transform.position;
        Vector3 bob_p = this.currentStatePosition;

        //berechnet, wie weit Pivot & bob voneinander entfernt w�ren, wenn die Gravity straight auf den Bob addiert wird
        Vector3 auxiliaryMovementDelta = currentVelocity * deltaTime;
        float distanceAfterGravity = Vector3.Distance(pivot_p, bob_p + auxiliaryMovementDelta);

        // If at the end of the rope: Technisch wollen wir sowas nicht, weil der Punkt immer am Ende des Seils ist/sein sollen ? m�ssen wir iwie �ndern 
        if (distanceAfterGravity > this.ropeLength || Mathf.Approximately(distanceAfterGravity, ropeLength))
        {
            //Vector richtung pivot;
            tensionDirection = (pivot_p - bob_p).normalized;
            //dreht tension direction um 90 Grad auf der x-achse, setzt y auf null & normalized sie
            pendulumSideDirection = (Quaternion.Euler(0f, 90f, 0f) * tensionDirection);
            pendulumSideDirection.Scale(new Vector3(1f, 0f, 1f));
            pendulumSideDirection.Normalize();
            //nimmt das negative Kreuzprodukt
            tangentDirection = (-1f * Vector3.Cross(tensionDirection, pendulumSideDirection)).normalized;
            // Winkel zwischen Gravity Direction & Vektor -> Bob
            float inclinationAngle = Vector3.Angle(bob_p - pivot_p, gravityDirection);
            //Gravityst�rke * Cos(inclinationAngle)
            tensionForce = mass * stats.SwingingGravity * Mathf.Cos(Mathf.Deg2Rad * inclinationAngle);
            float centripetalForce = ((mass * Mathf.Pow(currentVelocity.magnitude, 2)) / ropeLength);
            tensionForce += centripetalForce;

            currentVelocity += tensionDirection * tensionForce * deltaTime;
        }

        // Get the movement delta
        Vector3 movementDelta = Vector3.zero;
        movementDelta += currentVelocity * deltaTime;


        //Ist das Seil zu lang? return currentStatePosition + movementDelta
        float distance = Vector3.Distance(pivot_p, currentStatePosition + movementDelta);
        //bekomm den punkt ropelength oder distance in movementrichtung
        return this.GetPointOnLine(pivot_p, currentStatePosition + movementDelta, distance <= this.ropeLength ? distance : this.ropeLength);
    }

    Vector3 GetPointOnLine(Vector3 start, Vector3 end, float distanceFromStart)
    {
        return start + (distanceFromStart * Vector3.Normalize(end - start));
    }
    void ResetPendulumForces()
    {
        currentVelocity = Vector3.zero;

        // Set the transition state
        currentStatePosition = Bob.transform.position;
    }

    void ResetPendulumPosition()
    {
        if (this.bobStartingPositionSet)
            this.MoveBob(this.bobStartingPosition);
        else
            this.PendulumInit();
    }


    public PlayerSwinging(PlayerMovementStateMachine playerStateMachine)
       : base(playerStateMachine)
    {

    }

}


