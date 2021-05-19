using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PlayerSwinging : PlayerSliding
{
    #region INHERITED
    //float currentDistance;
    #endregion

    GameObject Pivot;
    GameObject swingingFeedback;
    GameObject ladderParent;

    float mass = 1f;
    float ropeLength = 2f;

    private Vector3 gravityDirection;
    private Vector3 tensionDirection;
    private Vector3 bobPosition;
    private Vector3 bobForward;

    private float tensionForce = 0f;
    private float gravityForce = 0f;

    // Keep track of the current velocity
    Vector3 currentVelocity = new Vector3();

    float dt = 0.01f;
    float accumulator = 0f;
    Rail.RailType railType;

    bool onWall;
    bool canPress;
    float accelerationFactor;
    float minDecelerationFactor;
    float maxDecelerationFactor;
    float minSwingSpeed = 8f;

    float inputTimer;
    float nextSwingingTime = 1f;

    Vector3 repelDirection;

    // We use these to smooth between values in certain framerate situations in the `Update()` loop
    Vector3 currentStatePosition;
    Vector3 previousStatePosition;
    Vector3 playerVelocity;
    Vector3 inputForce;
    Vector3 currentMovement = Vector3.zero;

    // makes sure, we can only give input once per direction
    bool inputGiven;

    public override void ReInitialize()
    {
        base.ReInitialize();
        pSM.bob.transform.position = pSM.ladder.transform.position + -pSM.ladderDirection * ladderSizeState.ladderLength;
        railType = closestRail.railType;
        onWall = false;
        inputGiven = false;
        ropeLength = Vector3.Distance(Pivot.transform.position, pSM.bob.transform.position);
        switch (railType)
        {
            case Rail.RailType.TwoSided:
                pSM.snapAction.started += context => AccelerationForce();
                minDecelerationFactor = stats.minSwingingDeceleration;
                maxDecelerationFactor = stats.maxSwingingDeceleration;
                accelerationFactor = 1;
                break;
            case Rail.RailType.FreeHanging:
                pSM.snapAction.started += context => AccelerationForce();
                minDecelerationFactor = stats.minHangingDeceleration;
                maxDecelerationFactor = stats.maxHangingDeceleration;
                accelerationFactor = stats.hangingAccelerationFactor;
                break;
            case Rail.RailType.OnWall:
                pSM.snapAction.started += context => RepellingForce();
                break;
        }
    }


    public override void Initialize()
    {
        //base.Initialize();
        SnappingOrientation();

        Pivot = pSM.ladder.gameObject; //ist ein gameObject, weil sich der Pivot ja verschiebt, wenn man slidet
        pathLength = path.cumulativeLengthAtEachVertex[path.cumulativeLengthAtEachVertex.Length - 1];
        ladderSizeState = pSM.ladderSizeStateMachine;
        pSM.bob = Pivot.transform.GetChild(1).gameObject;
        pSM.bob.transform.position = pSM.ladder.transform.position + -pSM.ladderDirection * ladderSizeState.ladderLengthBig;

        ladderParent = ladderSizeState.ladderParent.gameObject;
        swingingFeedback = ladderParent.transform.GetChild(5).gameObject;

        railType = closestRail.railType;
        onWall = false;
        inputGiven = false;
        canPress = true;

        // Get the initial rope length from how far away the bob is now
        ropeLength = Vector3.Distance(Pivot.transform.position, pSM.bob.transform.position);


        currentVelocity = Vector3.zero;
        currentStatePosition = pSM.bob.transform.position;

        bobPosition = pSM.bob.transform.position;
        bobForward = pSM.bob.transform.forward;

        currentStatePosition = pSM.bob.transform.position;
        previousStatePosition = pSM.bob.transform.position;

        switch (railType)
        {
            case Rail.RailType.TwoSided:
                pSM.swingAction.started += context => AccelerationForce();
                minDecelerationFactor = stats.minSwingingDeceleration;
                maxDecelerationFactor = stats.maxSwingingDeceleration;
                accelerationFactor = 1;
                break;
            case Rail.RailType.FreeHanging:
                pSM.swingAction.started += context => AccelerationForce();
                minDecelerationFactor = stats.minHangingDeceleration;
                maxDecelerationFactor = stats.maxHangingDeceleration;
                accelerationFactor = stats.hangingAccelerationFactor;
                break;
            case Rail.RailType.OnWall:
                pSM.swingAction.started += context => RepellingForce();
                break;
        }


    }

    public override void Movement()
    {
        Swing();
        base.Movement();


        //Vector3 railDirection = path.GetNormalAtDistance(currentDistance);
        //pSM.ladder.transform.forward = -railDirection;
    }

    public override void Swing()
    {
        float frameTime = Time.fixedDeltaTime;
        accumulator += frameTime;

        // immer wenn accumulator 0.01f ist, startet er eine while schleife, die die neue current Position berechnet. Das macht er (accumulator/dt)-Mal
        while (accumulator >= dt)
        {
            previousStatePosition = currentStatePosition;
            switch (railType)
            {
                case Rail.RailType.TwoSided:
                    currentStatePosition = PendulumUpdate();
                    break;
                case Rail.RailType.OnWall:
                    currentStatePosition = RepelUpdate();
                    break;
                case Rail.RailType.FreeHanging:
                    currentStatePosition = PendulumUpdate();
                    break;
            }
            accumulator -= dt;
        }
        float alpha = accumulator / dt;
        Vector3 newPosition = currentStatePosition * alpha + previousStatePosition * (1f - alpha);

        //die Leiter korrekt rotieren
        currentDistance = pSM.currentDistance;

        Vector3 axis = pSM.ladder.right;
        float rotateByAngle = (Vector3.SignedAngle(-pSM.ladderDirection, newPosition - pSM.ladder.transform.position, axis));

        Quaternion targetRotation = Quaternion.AngleAxis(rotateByAngle, axis);
        pSM.ladder.rotation = targetRotation * pSM.ladder.rotation;

        // The values that otherwise get deleted by the rotation in Update()
        bobPosition = pSM.bob.transform.position;
        bobForward = pSM.bob.transform.forward;

        //rotate the ladder, so that its not stuck in the wall, this is a shitty fix, but otherwise wed have to rewrite A LOT
        if (onWall)
        {
            Vector3 localVector = ladder.transform.localEulerAngles;
            ladder.transform.localEulerAngles = new Vector3(0, localVector.y, localVector.z);
        }
    }

    Vector3 PendulumUpdate()
    {
        // Erstellt eine Gravity und addiert sie auf die currentVelocity
        gravityForce = mass * stats.swingingGravity;
        gravityDirection = Physics.gravity.normalized;
        currentVelocity += gravityDirection * gravityForce * dt;
        //Debug.DrawRay(bobPosition, gravityDirection * gravityForce * dt, Color.red, dt);

        Vector3 pivot_p = Pivot.transform.position;
        Vector3 bob_p = bobPosition;

        //The tension force
        tensionDirection = (pivot_p - bob_p).normalized;
        float inclinationAngle = Vector3.Angle(bob_p - pivot_p, gravityDirection);

        tensionForce = gravityForce * Mathf.Cos(Mathf.Deg2Rad * inclinationAngle);
        float centripetalForce = mass * Mathf.Pow(playerVelocity.magnitude, 2) / ropeLength;
        tensionForce += centripetalForce;

        // if relative height > 0 -> remap tension force to get smaller

        float relativeHeight = (bob_p - pivot_p).normalized.y;
        if (relativeHeight > 0)
        { // 0 - 1 -> 0.9f - 0.1f 
            tensionForce *= (relativeHeight / 1) * (0.1f - 0.9f) + 0.9f;
        }

        currentVelocity += tensionDirection * tensionForce * dt;
        //Debug.DrawRay(bobPosition, tensionDirection * tensionForce * dt, Color.green, dt);

        // Check for Direction Change
        Vector3 currentNormal = -path.GetNormalAtDistance(currentDistance);
        if (inputGiven && !new Plane(currentNormal, pivot_p).GetSide(bobPosition))
        {
            inputGiven = false;
        }

        //Check if input can be given and give feedback accordingly

        if (Vector3.Dot(currentMovement.normalized, bobForward) >= .93f
           && currentVelocity.magnitude < stats.maxSwingSpeed
           && !inputGiven
           && inputTimer > nextSwingingTime
           || currentVelocity.magnitude <= minSwingSpeed)
        {
            canPress = true;
            swingingFeedback.SetActive(true);
        }

        else
        {
            canPress = false;
            swingingFeedback.SetActive(false);
        }

        //Acceleration
        inputForce = Vector3.zero;
        inputTimer += dt;

        // set max speed
        currentVelocity = currentVelocity.normalized * Mathf.Clamp(currentVelocity.magnitude, 0, stats.maxSwingSpeed);

        //Deceleration 
        // the higher the velocity, the higher the deceleration Factor
        float DecelerationFactor = (currentVelocity.magnitude) / (stats.maxSwingSpeed) * (maxDecelerationFactor - minDecelerationFactor) + minDecelerationFactor;
        currentVelocity = currentVelocity.normalized * (currentVelocity.magnitude * (1 - DecelerationFactor));

        // Get only the forward/backward force
        playerVelocity = bobForward * pSM.resultingSpeed(currentVelocity, bobForward);

        // pSM.playerVelocity for the Jump
        SetCurrentPlayerVelocity(pivot_p);


        //Debug.DrawRays
        /*
        Debug.DrawRay(Bob.transform.position + pSM.transform.up * 0.1f, currentVelocity, Color.cyan, dt);
        Debug.DrawRay(Bob.transform.position, playerVelocity, Color.white, dt);
        Debug.DrawRay(pSM.transform.position, pSM.playerVelocity, Color.magenta, dt);
        Debug.DrawRay(pSM.transform.position + pSM.transform.up * 0.01f, currentMovement, Color.green, dt);
        Debug.DrawRay(Bob.transform.position, inputForce, Color.black, dt);
        */

        // Get the movement delta
        Vector3 movementDelta = Vector3.zero;
        movementDelta += playerVelocity * dt;
        return GetPointOnLine(pivot_p, bobPosition + movementDelta, ropeLength);
    }

    Vector3 RepelUpdate()
    {
        // Get normal at current position
        repelDirection = -bobForward;
        Vector3 pivot_p = Pivot.transform.position;
        Vector3 bob_p = bobPosition;
        bool movingForward = Vector3.Dot(currentMovement.normalized, bobForward) >= .93f;

        //Check if OnWall
        if (movingForward && !onWall)
        {
            Vector3 axis = pSM.ladder.right;
            float angle = Vector3.SignedAngle(Vector3.down, (bob_p - pivot_p).normalized, axis);
            if (angle <= stats.maxPushAngle)
            {
                onWall = true;

                return GetPointOnLine(pivot_p, pivot_p + Vector3.down * 100, ropeLength);
            }
        }

        if (onWall)
        {
            currentVelocity = Vector3.zero;
            bobPosition = pivot_p + Vector3.down * 100;
        }
        else
        {
            // Erstellt eine Gravity und addiert sie auf die currentVelocity
            gravityForce = mass * stats.swingingGravity;
            gravityDirection = Physics.gravity.normalized;
            currentVelocity += gravityDirection * gravityForce * dt;

            tensionDirection = (pivot_p - bob_p).normalized;

            // Winkel zwischen Gravity Direction & Vektor -> Bob
            float inclinationAngle = Vector3.Angle(bob_p - pivot_p, gravityDirection);

            //Gravitystaerke * Cos(inclinationAngle)
            tensionForce = mass * stats.swingingGravity * Mathf.Cos(Mathf.Deg2Rad * inclinationAngle);
            float centripetalForce = ((mass * Mathf.Pow(currentVelocity.magnitude, 2)) / ropeLength);

            tensionForce += centripetalForce;
            currentVelocity += tensionDirection * tensionForce * dt;
        }

        //Acceleration
        inputForce = Vector3.zero;

        // set max speed
        currentVelocity = currentVelocity.normalized * Mathf.Clamp(currentVelocity.magnitude, 0, stats.maxSwingSpeed);

        // Get only the forward/backward force
        playerVelocity = bobForward * pSM.resultingSpeed(bobForward, currentVelocity);

        // pSM.playerVelocity for the Jump
        SetCurrentPlayerVelocity(pivot_p);

        /*
        //Debug.DrawRays
        Debug.DrawRay(Bob.transform.position + pSM.transform.up * 0.1f, currentVelocity, Color.cyan, dt);
        Debug.DrawRay(Bob.transform.position, playerVelocity, Color.white, dt);
        Debug.DrawRay(pSM.transform.position, pSM.playerVelocity, Color.magenta, dt);
        Debug.DrawRay(pSM.transform.position + pSM.transform.up * 0.01f, currentMovement, Color.green, dt);
        Debug.DrawRay(Bob.transform.position, inputForce, Color.black, dt);
        */

        // Get the movement delta
        Vector3 movementDelta = Vector3.zero;
        movementDelta += playerVelocity * dt;
        return GetPointOnLine(pivot_p, bobPosition + movementDelta, ropeLength);
    }

    Vector3 GetPointOnLine(Vector3 start, Vector3 end, float distanceFromStart)
    {
        return start + (distanceFromStart * Vector3.Normalize(end - start));
    }

    void AccelerationForce()
    {

        if (canPress)
        {

            inputForce = bobForward * stats.swingingAcceleration * dt * accelerationFactor;
            currentVelocity += inputForce;
            inputGiven = true;
            inputTimer = 0;
        }
    }

    void RepellingForce()
    {
        if (onWall)
        {
            onWall = false;
            inputForce = repelDirection * stats.swingingAcceleration * dt * 1.2f;
            currentVelocity += inputForce;
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
        pSM.playerVelocity = (pSM.resultingVelocity(pSM.playerVelocity, pSM.ladder.right) + currentMovement) / stats.swingingVelocityFactor;

    }

    void SnappingOrientation()
    {
        #region  Variable assignment
        pSM = PlayerStateMachine;
        stats = pSM.valuesAsset;

        ladderSizeState = pSM.ladderSizeStateMachine;
        closestRail = pSM.closestRail;
        speed = stats.climbingSpeedOnLadder;
        controller = pSM.controller;
        ladder = pSM.ladder;
        pathCreator = closestRail.pathCreator;
        path = pathCreator.path;

        #endregion
        #region LadderPlacement
        Vector3 startingPoint = pathCreator.path.GetClosestPointOnPath(pSM.transform.position);
        currentDistance = path.GetClosestDistanceAlongPath(startingPoint);
        ladder.transform.position = startingPoint;
        ladder.transform.forward = -path.GetNormalAtDistance(currentDistance);

        pSM.currentDistance = currentDistance;
        ladder.transform.SetParent(pSM.myParent);
        ladder.transform.localScale = new Vector3(1, 1, 1);
        controller.transform.localScale = new Vector3(1, 1, 1);

        //Ladder Rotation
        Vector3 axis = pSM.ladder.right;
        float rotateByAngle = (Vector3.SignedAngle(-pSM.ladderDirection, pSM.transform.position - startingPoint, axis));
        if (rotateByAngle < 0)
        {
            if (rotateByAngle < -90)
            {
                rotateByAngle = 150;
            }
            else
                rotateByAngle = 0;
        }
        else
        {
            rotateByAngle = Mathf.Clamp(rotateByAngle, 0, 150);
        }
        if (rotateByAngle < 120)
        {
            currentVelocity += pSM.resultingVelocity(pSM.playerVelocity, pSM.bob.transform.forward);
            currentVelocity = Vector3.ClampMagnitude(currentVelocity, stats.maxSwingSpeed);
        }

        Quaternion targetRotation = Quaternion.AngleAxis(rotateByAngle, axis);
        pSM.ladder.rotation = targetRotation * pSM.ladder.rotation;

        //LadderLength Calculation
        pSM.ladderSizeStateMachine.ladderLength = Vector3.Distance(pSM.transform.position, startingPoint);
        pSM.ladderSizeStateMachine.OnSnap();



        #endregion
        #region PlayerPlacement
        pSM.HeightOnLadder = -1;
        controller.transform.parent = ladder.transform;
        //pSM.transform.position = ladder.transform.position + pSM.ladderDirection * ladderSizeState.ladderLength * pSM.HeightOnLadder;
        pSM.transform.localPosition = new Vector3(0, -ladderSizeState.ladderLength, -.7f);
        controller.transform.localRotation = Quaternion.Euler(6.25f, 0, 0);

        #endregion
        #region Velocity Calculation

        if (!stats.preservesVelocityOnSnap)
        {
            pSM.baseVelocity = pSM.resultingClampedVelocity(pSM.baseVelocity, ladder.transform.forward, stats.maxSwingSpeed);
            pSM.bonusVelocity = pSM.resultingVelocity(pSM.bonusVelocity, ladder.transform.forward);
            //pSM.playerVelocity = pSM.ClampPlayerVelocity(pSM.playerVelocity, axis, 0);
        }
        Time.fixedDeltaTime = 0.002f;



        #endregion
    }

    public override IEnumerator Finish()
    {
        swingingFeedback.SetActive(false);
        return base.Finish();

    }
    public PlayerSwinging(PlayerMovementStateMachine playerStateMachine)
    : base(playerStateMachine)
    {

    }




}


