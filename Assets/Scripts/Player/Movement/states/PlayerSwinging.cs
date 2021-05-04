using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PlayerSwinging : PlayerSliding
{
    PlayerMovementStateMachine pSM;
    ValuesScriptableObject stats;
    Shelf closestShelf;

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

    float dt = 0.01f;
    float accumulator = 0f;
    Shelf.ShelfType shelfType;

    bool onWall;
    float maxSwingingAngle;

    Vector3 repelDirection;

    // We use these to smooth between values in certain framerate situations in the `Update()` loop
    Vector3 currentStatePosition;
    Vector3 previousStatePosition;
    Vector3 playerVelocity;
    Vector3 inputForce;
    Vector3 currentMovement = Vector3.zero;

    // makes sure, we can only give input once per direction
    bool inputGiven;

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

        bobStartingPosition = Bob.transform.position;
        bobStartingPositionSet = true;

        shelfType = closestShelf.shelfType;
        maxSwingingAngle = closestShelf.maxSwingingAngle;
        onWall = true;
        inputGiven = false;
        // Get the initial rope length from how far away the bob is now
        ropeLength = Vector3.Distance(Pivot.transform.position, Bob.transform.position);

        //reset pendulum forces
        currentVelocity = Vector3.zero;
        // Set the transition state
        currentStatePosition = Bob.transform.position;

        currentVelocity += pSM.playerVelocity;
    }

    public override void Movement()
    {
        base.Movement();
        Swing();
    }

    public override void Swing()
    {

        VertexPath path = pSM.closestShelf.pathCreator.path;
        Vector3 SwingingDirection = pSM.ladderMesh.forward;
        Vector3 swingingAxis = pSM.ladder.right;

        float frameTime = Time.deltaTime;
        accumulator += frameTime;
        // immer wenn accumulator 0.01f ist, startet er eine while schleife, die die neue current Position berechnet. Das macht er (accumulator/dt)-Mal
        while (accumulator >= dt)
        {
            previousStatePosition = currentStatePosition;
            switch (shelfType)
            {
                case Shelf.ShelfType.TwoSided:
                    currentStatePosition = PendulumUpdate(currentStatePosition);
                    break;
                case Shelf.ShelfType.OnWall:
                    currentStatePosition = RepelUpdate(currentStatePosition);
                    break;
                case Shelf.ShelfType.FreeHanging:
                    currentStatePosition = HangingUpdate(currentStatePosition);
                    break;
            }
            accumulator -= dt;
        }
        float alpha = accumulator / dt;
        Vector3 newPosition = currentStatePosition * alpha + previousStatePosition * (1f - alpha);

        //die Leiter korrekt rotieren
        Vector3 axis = pSM.ladder.right;
        float rotateByAngle = (Vector3.SignedAngle(-pSM.ladderDirection, newPosition - pSM.ladder.transform.position, axis));
        Quaternion targetRotation = Quaternion.AngleAxis(rotateByAngle, axis);
        pSM.ladder.rotation = targetRotation * pSM.ladder.rotation;
    }

    Vector3 PendulumUpdate(Vector3 currentStatePosition)
    {
        // Erstellt eine Gravity und addiert sie auf die currentVelocity
        gravityForce = mass * stats.SwingingGravity;
        gravityDirection = Physics.gravity.normalized;
        currentVelocity += gravityDirection * gravityForce * dt;

        Vector3 pivot_p = Pivot.transform.position;
        Vector3 bob_p = this.currentStatePosition;

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

        // Check for Direction Change
        if (Vector3.Dot(currentMovement, pSM.transform.forward) <= .97f)
        {
            inputGiven = false;
        }

        //Acceleration
        inputForce = Vector3.zero;
        pSM.snapAction.started += context => AccelerationForce();

        // set max speed
        currentVelocity = currentVelocity.normalized * Mathf.Clamp(currentVelocity.magnitude, 0, stats.maxSwingSpeed);

        //Deceleration 
        float decelerationFactor = stats.SwingingDeceleration;
        // the higher the velocity, the higher the deceleration Factor
        decelerationFactor = (currentVelocity.magnitude) / (stats.maxSwingSpeed) * (0.03f - decelerationFactor) + decelerationFactor;
        currentVelocity = currentVelocity.normalized * (currentVelocity.magnitude * (1 - decelerationFactor));

        // Get only the forward/backward force
        playerVelocity = Bob.transform.forward * pSM.resultingSpeed(Bob.transform.forward, currentVelocity);

        // pSM.playerVelocity for the Jump
        SetCurrentPlayerVelocity(pivot_p);

        //Debug.DrawRays
        Debug.DrawRay(Bob.transform.position + pSM.transform.up * 0.1f, currentVelocity, Color.cyan, dt);
        Debug.DrawRay(Bob.transform.position, playerVelocity, Color.white, dt);
        Debug.DrawRay(pSM.transform.position, pSM.playerVelocity, Color.magenta, dt);
        Debug.DrawRay(pSM.transform.position + pSM.transform.up * 0.01f, currentMovement, Color.green, dt);
        Debug.DrawRay(Bob.transform.position, inputForce, Color.black, dt);

        // Get the movement delta
        Vector3 movementDelta = Vector3.zero;
        movementDelta += playerVelocity * dt;
        return GetPointOnLine(pivot_p, currentStatePosition + movementDelta, ropeLength);
    }

    Vector3 RepelUpdate(Vector3 currentStatePosition)
    {
        // Get normal at current position
        repelDirection = -Bob.transform.forward;
        Vector3 pivot_p = Pivot.transform.position;
        Vector3 bob_p = this.currentStatePosition;
        bool movingForward = Vector3.Dot(currentMovement.normalized, Bob.transform.forward) >= .93f;

        //Check if OnWall
        if (movingForward && !onWall)
        {
            Vector3 axis = pSM.ladder.right;
            float angle = Vector3.SignedAngle(pivot_p + Vector3.down, pivot_p - pSM.ladderDirection, axis);
            Debug.Log(angle);
            if (angle >= -0.01f)
            {
                
                onWall = true;
            }
        }

        if (onWall)
        {
            currentVelocity = Vector3.zero;
            currentStatePosition = pivot_p + Vector3.down * ropeLength;
        }
        else
        {
            // Erstellt eine Gravity und addiert sie auf die currentVelocity
            gravityForce = mass * stats.SwingingGravity;
            gravityDirection = Physics.gravity.normalized;
            currentVelocity += gravityDirection * gravityForce * dt;


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
        }

        //Acceleration
        inputForce = Vector3.zero;
        pSM.snapAction.started += context => RepellingForce();

        // set max speed
        currentVelocity = currentVelocity.normalized * Mathf.Clamp(currentVelocity.magnitude, 0, stats.maxSwingSpeed);

        // Get only the forward/backward force
        playerVelocity = Bob.transform.forward * pSM.resultingSpeed(Bob.transform.forward, currentVelocity);

        // pSM.playerVelocity for the Jump
        SetCurrentPlayerVelocity(pivot_p);

        //Debug.DrawRays
        Debug.DrawRay(Bob.transform.position + pSM.transform.up * 0.1f, currentVelocity, Color.cyan, dt);
        Debug.DrawRay(Bob.transform.position, playerVelocity, Color.white, dt);
        Debug.DrawRay(pSM.transform.position, pSM.playerVelocity, Color.magenta, dt);
        Debug.DrawRay(pSM.transform.position + pSM.transform.up * 0.01f, currentMovement, Color.green, dt);
        Debug.DrawRay(Bob.transform.position, inputForce, Color.black, dt);

        // Get the movement delta
        Vector3 movementDelta = Vector3.zero;
        movementDelta += playerVelocity * dt;
        return GetPointOnLine(pivot_p, currentStatePosition + movementDelta, ropeLength);
    }

    Vector3 HangingUpdate(Vector3 currentStatePosition)
    {
        // Get normal at current position
        repelDirection = -Bob.transform.forward;
        Vector3 pivot_p = Pivot.transform.position;
        Vector3 bob_p = this.currentStatePosition;
        bool movingForward = Vector3.Dot(currentMovement.normalized, Bob.transform.forward) >= .93f;

        //Check if OnWall
        if (movingForward && !onWall)
        {
            Vector3 axis = pSM.ladder.right;
            float angle = Vector3.SignedAngle(pivot_p + Vector3.down, pivot_p - pSM.ladderDirection, axis);
            Debug.Log(angle);
            if (angle >= -0.01f)
            {

                onWall = true;
            }
        }

        if (onWall)
        {
            currentVelocity = Vector3.zero;
            currentStatePosition = pivot_p + Vector3.down * ropeLength;
        }
        else
        {
            // Erstellt eine Gravity und addiert sie auf die currentVelocity
            gravityForce = mass * stats.SwingingGravity;
            gravityDirection = Physics.gravity.normalized;
            currentVelocity += gravityDirection * gravityForce * dt;


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
        }

        //Acceleration
        inputForce = Vector3.zero;
        pSM.snapAction.started += context => RepellingForce();

        // set max speed
        currentVelocity = currentVelocity.normalized * Mathf.Clamp(currentVelocity.magnitude, 0, stats.maxSwingSpeed);

        // Get only the forward/backward force
        playerVelocity = Bob.transform.forward * pSM.resultingSpeed(Bob.transform.forward, currentVelocity);

        // pSM.playerVelocity for the Jump
        SetCurrentPlayerVelocity(pivot_p);

        //Debug.DrawRays
        Debug.DrawRay(Bob.transform.position + pSM.transform.up * 0.1f, currentVelocity, Color.cyan, dt);
        Debug.DrawRay(Bob.transform.position, playerVelocity, Color.white, dt);
        Debug.DrawRay(pSM.transform.position, pSM.playerVelocity, Color.magenta, dt);
        Debug.DrawRay(pSM.transform.position + pSM.transform.up * 0.01f, currentMovement, Color.green, dt);
        Debug.DrawRay(Bob.transform.position, inputForce, Color.black, dt);

        // Get the movement delta
        Vector3 movementDelta = Vector3.zero;
        movementDelta += playerVelocity * dt;
        return GetPointOnLine(pivot_p, currentStatePosition + movementDelta, ropeLength);
    }

    Vector3 GetPointOnLine(Vector3 start, Vector3 end, float distanceFromStart)
    {
        return start + (distanceFromStart * Vector3.Normalize(end - start));
    }

    void AccelerationForce()
    {
        if (Vector3.Dot(currentMovement.normalized, Bob.transform.forward) >= .93f
            && currentVelocity.magnitude < stats.maxSwingSpeed
            && !inputGiven)
        {
            inputForce = Bob.transform.forward * stats.SwingingAcceleration * dt;
            currentVelocity += inputForce;
            inputGiven = true;
        }
    }

    void RepellingForce()
    {
        if (onWall)
        {
            inputForce = repelDirection * stats.SwingingAcceleration * dt;
            Debug.Log(repelDirection + " * " + stats.SwingingAcceleration + " * " + dt + " = " + inputForce);
            currentVelocity += inputForce;
            onWall = false;
        }
    }

    void SetCurrentPlayerVelocity(Vector3 pivot_p)
    {
        // Set currentMovement Force
        float maxJumpSpeed = stats.maximumMovementSpeed * stats.jumpingDrag;
        float playerHeightOnLadder = (pivot_p - pSM.transform.position).magnitude;
        //remap the height of the player on the ladder from 0 -> ladderLength to 0.1f -> 1
        playerHeightOnLadder = (playerHeightOnLadder) / (ropeLength) * (1 - 0.1f) + 0.1f;
        maxJumpSpeed = maxJumpSpeed * playerHeightOnLadder;
        currentMovement = playerVelocity.normalized * Mathf.Clamp(playerVelocity.magnitude, 0, maxJumpSpeed);
        pSM.playerVelocity = currentMovement;
    }

    public PlayerSwinging(PlayerMovementStateMachine playerStateMachine)
   : base(playerStateMachine)
    {

    }

}


