using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PathCreation;

public class PlayerSwinging : State
{
    #region PRIVATE SWINGING
    bool onWall,
        canPress,
        inputGiven,
        inWallLimits,
        firstRound = true;
    bool finishWithNormalJump;

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
        gravityForce = 0f,
        wallLimitsAngle = 10;

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
            return PSM.bob.transform.position -
                PSM.ladder.transform.position;
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

    GameObject pivot,
        ladderParent;
    #endregion

    #region PRIVATE SLIDING
    float dismountTimer;
    float currentSlidingSpeed;
    float maxSlidingSpeed;
    float tAcceleration;
    float tDeceleration;
    float changeDirectionTimer;
    float changeDirectionWaitNotNeededTimer;
    float startSlidingSpeedForDeceleration;
    bool dismountedHalfways;
    bool decelerate;
    bool startedDecelerating;
    bool accelerate;
    bool startedAccelerating;
    bool waitToChangeDirection;
    bool mayChangeDirection;

    Vector3 dismountStartPos;
    Vector3 pathDirection;
    VertexPath path;
    Rail closestRail;
    ValuesScriptableObject stats;
    PathCreator pathCreator;
    PlayerMovementStateMachine pSM;
    LadderSizeStateMachine ladderSizeState;
    CharacterController controller;
    Transform ladder;

    float currentDistance,
        climbingSpeed,
        pathLength;
    bool shouldRetainSwingVelocity = true;
    #endregion

    public override void ReInitialize()
    {
        #region ReInitialize Sliding
        // Assign variables.
        pSM = PSM;
        stats = pSM.stats;

        ladderSizeState = pSM.ladderSizeStateMachine;
        climbingSpeed = stats.climbingSpeedOnLadder;
        closestRail = pSM.closestRail;
        controller = pSM.controller;
        ladder = pSM.ladder;
        pathCreator = closestRail.pathCreator;
        path = pathCreator.path;

        // Place the ladder on the path.
        Vector3 startingPoint = Vector3.zero;
        if (closestRail != null)
        {
            startingPoint = pathCreator.path.GetClosestPointOnPath(pSM.ladder.transform.position);
        }
        else
        {
            Debug.LogError("Shelf is null!");
        }

        currentDistance = path.GetClosestDistanceAlongPath(startingPoint);
        ladder.transform.position = startingPoint;

        pathLength = path.cumulativeLengthAtEachVertex[path.cumulativeLengthAtEachVertex.Length - 1];
        pSM.currentDistance = path.GetClosestDistanceAlongPath(startingPoint);
        #endregion
        #region ReInitialize Swinging
        pSM.bob.transform.position = pSM.ladder.transform.position + -pSM.ladderDirection * stats.ladderLengthBig;
        Rail.RailType oldRailType = railType;
        railType = closestRail.railType;
        if (railType != oldRailType)
        {
            onWall = false;
            inputGiven = false;
        }
        ropeLength = Vector3.Distance(pivot.transform.position, pSM.bob.transform.position);
        #endregion
    }

    public override void Initialize()
    {
        SnappingOrientation();

        #region Set Variables Sliding

        pSM.slidingInput = pSM.startingSlidingInput;
        maxSlidingSpeed = stats.maxSlidingSpeed;
        if (pSM.startingSlidingInput == 0)
        {
            currentSlidingSpeed = 0;
        }
        else
        {
            currentSlidingSpeed = pSM.playerVelocity.magnitude;
            accelerate = true;
        }
        #endregion

        #region Set Variables Swinging
        pivot = pSM.ladder.gameObject; //ist ein gameObject, weil sich der Pivot ja verschiebt, wenn man slidet
        pathLength = path.cumulativeLengthAtEachVertex[path.cumulativeLengthAtEachVertex.Length - 1];
        ladderSizeState = pSM.ladderSizeStateMachine;
        pSM.bob.transform.position = pSM.ladder.transform.position + -pSM.ladderDirection * stats.ladderLengthBig;

        ladderParent = ladderSizeState.ladderParent.gameObject;


        onWall = false;
        inputGiven = false;
        canPress = true;

        // Get the initial rope length from how far away the bob is now
        ropeLength = Vector3.Distance(pivot.transform.position, pSM.bob.transform.position);


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
        #endregion

        pSM.playerVelocity = Vector3.zero;
        pSM.baseVelocity = Vector3.zero;
        pSM.bonusVelocity = Vector3.zero;
    }

    void SnappingOrientation()
    {
        #region  Variable assignment
        pSM = PSM;
        stats = pSM.stats;

        ladderSizeState = pSM.ladderSizeStateMachine;
        closestRail = pSM.closestRail;
        railType = closestRail.railType;
        climbingSpeed = stats.climbingSpeedOnLadder;
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
                currentVelocity += ExtensionMethods.resultingVelocity(pSM.playerVelocity, pSM.bob.transform.forward);
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
            pSM.baseVelocity = ExtensionMethods.resultingClampedVelocity(pSM.baseVelocity, ladder.transform.forward, stats.maxSwingSpeed);
            pSM.bonusVelocity = ExtensionMethods.resultingVelocity(pSM.bonusVelocity, ladder.transform.forward);
        }
        Time.fixedDeltaTime = 0.002f;

        #endregion
    }

    public override void Movement()
    {
        SlidingMovement();
        RotateAroundY();
        Swing();
        RotateAroundY();
    }

    #region SWINGING Functions
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


        Vector3 pivot_p = pivot.transform.position;
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
        Vector3 pivotVectorDown = pivot_p + Vector3.down;
        Vector3 pivotVectorBob = pivot_p + bobPosition;
        float resultingAngle = Vector3.SignedAngle(pivotVectorDown, pivotVectorBob, ladder.right);
        if (Vector3.Dot(currentMovement.normalized, bobForward) >= .93f
           && currentVelocity.magnitude < stats.maxSwingSpeed
           && !inputGiven
           && inputTimer > nextSwingingTime
           || currentVelocity.magnitude <= minSwingSpeed
           && Mathf.Abs(resultingAngle) < 25)
        {
            canPress = true;
            pSM.effects.canSwing = true;
        }
        else
        {
            canPress = false;
            pSM.effects.canSwing = false;
        }
        #endregion
        #region Acceleration & Deceleration
        if (pSM.swingInputBool)
        {
            AccelerationForce();
        }
        inputForce = Vector3.zero;
        inputTimer += dt;

        // set max speed
        currentVelocity = currentVelocity.normalized * Mathf.Clamp(currentVelocity.magnitude, 0, stats.maxSwingSpeed);

        // the higher the velocity, the higher the deceleration Factor
        float DecelerationFactor = (currentVelocity.magnitude) / (stats.maxSwingSpeed) * (maxDecelerationFactor - minDecelerationFactor) + minDecelerationFactor;
        currentVelocity = currentVelocity.normalized * (currentVelocity.magnitude * (1 - DecelerationFactor));
        #endregion
        #region Final Calculations
        // Get only the forward/backward force
        playerVelocity = bobForward * ExtensionMethods.resultingSpeed(currentVelocity, bobForward);
        SetCurrentPlayerVelocity(pivot.transform.position);

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
        Vector3 pivot_p = pivot.transform.position;
        Vector3 bob_p = bobPosition;
        float forwardCheck = Vector3.Dot(currentMovement.normalized, bobForward);
        bool movingForward = forwardCheck >= .93f;

        //Calculate the wallDirection
        float distance = path.GetClosestDistanceAlongPath(pSM.transform.position);
        Vector3 right = pivot_p + pSM.ladder.right.normalized;
        Vector3 forward = pivot_p + path.GetNormalAtDistance(distance);

        Plane wallDirectionPlane = new Plane(pivot_p, right, forward);
        Vector3 wallDirection = -wallDirectionPlane.normal.normalized;
        #endregion
        #region If On Wall

        Vector3 axis = pSM.ladder.right;
        float angle = Vector3.SignedAngle(wallDirection, bob_p.normalized, axis);
        if (Mathf.Abs(angle) <= wallLimitsAngle)
        {
            inWallLimits = true;
        }
        else
        {
            inWallLimits = false;
        }

        if (movingForward && !onWall)
        {

            if (angle <= stats.maxPushAngle)
            {
                onWall = true;
                return GetPointOnLine(Vector3.zero, wallDirection * 100, ropeLength);
            }
        }
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

        #region Acceleration & Final Calculations
        inputForce = Vector3.zero;
        if (pSM.swingInputBool)
        {
            pSM.swingInputBool = false;
            if (!firstRound)
                RepellingForce();
            else
                firstRound = false;
        }

        // set max speed
        currentVelocity = currentVelocity.normalized * Mathf.Clamp(currentVelocity.magnitude, 0, stats.maxSwingSpeed);

        // Get only the forward/backward force
        playerVelocity = bobForward * ExtensionMethods.resultingSpeed(bobForward, currentVelocity);
        SetCurrentPlayerVelocity(pivot.transform.position);

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
        }
    }

    void RepellingForce()
    {
        if (inWallLimits)
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
        float maxJumpSpeed = stats.MaximumMovementSpeed * stats.JumpingDrag;
        float playerHeightOnLadder = (pivot_p - pSM.transform.position).magnitude;

        playerHeightOnLadder = ExtensionMethods.Remap(playerHeightOnLadder, 0, ropeLength, 0.1f, 1);
        maxJumpSpeed = maxJumpSpeed * playerHeightOnLadder;
        currentMovement = playerVelocity.normalized * Mathf.Clamp(playerVelocity.magnitude, 0, maxJumpSpeed);
    }

    private void RotateAroundY()
    {
        Vector3 pathDirection = pathCreator.path.GetDirectionAtDistance(currentDistance, EndOfPathInstruction.Stop);
        float rotateByAngle2 = Vector3.SignedAngle(pSM.ladder.right, pathDirection * pSM.snapdirection, Vector3.up);
        Quaternion targetRotation = Quaternion.AngleAxis(rotateByAngle2, Vector3.up);
        pSM.ladder.rotation = targetRotation * pSM.ladder.rotation;
    }
    #endregion

    public override void Jump()
    {
        if (ladderSizeState.isUnFolding)
        {
            float offSet = .5f;
            Vector3 direction = (-pSM.ladderDirection + Vector3.up * offSet).normalized; ;
            PSM.bonusVelocity = direction * (2.5f * stats.ReversedRailCatapultJumpMultiplier);
            shouldRetainSwingVelocity = false;
            PSM.OnFall();
            pSM.animationControllerisFoldingJumped = true;
        }

        if (ladderSizeState.isFoldingUp)
        {
            float offSet = .5f;
            float heightOnLadderRemapped = (-pSM.HeightOnLadder * stats.heightOnLadderKatapulFactor + 1 - stats.heightOnLadderKatapulFactor);
            Vector3 direction = (pSM.ladderDirection + Vector3.up * offSet).normalized;
            PSM.bonusVelocity = direction * (2.5f * stats.RailCatapultJumpMultiplier) * heightOnLadderRemapped;
            shouldRetainSwingVelocity = false;
            PSM.OnFall();
            pSM.animationControllerisFoldingJumped = true;
        }
        else
        {
            pSM.bonusVelocity += stats.fallingMomentumPercentage * pSM.playerVelocity;
            if (playerVelocity.x == playerVelocity.z && playerVelocity.z == 0)
            {
                finishWithNormalJump = true;
            }

            if (stats.jumpFromLadderDirection != Vector3.zero) //just that it doesn't bug for the others TODO: put it the if statement away, only use wallJump
            {
                Vector3 fromWallVector = (Quaternion.AngleAxis(90, Vector3.up) * pathDirection).normalized;
                fromWallVector = fromWallVector * stats.jumpFromLadderDirection.z;
                Vector3 fromWallValued = new Vector3(fromWallVector.x, stats.jumpFromLadderDirection.y, fromWallVector.z);
                pSM.playerVelocity += fromWallValued;
                pSM.isWallJumping = true;
            }
            else
            {
                PSM.baseVelocity.y += stats.JumpHeight;
            }
            shouldRetainSwingVelocity = true;
            PSM.OnFall();
            pSM.animationControllerisFoldingJumped = false;
        }
        PSM.jumpInputBool = false;
    }

    public override void FallFromLadder()
    {
        PSM.OnFall();
        pSM.animationControllerisFoldingJumped = false;
        PSM.jumpInputBool = false;
    }

    #region SLIDING Functions
    void SlidingMovement()
    {
        ChangeSlidingSpeed();

        if (!pSM.dismounting)
        {
            // Go up and down.
            if (!CheckForCollisionCharacter(pSM.forwardInput * pSM.ladderDirection))
            {
                pSM.HeightOnLadder += pSM.forwardInput * climbingSpeed * Time.fixedDeltaTime;
                pSM.HeightOnLadder = Mathf.Clamp(pSM.HeightOnLadder, -0.75f, 0);
                pSM.transform.localPosition = new Vector3(0, ladderSizeState.ladderLength * pSM.HeightOnLadder, -0.38f);
            }

            #region Move horizontally.
            if (stats.canSlide)
            {
                if (mayChangeDirection)
                {
                    changeDirectionWaitNotNeededTimer += Time.deltaTime;
                    if (changeDirectionWaitNotNeededTimer >= 0.16f)
                    {
                        waitToChangeDirection = false;
                        changeDirectionWaitNotNeededTimer = 0;
                        mayChangeDirection = false;
                    }
                }
                pathDirection = path.GetDirectionAtDistance(currentDistance);
                Vector3 slidingDirection = pathDirection * pSM.slidingInput;

                if (!CheckForCollisionCharacter(slidingDirection) && !CheckForCollisionLadder(slidingDirection))
                {
                    var pressureFactor = pSM.slideRightInput != 0 ? pSM.slideRightInput : pSM.slideLeftInput;
                    float remappedPressureFactor = RemapPressureFactor(pressureFactor);

                    if (waitToChangeDirection)
                    {
                        changeDirectionTimer += Time.deltaTime;
                        if (changeDirectionTimer >= 0.16f)
                        {
                            waitToChangeDirection = false;
                            changeDirectionTimer = 0;
                            mayChangeDirection = false;
                        }
                    }
                    else if (decelerate)
                    {
                        if ((pSM.slideRightInput != 0 && pSM.slideLeftInput != 0))
                        {
                            remappedPressureFactor = 1;
                        }

                        tDeceleration += Time.deltaTime / stats.timeToDecelerate * remappedPressureFactor;
                        currentSlidingSpeed = Mathf.Lerp(startSlidingSpeedForDeceleration, 0, tDeceleration);
                        tAcceleration = 0;
                        if (currentSlidingSpeed == 0)
                        {
                            pSM.slidingInput = 0;
                            tAcceleration = 0;
                            tDeceleration = 0;
                            decelerate = false;
                            startedDecelerating = false;
                            startSlidingSpeedForDeceleration = 0;
                            mayChangeDirection = true;
                        }
                    }
                    else if (accelerate)
                    {
                        tDeceleration = 0;
                        startSlidingSpeedForDeceleration = 0;
                        startedDecelerating = false;
                        var pressureAdjustment = remappedPressureFactor == 1 ? 0f : 0.19f;
                        tAcceleration += Time.deltaTime / stats.timeToAccecelerate * remappedPressureFactor;
                        mayChangeDirection = false;
                        currentSlidingSpeed = Mathf.Lerp(0, maxSlidingSpeed, tAcceleration);
                        if (currentSlidingSpeed == stats.maxSlidingSpeed)
                        {
                            tDeceleration = 0;
                            accelerate = false;
                        }
                    }
                }
                else
                {
                    currentSlidingSpeed = 0;
                }

                pSM.currentDistance += currentSlidingSpeed * pSM.slidingInput * Time.fixedDeltaTime;
                pSM.ladder.position = path.GetPointAtDistance(pSM.currentDistance, EndOfPathInstruction.Stop);
                #endregion

                #region end of Path
                //End Of Path, continue sliding with ReSnap or Fall from Path
                if (pSM.currentDistance <= 0 || pSM.currentDistance >= pathLength)
                {
                    Vector3 endOfShelfDirection = new Vector3();
                    if (pSM.closestRail != null)
                    {
                        if (pSM.currentDistance <= 0) //arriving at start of path
                        {
                            endOfShelfDirection = -pathDirection;
                        }
                        else if (pSM.currentDistance >= pathLength) //arriving at end of path
                        {
                            endOfShelfDirection = pathDirection; //ende - start
                        }
                    }
                    else
                        Debug.Log("There is something bad happening here lmao");
                    Plane railPlane = new Plane(endOfShelfDirection.normalized, Vector3.zero);
                    if (railPlane.GetSide(Vector3.zero + slidingDirection * currentSlidingSpeed)) //player moves in the direction of the end point (move left when going out at start, moves right when going out at end)
                    {
                        if (pSM.CheckForNextClosestRail(pSM.closestRail))
                        {
                            pSM.OnResnap();
                        }
                        else
                        {
                            if (pSM.closestRail.stopSlidingAtTheEnd)
                            {
                                pSM.playerVelocity = ExtensionMethods.ClampPlayerVelocity(pSM.playerVelocity, pathDirection, 0);
                                currentSlidingSpeed = 0;
                                pSM.slidingInput = 0;
                            }
                            else
                            {
                                pSM.coyoteTimer = 0;
                                pSM.bonusVelocity += stats.fallingMomentumPercentage * currentSlidingSpeed * pathDirection * pSM.slidingInput;
                                pSM.OnFall();
                            }
                        }
                    }
                }
            }
            #endregion
            if (railType != Rail.RailType.TwoSided)
                CheckIfReadyToDismount();
        }
        else
        {
            Dismount();
        }
    }

    void ChangeSlidingSpeed()
    {
        var slidingInput = pSM.slidingInput;
        if (pSM.slideLeftInput == 0 && pSM.slideRightInput != 0)
        {
            if (slidingInput == 0 || slidingInput == 1)
            {
                if (!startedAccelerating && currentSlidingSpeed != stats.maxSlidingSpeed)
                {
                    accelerate = true;
                    tDeceleration = 0;
                    decelerate = false;
                    pSM.slidingInput = 1;
                    startedDecelerating = false;
                    startedAccelerating = true;
                    if (slidingInput == 0 && mayChangeDirection)
                    {
                        waitToChangeDirection = true;
                        changeDirectionTimer = 0f;
                    }
                }
            }
            if (slidingInput == -1)
            {
                if (!startedDecelerating)
                {
                    startSlidingSpeedForDeceleration = currentSlidingSpeed;
                    decelerate = true;
                    accelerate = false;
                    startedAccelerating = false;
                    startedDecelerating = true;
                    tAcceleration = 0;
                }
            }
        }
        else if (pSM.slideLeftInput != 0 && pSM.slideRightInput == 0)
        {
            if (slidingInput == 0 || slidingInput == -1)
            {
                if (!startedAccelerating && currentSlidingSpeed != stats.maxSlidingSpeed)
                {
                    accelerate = true;
                    tDeceleration = 0;
                    decelerate = false;
                    pSM.slidingInput = -1;
                    startedAccelerating = true;
                    startedDecelerating = false;
                    if (slidingInput == 0 && mayChangeDirection)
                    {
                        waitToChangeDirection = true;
                        changeDirectionTimer = 0f;
                    }
                }
            }
            if (slidingInput == 1)
            {
                if (!startedDecelerating)
                {
                    tDeceleration = 0;
                    decelerate = true;
                    accelerate = false;
                    tAcceleration = 0;
                    startSlidingSpeedForDeceleration = currentSlidingSpeed;
                    startedAccelerating = false;
                    startedDecelerating = true;
                }
            }
        }
        else if (pSM.slideRightInput != 0 && pSM.slideLeftInput != 0)
        {
            if (!startedDecelerating)
            {
                startedAccelerating = false;
                startedDecelerating = true;
                decelerate = true;
                accelerate = false;
                tAcceleration = 0;
                startSlidingSpeedForDeceleration = currentSlidingSpeed;
            }
        }
        if (pSM.slideRightInput == 0 && pSM.slideLeftInput == 0 && (startedAccelerating || startedDecelerating))
        {
            startedDecelerating = false;
            startedAccelerating = false;
        }
    }

    float RemapPressureFactor(float pressureFactor)
    {
        float remappedPressureFactor = 0;
        if (pressureFactor <= 0.25f)
        {
            remappedPressureFactor = ExtensionMethods.Remap(pressureFactor, 0f, 0.25f, 0f, 0.125f);
        }
        else if (pressureFactor > 0.25f && pressureFactor <= 0.75)
        {
            remappedPressureFactor = ExtensionMethods.Remap(pressureFactor, 0.25f, 0.75f, 0.125f, 0.325f);
        }
        else if (pressureFactor > 0.75f)
        {
            remappedPressureFactor = ExtensionMethods.Remap(pressureFactor, 0.75f, 1f, 0.325f, 1f);
        }
        if (pressureFactor != 0 && remappedPressureFactor == 0)
            Debug.LogError("remapped factor is 0");
        return remappedPressureFactor;
    }

    protected bool CheckForCollisionCharacter(Vector3 moveDirection)
    {
        RaycastHit hit;
        Vector3 p1 = pSM.transform.position + controller.center + Vector3.up * -controller.height / 1.5f;
        Vector3 p2 = p1 + Vector3.up * controller.height;

        if (Physics.CapsuleCast(p1, p2, controller.radius, moveDirection.normalized, out hit, 0.2f, LayerMask.GetMask("SlidingObstacle")))
        {
            return true;
        }
        return false;
    }

    protected bool CheckForCollisionLadder(Vector3 moveDirection)
    {
        RaycastHit hit;
        LadderSizeStateMachine lSM = pSM.ladderSizeStateMachine;
        Vector3 boxExtents = new Vector3(lSM.ladderParent.localScale.x * 0.5f, lSM.ladderParent.localScale.y * 0.5f, lSM.ladderParent.localScale.z * 0.5f);

        if (Physics.BoxCast(pSM.ladder.position, lSM.ladderParent.localScale, moveDirection.normalized, out hit, Quaternion.identity, 0.1f, LayerMask.GetMask("SlidingObstacle")))
        {
            return true;
        }
        return false;
    }

    void CheckIfReadyToDismount()
    {
        // Dismounting the ladder on top and bottom 
        if (pSM.HeightOnLadder == 0 && pSM.forwardInput > 0)
        {
            dismountTimer += Time.fixedDeltaTime;
            RaycastHit hit;
            Vector3 boxExtents = new Vector3(1.540491f * 0.5f, 0.4483852f * 0.5f, 1.37359f * 0.5f);
            if (dismountTimer >= stats.ladderDismountTimer
            && !Physics.BoxCast(controller.transform.position + Vector3.up * 1.5f + controller.transform.forward * -1, boxExtents,
            controller.transform.forward, out hit, controller.transform.rotation, 4f, LayerMask.GetMask("SlidingObstacle", "Environment")))
            {
                if (hit.collider != controller.gameObject)
                {
                    dismountTimer = 0;
                    dismountStartPos = pSM.transform.position;
                    pSM.dismounting = true;
                }
            }
        }
        else if (pSM.HeightOnLadder == -1 && pSM.forwardInput < 0)
        {
        }
        else if (dismountTimer != 0)
        {
            dismountTimer = 0;
        }
    }

    void Dismount()
    {
        // 1 is how much units the player needs to move up to be on top of the rail.
        if ((pSM.transform.position - dismountStartPos).magnitude <= 1 && !dismountedHalfways)
        {
            pSM.HeightOnLadder += stats.ladderDismountSpeed * Time.fixedDeltaTime;
            pSM.transform.position = ladder.transform.position + pSM.ladderDirection * ladderSizeState.ladderLength * pSM.HeightOnLadder;
        }
        else if (!dismountedHalfways)
        {
            dismountStartPos = pSM.transform.position;
            dismountedHalfways = true;
        }

        // Make one step forward on the rail before changing to walking state.
        if ((pSM.transform.position - dismountStartPos).magnitude <= 0.1f && dismountedHalfways)
        {
            pSM.HeightOnLadder += stats.ladderDismountSpeed * Time.fixedDeltaTime;
            pSM.transform.position = ladder.transform.position + pSM.controller.transform.forward * ladderSizeState.ladderLength * pSM.HeightOnLadder;
        }
        else if (dismountedHalfways)
        {
            pSM.dismounting = false;
            pSM.OnLadderTop();
        }
    }
    #endregion

    public override IEnumerator Finish()
    {
        #region Finish Swinging
        SetCurrentPlayerVelocity(pivot.transform.position);
        if (!finishWithNormalJump)
        {
            if (shouldRetainSwingVelocity)
            {
                pSM.bonusVelocity += (currentMovement + Vector3.up * 1.1f).normalized * currentMovement.magnitude;
                pSM.baseVelocity = pSM.baseVelocity.normalized * Mathf.Clamp(pSM.baseVelocity.magnitude, 0, stats.MaximumMovementSpeed);
            }
            else
            {
                pSM.baseVelocity.y = 0;
            }
        }
        pSM.snapInputBool = false;
        pSM.startingSlidingInput = 0;
        #endregion
        #region Finish Sliding
        pSM.closestRail = null;
        Time.fixedDeltaTime = 0.02f;
        pSM.invertedSliding = false;
        #endregion

        yield break;
    }

    public PlayerSwinging(PlayerMovementStateMachine playerStateMachine)
    : base(playerStateMachine)
    {

    }
}


