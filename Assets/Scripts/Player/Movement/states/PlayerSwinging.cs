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
    Vector3 playerVelocity;
    float ladderLength;

    public override void Initialize()
    {
        pSM = PlayerStateMachine;
        stats = pSM.valuesAsset;
        pSM.swingingPosition = 0;
        base.Initialize();
        closestShelf = pSM.closestShelf;
        playerVelocity = Vector3.zero;

        ladderLength = pSM.ladder.GetComponent<LadderSizeStateMachine>().ladderLength;
        Pivot = pSM.ladder.gameObject; //ist ein gameObject, weil sich der Pivot ja verschiebt, wenn man slidet

        Bob = Pivot.transform.GetChild(1).gameObject;
        Bob.transform.position = pSM.ladder.transform.position + -pSM.ladderDirection * ladderLength;
        Bob.transform.parent = null;
        bobStartingPosition = Bob.transform.position;
        bobStartingPositionSet = true;
        PendulumInit();
    }

    float dt = 0.01f;
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

        #region PendulumTry


        float frameTime = Time.deltaTime;
        accumulator += frameTime;
        // immer wenn accumulator 0.01f ist, startet er eine while schleife, die die neue current Position berechnet. Das macht er (accumulator/dt)-Mal
        while (accumulator >= dt)
        {
            previousStatePosition = currentStatePosition;
            currentStatePosition = PendulumUpdate(currentStatePosition);
            accumulator -= dt;
        }
        float alpha = accumulator / dt;
        Vector3 newPosition = currentStatePosition * alpha + previousStatePosition * (1f - alpha);

        //die Leiter korrekt rotieren
        Vector3 axis = pSM.ladder.right;
        float rotateByAngle = (Vector3.SignedAngle(- pSM.ladderDirection, newPosition - pSM.ladder.transform.position, axis));
        Debug.Log(rotateByAngle);
        //rotateByAngle = Mathf.Clamp(rotateByAngle, -stats.maxSwingSpeed, stats.maxSwingSpeed);
        Quaternion targetRotation = Quaternion.AngleAxis(rotateByAngle, axis);
        pSM.ladder.rotation = targetRotation * pSM.ladder.rotation;

        //move Bob
        Bob.transform.position = newPosition;
        
        #endregion
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


    Vector3 PendulumUpdate(Vector3 currentStatePosition)
    {
        Vector3 inputForce = Vector3.zero;

        // Erstellt eine Gravity und addiert sie auf die currentVelocity
        gravityForce = mass * stats.SwingingGravity;
        gravityDirection = Physics.gravity.normalized;
        currentVelocity += gravityDirection * gravityForce * dt;
        Debug.DrawRay(Bob.transform.position, gravityDirection * gravityForce * dt, Color.green, Time.deltaTime);

        Vector3 pivot_p = Pivot.transform.position;
        Vector3 bob_p = this.currentStatePosition;

        //berechnet, wie weit Pivot & bob voneinander entfernt waeren, wenn die Gravity straight auf den Bob addiert wird
        Vector3 auxiliaryMovementDelta = currentVelocity * dt;
        float distanceAfterGravity = Vector3.Distance(pivot_p, bob_p + auxiliaryMovementDelta);


        // If at the end of the rope: Technisch wollen wir sowas nicht, weil der Punkt immer am Ende des Seils ist/sein sollen ? m�ssen wir iwie �ndern 
        //if (distanceAfterGravity > this.ropeLength || Mathf.Approximately(distanceAfterGravity, ropeLength))
        //{
        //Vector richtung pivot;
        tensionDirection = (pivot_p - bob_p).normalized;

        //dreht tension direction um 90 Grad auf der x-achse, setzt y auf null & normalized sie
        pendulumSideDirection = (Quaternion.Euler(0f, 90f, 0f) * tensionDirection);
        pendulumSideDirection.Scale(new Vector3(1f, 0f, 1f));
        pendulumSideDirection.Normalize();

        //nimmt das negative Kreuzprodukt => Tangente
        tangentDirection = (-1f * Vector3.Cross(tensionDirection, pendulumSideDirection)).normalized;

        // Winkel zwischen Gravity Direction & Vektor -> Bob
        float inclinationAngle = Vector3.Angle(bob_p - pivot_p, gravityDirection);

        //Gravitystaerke * Cos(inclinationAngle)
        tensionForce = mass * stats.SwingingGravity * Mathf.Cos(Mathf.Deg2Rad * inclinationAngle);
        float centripetalForce = ((mass * Mathf.Pow(currentVelocity.magnitude, 2)) / ropeLength);

        tensionForce += centripetalForce;
        currentVelocity += tensionDirection * tensionForce * dt;

        //Deceleration 
        currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, dt * stats.SwingingDeceleration * (1 - pSM.swingingInput));

        //Acceleration
        if (pSM.swingingInput != 0 && Mathf.Abs(Vector3.Dot(-tangentDirection, Bob.transform.forward)) >= .97f)
        {
            inputForce = Bob.transform.forward * stats.SwingingAcceleration * pSM.swingingInput * dt;
        }

        playerVelocity = Bob.transform.forward * pSM.resultingSpeed(Bob.transform.forward, currentVelocity);


        //Debug.DrawRays
        Debug.DrawRay(Bob.transform.position, tensionDirection * tensionForce * dt, Color.red, Time.deltaTime);
        Debug.DrawRay(Bob.transform.position + pSM.transform.up * 0.1f, currentVelocity, Color.cyan, Time.deltaTime);
        Debug.DrawRay(Bob.transform.position, inputForce, Color.black, Time.deltaTime);
        Debug.DrawRay(Bob.transform.position, tangentDirection, Color.blue, Time.deltaTime);
        Debug.DrawRay(Bob.transform.position, playerVelocity, Color.white, Time.deltaTime);
        //}

        // Get the movement delta
        Vector3 movementDelta = Vector3.zero;
        movementDelta += (playerVelocity + inputForce) * dt;
        return GetPointOnLine(pivot_p, currentStatePosition + movementDelta, ropeLength);
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


