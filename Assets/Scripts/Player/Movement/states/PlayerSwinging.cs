using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PlayerSwinging : PlayerSliding
{
    #region Private

    bool onWall,
        canPress,
        inputGiven;

    float dt = 0.01f,
        accumulator = 0f,
        minSwingSpeed = 8f,
        nextSwingingTime = 1f,
        ropeLength = 2f,
        mass = 1f,
        accelerationFactor,
        minDecelerationFactor,
        maxDecelerationFactor,
        inputTimer,
        tensionForce = 0f,
        gravityForce = 0f;

    Vector3 repelDirection,
        currentStatePosition,
        previousStatePosition,
        playerVelocity,
        inputForce,
        gravityDirection,
        tensionDirection,
        currentMovement = Vector3.zero,
        currentVelocity = new Vector3();
    private Vector3 bobPosition
    {
        get
        {
            return PlayerStateMachine.bob.transform.position -
                PlayerStateMachine.ladder.transform.position;
        }
    }
    private Vector3 bobForward
    {
        get
        {
            return pSM.bob.transform.forward;
        }
    }

    Rail.RailType railType;

    GameObject Pivot,
        swingingFeedback,
        ladderParent;

    #endregion

    public override void ReInitialize()
    {
        base.ReInitialize();
        pSM.bob.transform.position = pSM.ladder.transform.position + -pSM.ladderDirection * ladderSizeState.ladderLengthBig;
        Rail.RailType oldRailType = railType;
        railType = closestRail.railType;
        if (railType != oldRailType)
        {
            onWall = false;
            inputGiven = false;
        }
        ropeLength = Vector3.Distance(Pivot.transform.position, pSM.bob.transform.position);

    }


    public override void Initialize()
    {
        SnappingOrientation();

        // Input Callbacks
        if (stats.useNewSliding)
        {
            pSM.slidingInput = pSM.startingSlidingInput;
            pSM.slideLeftAction.started += context =>
            {
                if (pSM.slidingInput * pSM.adjustedSlideDirection == 1)
                {
                    startLeftHoldTimer = true;
                }
                holdingChangeDirection = false;
                leftHoldTimer = 0;
                holdingLeftSlideButton = true;
            };
            pSM.slideRightAction.started += context =>
            {
                if (pSM.slidingInput * pSM.adjustedSlideDirection == -1)
                {
                    startRightHoldTimer = true;
                }
                holdingChangeDirection = false;
                rightHoldTimer = 0;
                holdingRightSlideButton = true;
            };
            pSM.slideLeftAction.canceled += context =>
            {
                holdingLeftSlideButton = false;
                startLeftHoldTimer = false;
                if (pSM.slidingInput * pSM.adjustedSlideDirection == 1 && holdingChangeDirection == false)
                {
                    SwitchSpeedLevel("left");
                }
            };
            pSM.slideRightAction.canceled += context =>
            {
                holdingRightSlideButton = false;
                startRightHoldTimer = false;
                if (pSM.slidingInput * pSM.adjustedSlideDirection == -1 && holdingChangeDirection == false)
                {
                    SwitchSpeedLevel("right");
                }
            };
            pSM.slideLeftAction.performed += context =>
            {
                if (pSM.slidingInput * pSM.adjustedSlideDirection != 1)
                {
                    SwitchSpeedLevel("left");
                    holdingChangeDirection = true;
                }
            };
            pSM.slideRightAction.performed += context =>
            {
                if (pSM.slidingInput * pSM.adjustedSlideDirection != -1)
                {
                    SwitchSpeedLevel("right");
                    holdingChangeDirection = true;
                }
            };
            if (pSM.startingSlidingInput == 0)
            {
                currentSlidingLevel = 0;
                currentSlidingLevelSpeed = 0;
            }
            else
            {
                holdingChangeDirection = true;
                float resultingSpeed = pSM.playerVelocity.magnitude;

                float tempSpeed = 100;
                int closestSpeedLevel = 1;
                for (int i = 2; i < stats.speedLevels.Count; i++)
                {
                    float newTempSpeed = Mathf.Abs(stats.speedLevels[i] - resultingSpeed);
                    if (tempSpeed > stats.speedLevels[i])
                    {
                        tempSpeed = newTempSpeed;
                        closestSpeedLevel = i;
                    }
                }
                currentSlidingLevel = closestSpeedLevel;
                currentSlidingLevelSpeed = stats.speedLevels[currentSlidingLevel];
            }
        }
        else
        {
            pSM.stopSlidingAction.started += context => stopping = true;
            pSM.stopSlidingAction.canceled += context => stopping = false;
        }

        Pivot = pSM.ladder.gameObject; //ist ein gameObject, weil sich der Pivot ja verschiebt, wenn man slidet
        pathLength = path.cumulativeLengthAtEachVertex[path.cumulativeLengthAtEachVertex.Length - 1];
        ladderSizeState = pSM.ladderSizeStateMachine;
        pSM.bob.transform.position = pSM.ladder.transform.position + -pSM.ladderDirection * ladderSizeState.ladderLengthBig;

        ladderParent = ladderSizeState.ladderParent.gameObject;
        swingingFeedback = ladderParent.transform.GetChild(5).gameObject;


        onWall = false;
        inputGiven = false;
        canPress = true;

        // Get the initial rope length from how far away the bob is now
        ropeLength = Vector3.Distance(Pivot.transform.position, pSM.bob.transform.position);


        currentVelocity = Vector3.zero;
        currentStatePosition = bobPosition;
        previousStatePosition = bobPosition;

        switch (railType)
        {
            case Rail.RailType.TwoSided:
                minDecelerationFactor = stats.minSwingingDeceleration;
                maxDecelerationFactor = stats.maxSwingingDeceleration;
                accelerationFactor = 1;
                break;
            case Rail.RailType.FreeHanging:
                minDecelerationFactor = stats.minHangingDeceleration;
                maxDecelerationFactor = stats.maxHangingDeceleration;
                accelerationFactor = stats.hangingAccelerationFactor;
                break;
            case Rail.RailType.OnWall:
                break;
        }

        pSM.playerVelocity = Vector3.zero;
        pSM.baseVelocity = Vector3.zero;
        pSM.bonusVelocity = Vector3.zero;
    }

    public override void Movement()
    {
        base.Movement();
        RotateAorundY();
        Swing();
        RotateAorundY();
    }



    public override void Swing()
    {
        #region FixedUpdate
        float frameTime = Time.fixedDeltaTime;
        accumulator += frameTime;

        // fixed update
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
        #endregion
        #region SwingingRotation
        float alpha = accumulator / dt;
        Vector3 newPosition = currentStatePosition * alpha + previousStatePosition * (1f - alpha);

        //die Leiter korrekt rotieren
        currentDistance = pSM.currentDistance;

        Vector3 axis = pSM.ladder.right;
        float rotateByAngle = (Vector3.SignedAngle(-pSM.ladderDirection, newPosition, axis));
        Quaternion targetRotation = Quaternion.AngleAxis(rotateByAngle, axis);
        pSM.ladder.rotation = targetRotation * pSM.ladder.rotation;
        #endregion
    }

    Vector3 PendulumUpdate()
    {
        #region Gravity & Tension
        gravityForce = mass * stats.swingingGravity;
        gravityDirection = Physics.gravity.normalized;
        currentVelocity += gravityDirection * gravityForce * dt;


        Vector3 pivot_p = Pivot.transform.position;
        Vector3 bob_p = bobPosition;

        tensionDirection = (-bob_p).normalized;
        float inclinationAngle = Vector3.Angle(bob_p, gravityDirection);


        tensionForce = gravityForce * Mathf.Cos(Mathf.Deg2Rad * inclinationAngle);
        float centripetalForce = mass * Mathf.Pow(playerVelocity.magnitude, 2) / ropeLength;
        tensionForce += centripetalForce;

        float relativeHeight = (bob_p).normalized.y;
        if (relativeHeight > 0)
        {
            tensionForce *= ExtensionMethods.Remap(relativeHeight, 0, 1, 0.9f, 0.1f);
        }

        currentVelocity += tensionDirection * tensionForce * dt;
        #endregion
        #region ResetInput
        // Check for Direction Change
        Vector3 currentNormal = -path.GetNormalAtDistance(currentDistance);
        Plane forward = new Plane(currentNormal, pivot_p);

        if (inputGiven && !forward.GetSide(pSM.bob.transform.position))
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
        #endregion
        #region Acceleration
        if (pSM.swingInputBool)
        {
            AccelerationForce();
        }
        inputForce = Vector3.zero;
        inputTimer += dt;

        // set max speed
        currentVelocity = currentVelocity.normalized * Mathf.Clamp(currentVelocity.magnitude, 0, stats.maxSwingSpeed);
        #endregion
        #region Deceleration
        // the higher the velocity, the higher the deceleration Factor
        float DecelerationFactor = (currentVelocity.magnitude) / (stats.maxSwingSpeed) * (maxDecelerationFactor - minDecelerationFactor) + minDecelerationFactor;
        currentVelocity = currentVelocity.normalized * (currentVelocity.magnitude * (1 - DecelerationFactor));
        #endregion
        #region FinalCalculation
        // Get only the forward/backward force
        playerVelocity = bobForward * pSM.resultingSpeed(currentVelocity, bobForward);
        SetCurrentPlayerVelocity(Pivot.transform.position);

        // Get the movement delta
        Vector3 movementDelta = Vector3.zero;
        movementDelta += playerVelocity * dt;

        return GetPointOnLine(Vector3.zero, bobPosition + movementDelta, ropeLength);
        #endregion
    }
    Vector3 RepelUpdate()
    {
        #region SetVariables
        // Get normal at current position
        repelDirection = -bobForward;
        Vector3 pivot_p = Pivot.transform.position;
        Vector3 bob_p = bobPosition;
        bool movingForward = Vector3.Dot(currentMovement.normalized, bobForward) >= .93f;

        //Calculate the wallDirection
        float distance = path.GetClosestDistanceAlongPath(pSM.transform.position);
        Vector3 right = pivot_p + pSM.ladder.right.normalized;
        Vector3 forward = pivot_p + path.GetNormalAtDistance(distance);

        Plane wallDirectionPlane = new Plane(pivot_p, right, forward);
        Vector3 wallDirection = -wallDirectionPlane.normal.normalized;
        #endregion
        #region Check If On Wall
        if (movingForward && !onWall)
        {
            Vector3 axis = pSM.ladder.right;

            float angle = Vector3.SignedAngle(wallDirection, (bob_p).normalized, axis);

            if (angle <= stats.maxPushAngle)
            {
                onWall = true;
                return GetPointOnLine(Vector3.zero, wallDirection * 100, ropeLength);
            }
        }
        #endregion
        #region On Wall
        if (onWall)
        {
            currentVelocity = Vector3.zero;
            bob_p = wallDirection * ropeLength;
            SetCurrentPlayerVelocity(pivot_p);
        }
        #endregion
        #region Swinging
        else
        {
            // Erstellt eine Gravity und addiert sie auf die currentVelocity
            gravityForce = mass * stats.swingingGravity;
            gravityDirection = Physics.gravity.normalized;
            currentVelocity += gravityDirection * gravityForce * dt;

            tensionDirection = (-bob_p).normalized;
            float inclinationAngle = Vector3.Angle(bob_p, gravityDirection);

            tensionForce = mass * stats.swingingGravity * Mathf.Cos(Mathf.Deg2Rad * inclinationAngle);
            float centripetalForce = ((mass * Mathf.Pow(currentVelocity.magnitude, 2)) / ropeLength);

            tensionForce += centripetalForce;
            currentVelocity += tensionDirection * tensionForce * dt;
        }
        #endregion
        #region Acceleration
        inputForce = Vector3.zero;
        if (pSM.swingInputBool)
        {
            RepellingForce();
        }
        // set max speed
        currentVelocity = currentVelocity.normalized * Mathf.Clamp(currentVelocity.magnitude, 0, stats.maxSwingSpeed);
        #endregion
        #region Final Calculations
        // Get only the forward/backward force
        playerVelocity = bobForward * pSM.resultingSpeed(bobForward, currentVelocity);
        SetCurrentPlayerVelocity(Pivot.transform.position);

        // Get the movement delta
        Vector3 movementDelta = Vector3.zero;
        movementDelta += playerVelocity * dt;
        return GetPointOnLine(Vector3.zero, bob_p + movementDelta, ropeLength);
        #endregion
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

            pSM.swingInputBool = false;
        }
    }

    void RepellingForce()
    {
        if (onWall)
        {
            onWall = false;
            inputForce = repelDirection * stats.swingingAcceleration * dt * 1.2f;
            currentVelocity += inputForce;
            pSM.swingInputBool = false;
        }
    }

    void SetCurrentPlayerVelocity(Vector3 pivot_p)
    {
        // Set currentMovement Force
        float maxJumpSpeed = stats.maximumMovementSpeed * stats.jumpingDrag;
        float playerHeightOnLadder = (pivot_p - pSM.transform.position).magnitude;

        playerHeightOnLadder = ExtensionMethods.Remap(playerHeightOnLadder, 0, ropeLength, 0.1f, 1);
        maxJumpSpeed = maxJumpSpeed * playerHeightOnLadder;
        currentMovement = playerVelocity.normalized * Mathf.Clamp(playerVelocity.magnitude, 0, maxJumpSpeed);
    }

    private void RotateAorundY()
    {
        Vector3 pathDirection = pathCreator.path.GetDirectionAtDistance(currentDistance, EndOfPathInstruction.Stop);
        float rotateByAngle2 = Vector3.SignedAngle(pSM.ladder.right, pathDirection * pSM.snapdirection, Vector3.up);
        Quaternion targetRotation = Quaternion.AngleAxis(rotateByAngle2, Vector3.up);
        pSM.ladder.rotation = targetRotation * pSM.ladder.rotation;
    }

    void SnappingOrientation()
    {
        #region  Variable assignment
        pSM = PlayerStateMachine;
        stats = pSM.stats;

        ladderSizeState = pSM.ladderSizeStateMachine;
        closestRail = pSM.closestRail;
        railType = closestRail.railType;
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
        Vector3 startingNormal = path.GetNormalAtDistance(currentDistance);



        //evtl. fuer spaeter noch wichtig wenn ich nochmal versuche das ganze velocity base zu machen
        if (railType == Rail.RailType.TwoSided && pSM.playerVelocity.magnitude >= stats.minVelocityToChangeSnapDirection)
        {
            if (Vector3.Dot(pSM.playerVelocity.normalized, startingNormal) < 0)
            {
                ladder.transform.forward = -startingNormal;
                pSM.snapdirection = 1;
            }
            else
            {
                ladder.transform.forward = startingNormal;
                pSM.snapdirection = -1;
            }
        }
        else if (railType == Rail.RailType.TwoSided && Vector3.Dot(startingPoint - pSM.transform.position, startingNormal) >= 0)
        {
            ladder.transform.forward = startingNormal;
            pSM.snapdirection = -1;
        }
        else
        {
            ladder.transform.forward = -startingNormal;
            pSM.snapdirection = 1;
        }

        pSM.currentDistance = currentDistance;
        ladder.transform.SetParent(pSM.myParent);
        ladder.transform.localScale = new Vector3(1, 1, 1);
        controller.transform.localScale = new Vector3(1, 1, 1);

        //Ladder Rotation
        Vector3 axis = pSM.ladder.right;
        float rotateByAngle = (Vector3.SignedAngle(-pSM.ladderDirection, pSM.transform.position - startingPoint, axis));
        if (railType != Rail.RailType.TwoSided)
        {
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
        pSM.transform.position = ladder.transform.position + pSM.ladderDirection * ladderSizeState.ladderLength * pSM.HeightOnLadder + ladder.transform.forward * -stats.playerOffsetFromLadder;
        controller.transform.localRotation = Quaternion.Euler(0, 0, 0);
        #endregion
        #region Velocity Calculation

        if (!stats.preservesVelocityOnSnap)
        {

            pSM.baseVelocity = pSM.resultingClampedVelocity(pSM.baseVelocity, ladder.transform.forward, stats.maxSwingSpeed);
            pSM.bonusVelocity = pSM.resultingVelocity(pSM.bonusVelocity, ladder.transform.forward);

        }
        Time.fixedDeltaTime = 0.002f;

        #endregion
    }

    public override IEnumerator Finish()
    {
        SetCurrentPlayerVelocity(Pivot.transform.position);
        if (shouldRetainSwingVelocity)
        {
            pSM.bonusVelocity += (currentMovement + Vector3.up * 1.1f).normalized * (currentMovement.magnitude * stats.swingingVelocityFactor);
            pSM.baseVelocity = pSM.baseVelocity.normalized * Mathf.Clamp(pSM.baseVelocity.magnitude, 0, stats.maximumMovementSpeed);
        }
        else
        {
            pSM.baseVelocity.y = 0;
        }
        swingingFeedback.SetActive(false);
        pSM.snapInputBool = false;

        return base.Finish();
    }
    public PlayerSwinging(PlayerMovementStateMachine playerStateMachine)
    : base(playerStateMachine)
    {

    }
}


