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
    bool stoppedSwingingToDismount;

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
            if (PSM.useRelativeBobPosition)
            {
                return base.PSM.bob.transform.position -
                    base.PSM.ladder.transform.position;
            }
            return PSM.bob.transform.position;
        }
    }

    private Vector3 bobForward
    {
        get
        {
            return PSM.bob.transform.forward;
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
    LadderSizeStateMachine ladderSizeState;
    CharacterController controller;
    Transform ladder;

    float climbingSpeed,
        pathLength;
    bool shouldRetainSwingVelocity = true;
    #endregion

    public override void ReInitialize()
    {
        #region ReInitialize Sliding
        // Assign variables.
        stats = PSM.stats;

        ladderSizeState = PSM.ladderSizeStateMachine;
        climbingSpeed = stats.climbingSpeedOnLadder;
        closestRail = PSM.closestRail;
        controller = PSM.controller;
        ladder = PSM.ladder;
        pathCreator = closestRail.pathCreator;
        path = pathCreator.path;

        // Place the ladder on the path.
        Vector3 startingPoint = Vector3.zero;
        if (closestRail != null)
        {
            startingPoint = pathCreator.path.GetClosestPointOnPath(PSM.ladder.transform.position);
        }
        else
        {
            Debug.LogError("Shelf is null!");
        }

        ladder.transform.position = startingPoint;

        pathLength = path.cumulativeLengthAtEachVertex[path.cumulativeLengthAtEachVertex.Length - 1];
        PSM.currentDistance = path.GetClosestDistanceAlongPath(startingPoint);
        #endregion
        #region ReInitialize Swinging
        PSM.bob.transform.position = PSM.ladder.transform.position + -PSM.ladderDirection * stats.ladderLengthBig;
        Rail.RailType oldRailType = railType;
        railType = closestRail.railType;
        if (railType != oldRailType)
        {
            onWall = false;
            inputGiven = false;
        }
        ropeLength = Vector3.Distance(pivot.transform.position, PSM.bob.transform.position);
        #endregion
    }

    public override void Initialize()
    {
        SnappingOrientation();

        #region Set Variables Sliding

        PSM.slidingInput = PSM.startingSlidingInput;
        maxSlidingSpeed = stats.maxSlidingSpeed;
        if (PSM.startingSlidingInput == 0)
        {
            currentSlidingSpeed = 0;
        }
        else
        {
            currentSlidingSpeed = PSM.playerVelocity.magnitude;
            accelerate = true;
        }
        #endregion

        #region Set Variables Swinging
        if (PSM.useRelativeBobPosition)
        {
            pivot = PSM.ladder.gameObject;
        }
        else
        {
            pivot = PSM.Bob_Pivot.gameObject;
        }//ist ein gameObject, weil sich der Pivot ja verschiebt, wenn man slidet
        pathLength = path.cumulativeLengthAtEachVertex[path.cumulativeLengthAtEachVertex.Length - 1];
        ladderSizeState = PSM.ladderSizeStateMachine;
        if (PSM.useRelativeBobPosition)
        {
            PSM.bob.transform.position = PSM.ladder.transform.position + -PSM.ladderDirection * stats.ladderLengthBig;
        }
        else
        {
            PSM.bob.transform.position = PSM.Bob_Pivot.transform.position + -PSM.ladderDirection * stats.ladderLengthBig;
        }

        ladderParent = ladderSizeState.ladderParent.gameObject;

        onWall = false;
        inputGiven = false;
        canPress = true;

        // Get the initial rope length from how far away the bob is now
        ropeLength = Vector3.Distance(pivot.transform.position, PSM.bob.transform.position);


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

        PSM.playerVelocity = Vector3.zero;
        PSM.baseVelocity = Vector3.zero;
        PSM.bonusVelocity = Vector3.zero;
    }

    void SnappingOrientation()
    {
        #region  Variable assignment
        PSM = base.PSM;
        stats = PSM.stats;

        ladderSizeState = PSM.ladderSizeStateMachine;
        closestRail = PSM.closestRail;
        railType = closestRail.railType;
        climbingSpeed = stats.climbingSpeedOnLadder;
        controller = PSM.controller;
        ladder = PSM.ladder;
        pathCreator = closestRail.pathCreator;
        path = pathCreator.path;

        #endregion
        #region LadderPlacement
        Vector3 startingPoint = pathCreator.path.GetClosestPointOnPath(PSM.transform.position);

        ladder.transform.position = startingPoint;
        Vector3 startingNormal = path.GetNormalAtDistance(PSM.currentDistance);
        Vector3 cameraDirection = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;

        //decision of which side to snap to based on camera, velocity and position

        if (railType == Rail.RailType.TwoSided)
        {
            //look for Camera direction
            if (Mathf.Abs(Vector3.Dot(cameraDirection, startingNormal)) > stats.minCameraAngleToChangeSnapDirection)
            {
                if (Vector3.Dot(cameraDirection, startingNormal) < 0)
                {
                    ladder.transform.forward = -startingNormal;
                    PSM.snapdirection = 1;
                }
                else
                {
                    ladder.transform.forward = startingNormal;
                    PSM.snapdirection = -1;
                }

            }
            //look for velocity
            else if (PSM.playerVelocity.magnitude >= stats.minVelocityToChangeSnapDirection)
            {
                if (Vector3.Dot(PSM.playerVelocity.normalized, startingNormal) < 0)
                {
                    ladder.transform.forward = -startingNormal;
                    PSM.snapdirection = 1;
                }
                else
                {
                    ladder.transform.forward = startingNormal;
                    PSM.snapdirection = -1;
                }
            }
            //look for position
            else if (Vector3.Dot(startingPoint - PSM.transform.position, startingNormal) >= 0)
            {
                ladder.transform.forward = startingNormal;
                PSM.snapdirection = -1;
            }
            else
            {
                ladder.transform.forward = -startingNormal;
                PSM.snapdirection = 1;
            }
        }
        else
        {
            ladder.transform.forward = -startingNormal;
            PSM.snapdirection = 1;
        }

        PSM.currentDistance = path.GetClosestDistanceAlongPath(startingPoint);
        ladder.transform.SetParent(PSM.myParent);
        ladder.transform.localScale = new Vector3(1, 1, 1);
        controller.transform.localScale = new Vector3(1, 1, 1);

        //Ladder Rotation
        Vector3 axis = PSM.ladder.right;
        float rotateByAngle;
        rotateByAngle = (Vector3.SignedAngle(-PSM.ladderDirection, PSM.transform.position - startingPoint, axis));

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
                currentVelocity += ExtensionMethods.resultingVelocity(PSM.playerVelocity, PSM.bob.transform.forward);
                currentVelocity = Vector3.ClampMagnitude(currentVelocity, stats.maxSwingSpeed);
            }
        }
        Quaternion targetRotation = Quaternion.AngleAxis(rotateByAngle, axis);
        PSM.ladder.rotation = targetRotation * PSM.ladder.rotation;

        //LadderLength Calculation
        PSM.ladderSizeStateMachine.ladderLength = Vector3.Distance(PSM.transform.position, startingPoint);
        PSM.ladderSizeStateMachine.OnSnap();

        #endregion
        #region PlayerPlacement
        PSM.HeightOnLadder = -1;
        controller.transform.parent = ladder.transform;
        PSM.transform.position = ladder.transform.position + PSM.ladderDirection * ladderSizeState.ladderLength * PSM.HeightOnLadder + ladder.transform.forward * -stats.playerOffsetFromLadder;
        controller.transform.localRotation = Quaternion.Euler(0, 0, 0);
        #endregion
        #region Velocity Calculation

        if (!stats.preservesVelocityOnSnap)
        {
            PSM.baseVelocity = ExtensionMethods.resultingClampedVelocity(PSM.baseVelocity, ladder.transform.forward, stats.maxSwingSpeed);
            PSM.bonusVelocity = ExtensionMethods.resultingVelocity(PSM.bonusVelocity, ladder.transform.forward);
        }
        Time.fixedDeltaTime = 0.002f;

        #endregion
    }

    public override void Movement()
    {
        SlidingMovement();
        RotateAroundY();
        Swing();
        //RotateAroundY();
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

        Vector3 axis = PSM.ladder.right;
        float rotateByAngle = (Vector3.SignedAngle(-PSM.ladderDirection, newPosition, axis));
        Quaternion targetRotation = Quaternion.AngleAxis(rotateByAngle, axis);
        PSM.ladder.rotation = targetRotation * PSM.ladder.rotation;
        if (!PSM.useRelativeBobPosition)
        {
            PSM.Bob_Pivot.rotation = targetRotation * PSM.Bob_Pivot.rotation;
        }
        #endregion
    }

    Vector3 PendulumUpdate()
    {
        #region Gravity & Tension
        gravityForce = mass * stats.swingingGravity;
        gravityDirection = Physics.gravity.normalized;
        currentVelocity += gravityDirection * gravityForce * dt;
        Vector3 pivot_p;
        if (PSM.useRelativeBobPosition)
        {
            pivot_p = PSM.Bob_Pivot.transform.position;
        }
        else
        {
            pivot_p = ladder.transform.position;
        }
        Vector3 bob_p = bobPosition;

        tensionDirection = (-bob_p).normalized;
        float inclinationAngle = Vector3.Angle(bob_p, gravityDirection);


        tensionForce = gravityForce * Mathf.Cos(Mathf.Deg2Rad * inclinationAngle);
        float centripetalForce = mass * Mathf.Pow(playerVelocity.magnitude, 2) / ropeLength;
        tensionForce += centripetalForce;

        float relativeHeight;
        if (PSM.useRelativeBobPosition)
        {
            relativeHeight = (bob_p).normalized.y;
        }
        else
        {
            relativeHeight = (bob_p - pivot_p).normalized.y;
        }
        if (relativeHeight > 0)
        {
            tensionForce *= ExtensionMethods.Remap(relativeHeight, 0, 1, 0.9f, 0.1f);
        }

        currentVelocity += tensionDirection * tensionForce * dt;
        #endregion
        #region ResetInput
        // Check for Direction Change
        Vector3 currentNormal = -path.GetNormalAtDistance(base.PSM.currentDistance);
        Plane forward = new Plane(currentNormal, pivot_p);

        if (inputGiven && !forward.GetSide(PSM.bob.transform.position))
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
            PSM.effects.canSwing = true;
        }
        else
        {
            canPress = false;
            PSM.effects.canSwing = false;
        }
        #endregion
        #region Acceleration & Deceleration
        if (PSM.swingInputBool)
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
        SetCurrentPlayerVelocity(ladder.transform.position);

        // Get the movement delta
        Vector3 movementDelta = Vector3.zero;
        movementDelta += playerVelocity * dt;
        if (PSM.useRelativeBobPosition)
        {
            return GetPointOnLine(Vector3.zero, bobPosition + movementDelta, ropeLength);
        }
        else
        {
            return GetPointOnLine(pivot_p, bobPosition + movementDelta, ropeLength);
        }
        #endregion
    }
    Vector3 RepelUpdate()
    {
        #region SetVariables
        // Get normal at current position
        repelDirection = -bobForward;
        Vector3 pivot_p;
        if (PSM.useRelativeBobPosition)
        {
            pivot_p = PSM.Bob_Pivot.transform.position;
        }
        else
        {
            pivot_p = ladder.transform.position;
        }
        Vector3 bob_p = bobPosition;
        float forwardCheck = Vector3.Dot(currentMovement.normalized, bobForward);
        bool movingForward = forwardCheck >= .93f;

        //Calculate the wallDirection
        float distance = path.GetClosestDistanceAlongPath(PSM.transform.position);
        Vector3 right = pivot_p + PSM.ladder.right.normalized;
        Vector3 forward = pivot_p + path.GetNormalAtDistance(distance);

        Plane wallDirectionPlane = new Plane(pivot_p, right, forward);
        Vector3 wallDirection = -wallDirectionPlane.normal.normalized;
        #endregion
        #region If On Wall

        Vector3 axis = PSM.ladder.right;
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
            SetCurrentPlayerVelocity(ladder.transform.position);
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
            if (PSM.dismounting && !stoppedSwingingToDismount)
            {
                currentVelocity = Vector3.zero;
                stoppedSwingingToDismount = true;
            }
        }
        #endregion

        #region Acceleration & Final Calculations
        inputForce = Vector3.zero;
        if (!PSM.dismounting)
        {
            if (PSM.swingInputBool)
            {
                PSM.swingInputBool = false;
                if (!firstRound)
                    RepellingForce();
                else
                    firstRound = false;
            }
            currentVelocity = currentVelocity.normalized * Mathf.Clamp(currentVelocity.magnitude, 0, stats.maxSwingSpeed);
        }
        else
        {
            currentVelocity = currentVelocity.normalized * 100;
        }

        // Get only the forward/backward force
        playerVelocity = bobForward * ExtensionMethods.resultingSpeed(bobForward, currentVelocity);
        SetCurrentPlayerVelocity(ladder.transform.position);

        // Get the movement delta
        Vector3 movementDelta = Vector3.zero;
        movementDelta += playerVelocity * dt;
        if (PSM.useRelativeBobPosition)
        {
            return GetPointOnLine(Vector3.zero, bob_p + movementDelta, ropeLength);
        }
        else
        {
            return GetPointOnLine(pivot_p, bob_p + movementDelta, ropeLength);
        }
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
            PSM.swingInputBool = false;
        }
    }

    void SetCurrentPlayerVelocity(Vector3 pivot_p)
    {
        // Set currentMovement Force
        float maxJumpSpeed = stats.MaximumMovementSpeed * stats.JumpingDrag;
        float playerHeightOnLadder = (pivot_p - PSM.transform.position).magnitude;

        playerHeightOnLadder = ExtensionMethods.Remap(playerHeightOnLadder, 0, ropeLength, 0.1f, 1);
        maxJumpSpeed = maxJumpSpeed * playerHeightOnLadder;
        currentMovement = playerVelocity.normalized * Mathf.Clamp(playerVelocity.magnitude, 0, maxJumpSpeed);
    }

    private void RotateAroundY()
    {
        Vector3 localUp = Vector3.up;
        Vector3 pathDirection = pathCreator.path.GetDirectionAtDistance(PSM.currentDistance, EndOfPathInstruction.Stop);
        Vector3 HorizontalRailDirection = new Vector3(pathDirection.x, 0, pathDirection.z);
        float rotateByAngle2 = Vector3.SignedAngle(PSM.ladder.right, HorizontalRailDirection * PSM.snapdirection, localUp);
        Quaternion targetRotation = Quaternion.AngleAxis(rotateByAngle2, localUp);
        PSM.ladder.rotation = targetRotation * PSM.ladder.rotation;
    }
    #endregion

    public override void Jump()
    {
        if (ladderSizeState.isUnFolding)
        {
            float offSet = .5f;
            Vector3 direction = (-PSM.ladderDirection + Vector3.up * offSet).normalized; ;
            base.PSM.bonusVelocity = direction * (2.5f * stats.ReversedRailCatapultJumpMultiplier);
            shouldRetainSwingVelocity = false;
            base.PSM.OnFall();
            PSM.animationControllerisFoldingJumped = true;
        }

        if (ladderSizeState.isFoldingUp)
        {
            float offSet = .5f;
            float heightOnLadderRemapped = (-PSM.HeightOnLadder * stats.heightOnLadderKatapulFactor + 1 - stats.heightOnLadderKatapulFactor);
            Vector3 direction = (PSM.ladderDirection + Vector3.up * offSet).normalized;
            base.PSM.bonusVelocity = direction * (2.5f * stats.RailCatapultJumpMultiplier) * heightOnLadderRemapped;
            shouldRetainSwingVelocity = false;
            base.PSM.OnFall();
            PSM.animationControllerisFoldingJumped = true;
        }
        else
        {
            PSM.bonusVelocity += stats.fallingMomentumPercentage * PSM.playerVelocity;
            if (playerVelocity.x == playerVelocity.z && playerVelocity.z == 0)
            {
                finishWithNormalJump = true;
            }

            if (stats.jumpFromLadderDirection != Vector3.zero) //just that it doesn't bug for the others TODO: put it the if statement away, only use wallJump
            {
                Vector3 fromWallVector = (Quaternion.AngleAxis(90, Vector3.up) * pathDirection).normalized;
                fromWallVector = fromWallVector * stats.jumpFromLadderDirection.z;
                Vector3 fromWallValued = new Vector3(fromWallVector.x, stats.jumpFromLadderDirection.y, fromWallVector.z);
                PSM.playerVelocity += fromWallValued;
                PSM.isWallJumping = true;
            }
            else
            {
                base.PSM.baseVelocity.y += stats.JumpHeight;
            }
            shouldRetainSwingVelocity = true;
            base.PSM.OnFall();
            PSM.animationControllerisFoldingJumped = false;
        }
        base.PSM.jumpInputBool = false;
    }

    public override void FallFromLadder()
    {
        base.PSM.OnFall();
        PSM.animationControllerisFoldingJumped = false;
        base.PSM.jumpInputBool = false;
    }

    #region SLIDING Functions
    void SlidingMovement()
    {
        ChangeSlidingSpeed();

        if (!PSM.dismounting)
        {
            // Go up and down.
            if (!CheckForCollisionCharacter(PSM.forwardInput * PSM.ladderDirection))
            {
                PSM.HeightOnLadder += PSM.forwardInput * climbingSpeed * Time.fixedDeltaTime;
                PSM.HeightOnLadder = Mathf.Clamp(PSM.HeightOnLadder, -0.75f, 0);
                PSM.transform.localPosition = new Vector3(0, ladderSizeState.ladderLength * PSM.HeightOnLadder, -0.38f);
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
                pathDirection = path.GetDirectionAtDistance(PSM.currentDistance);
                Vector3 slidingDirection = pathDirection * PSM.slidingInput;

                if (!CheckForCollisionCharacter(slidingDirection) && !CheckForCollisionLadder(slidingDirection))
                {
                    var pressureFactor = PSM.slideRightInput != 0 ? PSM.slideRightInput : PSM.slideLeftInput;
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
                        if ((PSM.slideRightInput != 0 && PSM.slideLeftInput != 0) || (PSM.slideRightInput == 0 && PSM.slideLeftInput == 0))
                        {
                            remappedPressureFactor = 1;
                        }

                        tDeceleration += Time.deltaTime / stats.timeToDecelerate * remappedPressureFactor;
                        currentSlidingSpeed = Mathf.Lerp(startSlidingSpeedForDeceleration, 0, tDeceleration);
                        tAcceleration = 0;
                        if (currentSlidingSpeed == 0)
                        {
                            PSM.slidingInput = 0;
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

                PSM.currentDistance += currentSlidingSpeed * PSM.slidingInput * Time.fixedDeltaTime;
                PSM.ladder.position = path.GetPointAtDistance(PSM.currentDistance, EndOfPathInstruction.Stop);
                #endregion

                #region end of Path
                //End Of Path, continue sliding with ReSnap or Fall from Path
                if (PSM.currentDistance <= 0 || PSM.currentDistance >= pathLength)
                {
                    Vector3 endOfShelfDirection = new Vector3();
                    if (PSM.closestRail != null)
                    {
                        if (PSM.currentDistance <= 0) //arriving at start of path
                        {
                            endOfShelfDirection = -pathDirection;
                        }
                        else if (PSM.currentDistance >= pathLength) //arriving at end of path
                        {
                            endOfShelfDirection = pathDirection; //ende - start
                        }
                    }
                    else
                        Debug.Log("There is something bad happening here lmao");

                    if (Vector3.Dot(slidingDirection, endOfShelfDirection) >= 0.9f) //player moves in the direction of the end point (move left when going out at start, moves right when going out at end)
                    {
                        if (PSM.CheckForNextClosestRail(PSM.closestRail))
                        {

                            PSM.OnResnap();
                        }
                        else
                        {
                            if (PSM.closestRail.stopSlidingAtTheEnd)
                            {
                                PSM.playerVelocity = ExtensionMethods.ClampPlayerVelocity(PSM.playerVelocity, pathDirection, 0);
                                currentSlidingSpeed = 0;
                                PSM.slidingInput = 0;
                            }
                            else
                            {
                                PSM.coyoteTimer = 0;
                                PSM.bonusVelocity += stats.fallingMomentumPercentage * currentSlidingSpeed * pathDirection * PSM.slidingInput;
                                PSM.OnFall();
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
        var slidingInput = PSM.slidingInput;
        if (PSM.slideLeftInput == 0 && PSM.slideRightInput != 0)
        {
            if (slidingInput == 0 || slidingInput == 1)
            {
                if (!startedAccelerating && currentSlidingSpeed != stats.maxSlidingSpeed)
                {
                    accelerate = true;
                    tDeceleration = 0;
                    decelerate = false;
                    PSM.slidingInput = 1;
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
        else if (PSM.slideLeftInput != 0 && PSM.slideRightInput == 0)
        {
            if (slidingInput == 0 || slidingInput == -1)
            {
                if (!startedAccelerating && currentSlidingSpeed != stats.maxSlidingSpeed)
                {
                    accelerate = true;
                    tDeceleration = 0;
                    decelerate = false;
                    PSM.slidingInput = -1;
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
        else if (PSM.slideRightInput != 0 && PSM.slideLeftInput != 0)
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
        if (PSM.slideRightInput == 0 && PSM.slideLeftInput == 0 && (startedAccelerating || startedDecelerating))
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
        Vector3 p1 = PSM.transform.position + controller.center + Vector3.up * -controller.height / 1.5f;
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
        LadderSizeStateMachine lSM = PSM.ladderSizeStateMachine;
        Vector3 boxExtents = new Vector3(lSM.ladderParent.localScale.x * 0.5f, lSM.ladderParent.localScale.y * 0.5f, lSM.ladderParent.localScale.z * 0.5f);

        if (Physics.BoxCast(PSM.ladder.position, lSM.ladderParent.localScale, moveDirection.normalized, out hit, Quaternion.identity, 0.1f, LayerMask.GetMask("SlidingObstacle")))
        {
            return true;
        }
        return false;
    }

    void CheckIfReadyToDismount()
    {
        // Dismounting the ladder on top and bottom 
        if (PSM.HeightOnLadder == 0 && PSM.forwardInput > 0)
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
                    dismountStartPos = PSM.transform.position;
                    PSM.dismounting = true;
                }
            }
        }
        else if (dismountTimer != 0)
        {
            dismountTimer = 0;
        }
    }

    void Dismount()
    {
        // 1 is how much units the player needs to move up to be on top of the rail.
        if ((PSM.transform.position - dismountStartPos).magnitude <= 1.3f && !dismountedHalfways)
        {
            PSM.HeightOnLadder += stats.ladderDismountSpeed * Time.fixedDeltaTime;
            PSM.transform.position = ladder.transform.position + PSM.ladderDirection * ladderSizeState.ladderLength * PSM.HeightOnLadder;
        }
        else if (!dismountedHalfways)
        {
            dismountStartPos = PSM.transform.position;
            dismountedHalfways = true;
        }

        // Make one step forward on the rail before changing to walking state.
        if ((PSM.transform.position - dismountStartPos).magnitude <= 0.1f && dismountedHalfways)
        {
            PSM.HeightOnLadder += stats.ladderDismountSpeed * Time.fixedDeltaTime;
            PSM.transform.position = ladder.transform.position + PSM.controller.transform.forward * ladderSizeState.ladderLength * PSM.HeightOnLadder;
        }
        else if (dismountedHalfways)
        {
            PSM.dismounting = false;
            PSM.OnLadderTop();
        }
    }
    #endregion

    public override IEnumerator Finish()
    {
        #region Finish Swinging
        SetCurrentPlayerVelocity(ladder.transform.position);
        if (!finishWithNormalJump)
        {
            if (shouldRetainSwingVelocity)
            {
                PSM.bonusVelocity += (currentMovement + Vector3.up * 1.1f).normalized * currentMovement.magnitude;
                PSM.baseVelocity = PSM.baseVelocity.normalized * Mathf.Clamp(PSM.baseVelocity.magnitude, 0, stats.MaximumMovementSpeed);
            }
            else
            {
                PSM.baseVelocity.y = 0;
            }
        }
        PSM.snapInputBool = false;
        PSM.startingSlidingInput = 0;
        #endregion
        #region Finish Sliding
        PSM.closestRail = null;
        Time.fixedDeltaTime = 0.02f;
        #endregion

        yield break;
    }

    public PlayerSwinging(PlayerMovementStateMachine playerStateMachine)
    : base(playerStateMachine)
    {

    }
}


