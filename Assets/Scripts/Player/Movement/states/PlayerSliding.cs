using System.Collections;
using UnityEngine;
using PathCreation;

public class PlayerSliding : State
{
    #region INHERITED


    #endregion

    #region PRIVATE
    float dismountTimer;
    float accelerateTimer;
    bool dismountedHalfways;
    bool canAccalerate;
    bool donethisCallbackAlready;
    Vector3 dismountStartPos;
    Vector3 pathDirection;

    #endregion


    #region PROTECTED
    protected VertexPath path;
    protected Rail closestRail;
    protected ValuesScriptableObject stats;
    protected PathCreator pathCreator;
    protected PlayerMovementStateMachine pSM;
    protected LadderSizeStateMachine ladderSizeState;
    protected CharacterController controller;
    protected Transform ladder;
    protected float currentDistance;
    protected float speed;
    protected float pathLength;
    protected float currentSlidingLevelSpeed;
    protected float leftHoldTimer;
    protected float rightHoldTimer;
    protected int currentSlidingLevel;
    protected bool stopping;
    protected bool startLeftHoldTimer;
    protected bool startRightHoldTimer;
    protected bool holdingChangeDirection;
    protected bool holdingLeftSlideButton;
    protected bool holdingRightSlideButton;
    protected bool shouldRetainSwingVelocity = true;



    #endregion
    public override void Initialize()
    {
        // Done in PlayerSwinging

        /*
        pSM = PlayerStateMachine;
        stats = pSM.stats;

        ladderSizeState = pSM.ladderSizeStateMachine;
        speed = stats.climbingSpeedOnLadder;
        closestRail = pSM.closestRail;
        controller = pSM.controller;
        ladder = pSM.ladder;
        pathCreator = closestRail.pathCreator;
        path = pathCreator.path;
        //pSM.HeightOnLadder = -1;

        // Place the ladder on the path.
        Vector3 startingPoint = pathCreator.path.GetClosestPointOnPath(pSM.transform.position);

        currentDistance = path.GetClosestDistanceAlongPath(startingPoint);
        ladder.transform.position = startingPoint;
        ladder.transform.forward = -path.GetNormalAtDistance(currentDistance);

        pathLength = path.cumulativeLengthAtEachVertex[path.cumulativeLengthAtEachVertex.Length - 1];
        pSM.currentDistance = currentDistance;

        // Place the character on ladder.
        ladder.transform.SetParent(pSM.myParent);
        Vector3 targetPosition = startingPoint - pSM.ladderDirection * ladderSizeState.ladderLength;
        targetPosition.y = Mathf.Clamp(controller.transform.position.y, targetPosition.y, startingPoint.y);
        pSM.HeightOnLadder = -(startingPoint - targetPosition).magnitude / ladderSizeState.ladderLength;
        pSM.HeightOnLadder = Mathf.Clamp(pSM.HeightOnLadder, -1, 0);
        pSM.transform.position = ladder.transform.position + pSM.ladderDirection * ladderSizeState.ladderLength * pSM.HeightOnLadder;
        controller.transform.forward = -pathCreator.path.GetNormalAtDistance(currentDistance);
        controller.transform.SetParent(ladder.transform);
        pSM.ladderSizeStateMachine.OnGrow();

        pathDirection = pathCreator.path.GetDirectionAtDistance(currentDistance, EndOfPathInstruction.Stop);
        if (stats.preservesVelocityOnSnap)
        {
            pSM.playerVelocity = pSM.ClampPlayerVelocity(pSM.playerVelocity, pathDirection, stats.maxSlidingSpeed);
        }
        else
        {
            pSM.playerVelocity = pSM.ClampPlayerVelocity(pSM.playerVelocity, pathDirection, 0);
        }

        pSM.stopSlidingAction.started += context => stopping = true;
        pSM.stopSlidingAction.canceled += context => stopping = false;
        Debug.LogError("fffd");
        if (stats.useNewSliding)
        {
            pSM.slidingInput = pSM.startingSlidingInput;
            pSM.slideLeftAction.performed += context => SwitchSpeedLevel("left");
            pSM.slideRightAction.performed += context => SwitchSpeedLevel("right");
            pSM.slideHoldLeftAction.performed += context => currentSlidingLevel = 1;
            pSM.slideHoldRightAction.performed += context => currentSlidingLevel = 1;
            Debug.LogError(pSM.startingSlidingInput);
            Debug.LogError("pSM.startingSlidingInput");
            if (pSM.startingSlidingInput == 0)
            {
                currentSlidingLevel = 0;
                currentSlidingLevelSpeed = 0;
                Debug.LogError("zes");
            }
            else
            {
                currentSlidingLevel = 1;
                currentSlidingLevelSpeed = stats.speedLevels[currentSlidingLevel];
            }
        }

        Time.fixedDeltaTime = 0.002f;*/
    }

    public override void ReInitialize()
    {
        // Assign variables.
        pSM = PlayerStateMachine;
        stats = pSM.stats;

        ladderSizeState = pSM.ladderSizeStateMachine;
        speed = stats.climbingSpeedOnLadder;
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

        //ladder.transform.forward = -path.GetNormalAtDistance(currentDistance);

        pathLength = path.cumulativeLengthAtEachVertex[path.cumulativeLengthAtEachVertex.Length - 1];
        pSM.currentDistance = path.GetClosestDistanceAlongPath(startingPoint);
    }

    public override IEnumerator Finish()
    {
        pSM.closestRail = null;
        Time.fixedDeltaTime = 0.02f;
        yield break;
    }

    public override void Jump()
    {
        if (ladderSizeState.isUnFolding)
        {
            float offSet = .5f;
            //PlayerStateMachine.baseVelocity.y += Mathf.Clamp((pSM.transform.position.y - ladderSizeState.startFoldingUpPos.y), 0, 1f) * ladderSizeState.foldJumpMultiplier;
            Vector3 direction = (-pSM.ladderDirection + Vector3.up * offSet).normalized; ;
            //PlayerStateMachine.baseVelocity += direction * 2.5f * ladderSizeState.foldJumpMultiplier;
            //PlayerStateMachine.ClampPlayerVelocity(PlayerStateMachine.baseVelocity, Vector3.up, stats.maxJumpingSpeed);
            //PlayerStateMachine.bonusVelocity = direction * (2.5f * ladderSizeState.foldJumpMultiplier - stats.maxJumpingSpeed);

            PlayerStateMachine.bonusVelocity = direction * (2.5f * ladderSizeState.foldJumpMultiplier);
            //Debug.Log("fold jump: " + direction * 2.5f * ladderSizeState.foldJumpMultiplier);
            // Debug.Log("fold jump bonus" + (2.5f * ladderSizeState.foldJumpMultiplier - stats.maxJumpingSpeed));
            //Debug.Log("fold jump : " + (pSM.transform.position.y - ladderSizeState.startFoldingUpPos.y) );
            shouldRetainSwingVelocity = false;
            PlayerStateMachine.OnFall();
            pSM.animationControllerisFoldingJumped = true;
        }

        if (ladderSizeState.isFoldingUp)
        {
            float offSet = .5f;
            float heightOnLadderRemapped = (-pSM.HeightOnLadder * stats.heightOnLadderKatapulFactor + 1 - stats.heightOnLadderKatapulFactor);
            //PlayerStateMachine.baseVelocity.y += Mathf.Clamp((pSM.transform.position.y - ladderSizeState.startFoldingUpPos.y), 0, 1f) * ladderSizeState.foldJumpMultiplier;
            Vector3 direction = (pSM.ladderDirection + Vector3.up * offSet).normalized;
            //PlayerStateMachine.baseVelocity += direction * 2.5f * ladderSizeState.foldJumpMultiplier;
            //PlayerStateMachine.ClampPlayerVelocity(PlayerStateMachine.baseVelocity, Vector3.up, stats.maxJumpingSpeed);
            //PlayerStateMachine.bonusVelocity = direction *( 2.5f * ladderSizeState.foldJumpMultiplier - stats.maxJumpingSpeed);
            //PlayerStateMachine.bonusVelocity = direction * (2.5f * ladderSizeState.foldJumpMultiplier) - direction * pSM.HeightOnLadder * 2.5f;
            PlayerStateMachine.bonusVelocity = direction * (2.5f * ladderSizeState.foldJumpMultiplier) * heightOnLadderRemapped;           
            //Debug.Log("fold jump: " + direction * 2.5f * ladderSizeState.foldJumpMultiplier);
            //Debug.Log("fold jump bonus" + (2.5f * ladderSizeState.foldJumpMultiplier - stats.maxJumpingSpeed));
            //Debug.Log("fold jump : " + (pSM.transform.position.y - ladderSizeState.startFoldingUpPos.y) );
            shouldRetainSwingVelocity = false;
            PlayerStateMachine.OnFall();
            pSM.animationControllerisFoldingJumped = true;
        }
        else
        {
            pSM.bonusVelocity += stats.fallingMomentumPercentage * pSM.playerVelocity;

            if (stats.wallJump != Vector3.zero) //just that it doesn't bug for the others TODO: put it the if statement away, only use wallJump
            {
                Vector3 fromWallVector = (Quaternion.AngleAxis(90, Vector3.up) * pathDirection).normalized;
                fromWallVector = fromWallVector * stats.wallJump.z;
                Vector3 fromWallValued = new Vector3(fromWallVector.x, stats.wallJump.y, fromWallVector.z);
                PlayerStateMachine.playerVelocity += fromWallValued;
                PlayerStateMachine.baseVelocity.y += stats.jumpHeight;
                PlayerStateMachine.isWallJumping = true;
            }
            else
            {
                PlayerStateMachine.baseVelocity.y += stats.jumpHeight;
            }
            shouldRetainSwingVelocity = true;
            PlayerStateMachine.OnFall();
            pSM.animationControllerisFoldingJumped = false;
        }
        PlayerStateMachine.jumpInputBool = false;
    }

    public override void Movement()
    {
        if (!stats.useNewSliding)
        {
            
            if (pSM.slidingInput == 0)
            {
                SwitchSlidingDirectionWithCameraRotation();
            }
            pathDirection = pathCreator.path.GetDirectionAtDistance(currentDistance, EndOfPathInstruction.Stop);
            if (!pSM.dismounting)
            {
                // Go up and down.
                if (!CheckForCollisionCharacter(pSM.forwardInput * pSM.ladderDirection))
                {
                    pSM.HeightOnLadder += pSM.forwardInput * speed * Time.fixedDeltaTime;
                    pSM.HeightOnLadder = Mathf.Clamp(pSM.HeightOnLadder, -1, 0);
                    
                    // set player position on ladder
                    pSM.transform.position = ladder.transform.position + 
                        pSM.ladderDirection * ladderSizeState.ladderLength * pSM.HeightOnLadder + 
                        ladder.transform.forward * -stats.playerOffsetFromLadder; //pos on ladder
                    //pSM.transform.localPosition = new Vector3(pSM.transform.localPosition.x, pSM.transform.localPosition.y, -0.7f);
                }

                #region Move horizontally.
                pathDirection = path.GetDirectionAtDistance(currentDistance);

                // Get sideways input, no input if both buttons held down.
                if (pSM.slideAction.triggered && pSM.slideAction.ReadValue<float>() == 0)
                {
                    pSM.playerVelocity -= pSM.resultingVelocity(pSM.playerVelocity, pathDirection);
                }

                //playervelocity increased with input
                float slidingAcceleration = ExtensionMethods.Remap(ladderSizeState.ladderLength, ladderSizeState.ladderLengthSmall, ladderSizeState.ladderLengthBig, stats.slidingAcceleration * stats.slidingSpeedSizeFactor, stats.slidingAcceleration);
                pSM.playerVelocity += pSM.slidingInput * pathDirection * Time.fixedDeltaTime * slidingAcceleration;

                //drag calculation
                float resultingSpeed = pSM.resultingSpeed(pSM.playerVelocity, pathDirection);

                //speed Drag (dependant on ladder size)
                float maxSlidingSpeed = ExtensionMethods.Remap(ladderSizeState.ladderLength, ladderSizeState.ladderLengthSmall, ladderSizeState.ladderLengthBig, stats.maxSlidingSpeed * stats.slidingSpeedSizeFactor, stats.maxSlidingSpeed);
                pSM.playerVelocity -= pathDirection * Mathf.Clamp(resultingSpeed * stats.slidingDragPercentage / 100, -maxSlidingSpeed, maxSlidingSpeed);

                //moving the object
                if (!CheckForCollisionCharacter(pSM.playerVelocity) && !stopping && !CheckForCollisionLadder(pSM.playerVelocity))
                {
                    pSM.currentDistance += pSM.resultingSpeed(pSM.playerVelocity, pathDirection) * stats.slidingVelocityFactor;

                    pSM.ladder.position = path.GetPointAtDistance(pSM.currentDistance, EndOfPathInstruction.Stop);



                    // Debug.Log(pSM.resultingSpeed(pSM.playerVelocity, pathDirection) * values.slidingVelocityFactor + " " + values.slidingVelocityFactor);

                }
                else
                {
                    pSM.playerVelocity = pSM.ClampPlayerVelocity(pSM.playerVelocity, pathDirection, 0);
                }
                #endregion
                #region end of Path
                //End Of Path, continue sliding with ReSnap or Fall from Path
                if (pSM.currentDistance <= 0 || pSM.currentDistance >= pathLength)
                {
                    Vector3 endOfRailDirection = new Vector3();
                    if (pSM.currentDistance <= 0) //arriving at start of path
                    {
                        endOfRailDirection = pSM.closestRail.transform.TransformPoint(pathCreator.bezierPath.GetPoint(0))
                                            - pSM.closestRail.transform.TransformPoint(pathCreator.bezierPath.GetPoint(pathCreator.bezierPath.NumAnchorPoints)); //start - ende
                    }
                    else if (pSM.currentDistance >= pathLength) //arriving at end of path
                    {
                        endOfRailDirection = pSM.closestRail.transform.TransformPoint(pathCreator.bezierPath.GetPoint(pathCreator.bezierPath.NumAnchorPoints))
                                            - pSM.closestRail.transform.TransformPoint(pathCreator.bezierPath.GetPoint(0)); //ende - start
                    }

                    Plane railPlane = new Plane(endOfRailDirection.normalized, Vector3.zero);

                    if (/* pSM.resultingSpeed( pSM.playerVelocity, pathDirection) >0   )*/railPlane.GetSide(Vector3.zero + pSM.playerVelocity)) //player moves in the direction of the end point (move left when going out at start, moves right when going out at end)
                    {
                        if (pSM.CheckForNextClosestRail(pSM.closestRail))
                        {
                            pSM.OnResnap();
                        }
                        else
                        {
                            if (pSM.closestRail.stopSlidingAtTheEnd)
                            {
                                pSM.playerVelocity = pSM.ClampPlayerVelocity(pSM.playerVelocity, pathDirection, 0);
                            }
                            else
                            {
                                pSM.bonusVelocity += stats.fallingMomentumPercentage * pSM.playerVelocity;
                                pSM.OnFall();
                            }
                        }
                    }
                }
                #endregion
                CheckIfReadyToDismount();
            }
            else
            {
                Dismount();
            }
        }
        else
        {
            if (!holdingRightSlideButton && !holdingLeftSlideButton)
            {
                SwitchSlidingDirectionWithCameraRotation();
            }
            CustomHoldInput();
            donethisCallbackAlready = false;
            accelerateTimer += Time.fixedDeltaTime;
            if (accelerateTimer >= stats.accelerationCooldown)
            {
                canAccalerate = true;
            }
            // Debug.LogError("speed " + currentSlidingLevelSpeed);
            // Debug.LogError("level " + currentSlidingLevel);

            pathDirection = pathCreator.path.GetDirectionAtDistance(currentDistance, EndOfPathInstruction.Stop);
            if (!pSM.dismounting)
            {
                // Go up and down.
                if (!CheckForCollisionCharacter(pSM.forwardInput * pSM.ladderDirection))
                {
                    pSM.HeightOnLadder += pSM.forwardInput * speed * Time.fixedDeltaTime;
                    pSM.HeightOnLadder = Mathf.Clamp(pSM.HeightOnLadder, -1, 0);
                    //pSM.transform.position = ladder.transform.position + pSM.ladderDirection * ladderSizeState.ladderLength * pSM.HeightOnLadder + ladder.transform.forward * -stats.playerOffsetFromLadder; //pos on ladder
                    pSM.transform.localPosition = new Vector3(0, ladderSizeState.ladderLength * pSM.HeightOnLadder, -0.38f);
                }

                #region Move horizontally.
                pathDirection = path.GetDirectionAtDistance(currentDistance);

                // Get sideways input, no input if both buttons held down.
                if (pSM.slideAction.triggered && pSM.slideAction.ReadValue<float>() == 0)
                {
                    pSM.playerVelocity -= pSM.resultingVelocity(pSM.playerVelocity, pathDirection);

                }

                //playervelocity increased with input
                float slidingAcceleration = ExtensionMethods.Remap(ladderSizeState.ladderLength, ladderSizeState.ladderLengthSmall, ladderSizeState.ladderLengthBig, stats.slidingAcceleration * stats.slidingSpeedSizeFactor, stats.slidingAcceleration);
                pSM.playerVelocity += pSM.slidingInput * pathDirection * Time.fixedDeltaTime * slidingAcceleration;

                //drag calculation
                float resultingSpeed = pSM.resultingSpeed(pSM.playerVelocity, pathDirection);

                //speed Clamp (dependant on ladder size)
                float maxSlidingSpeed = ExtensionMethods.Remap(ladderSizeState.ladderLength, ladderSizeState.ladderLengthSmall, ladderSizeState.ladderLengthBig, stats.maxSlidingSpeed * stats.slidingSpeedSizeFactor, stats.maxSlidingSpeed);
                pSM.playerVelocity -= pathDirection * Mathf.Clamp(resultingSpeed * stats.slidingDragPercentage / 100, --maxSlidingSpeed, maxSlidingSpeed);

                //moving the object
                if (!CheckForCollisionCharacter(pSM.playerVelocity) && !stopping && !CheckForCollisionLadder(pSM.playerVelocity))
                {
                    pSM.playerVelocity = pSM.ClampPlayerVelocity(pSM.playerVelocity, pathDirection, currentSlidingLevelSpeed * -pSM.slidingInput);
                    pSM.currentDistance += pSM.resultingSpeed(pSM.playerVelocity, pathDirection) * stats.slidingVelocityFactor;

                    pSM.ladder.position = path.GetPointAtDistance(pSM.currentDistance, EndOfPathInstruction.Stop);

                }
                else
                {
                    pSM.playerVelocity = pSM.ClampPlayerVelocity(pSM.playerVelocity, pathDirection, 0);
                }
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
                            endOfShelfDirection = pSM.closestRail.transform.TransformPoint(pathCreator.bezierPath.GetPoint(0))
                                                - pSM.closestRail.transform.TransformPoint(pathCreator.bezierPath.GetPoint(pathCreator.bezierPath.NumAnchorPoints)); //start - ende
                        }
                        else if (pSM.currentDistance >= pathLength) //arriving at end of path
                        {
                            endOfShelfDirection = pSM.closestRail.transform.TransformPoint(pathCreator.bezierPath.GetPoint(pathCreator.bezierPath.NumAnchorPoints))
                                                - pSM.closestRail.transform.TransformPoint(pathCreator.bezierPath.GetPoint(0)); //ende - start
                        }
                    }
                    else
                        Debug.Log("There is something bad happening here lmao");

                    Plane railPlane = new Plane(endOfShelfDirection.normalized, Vector3.zero);

                    if (/* pSM.resultingSpeed( pSM.playerVelocity, pathDirection) >0   )*/railPlane.GetSide(Vector3.zero + pSM.playerVelocity)) //player moves in the direction of the end point (move left when going out at start, moves right when going out at end)
                    {
                        if (pSM.CheckForNextClosestRail(pSM.closestRail))
                        {
                            pSM.OnResnap();
                        }
                        else
                        {
                            if (pSM.closestRail.stopSlidingAtTheEnd)
                            {
                                pSM.playerVelocity = pSM.ClampPlayerVelocity(pSM.playerVelocity, pathDirection, 0);
                                currentSlidingLevel = 0;
                                currentSlidingLevelSpeed = stats.speedLevels[0];
                                pSM.slidingInput = 0;
                            }
                            else
                            {
                                pSM.coyoteTimer = 0;
                                pSM.bonusVelocity += stats.fallingMomentumPercentage * pSM.playerVelocity;
                                pSM.OnFall();
                            }
                        }
                    }
                }
                #endregion
                CheckIfReadyToDismount();
            }
            else
            {
                Dismount();
            }
        }
    }

    void CustomHoldInput()
    {
        if (startLeftHoldTimer)
        {
            if (rightHoldTimer == 0)
            {
                leftHoldTimer += Time.fixedDeltaTime;
            }
            if (leftHoldTimer >= stats.timeToSwitchDirection)
            {
                holdingChangeDirection = true;
                ChangeDirection("left");
                leftHoldTimer = 0;
                startLeftHoldTimer = false;
            }
        }
        else if (startRightHoldTimer)
        {
            if (leftHoldTimer == 0)
            {
                rightHoldTimer += Time.fixedDeltaTime;
            }
            if (rightHoldTimer >= stats.timeToSwitchDirection)
            {
                holdingChangeDirection = true;
                ChangeDirection("right");
                rightHoldTimer = 0;
                startRightHoldTimer = false;
            }
        }
    }

    void ChangeDirection(string direction)
    {
        if ((direction == "left" && pSM.slidingInput * pSM.adjustedSlideDirection == 1) || (direction == "right" && pSM.slidingInput * pSM.adjustedSlideDirection == -1))
        {
            currentSlidingLevel = 1;
            currentSlidingLevelSpeed = stats.speedLevels[1];
            pSM.slidingInput *= -1;
        }
    }

    protected void SwitchSpeedLevel(string direction)
    {
        if (donethisCallbackAlready)
        {
            return;
        }

        float slidingInput = pSM.slidingInput * pSM.adjustedSlideDirection;
        if (direction == "left" && !holdingChangeDirection && !holdingRightSlideButton)
        {
            if (slidingInput == -1 && canAccalerate)
            {
                if (stats.speedLevels[stats.speedLevels.Count - 1] != stats.speedLevels[currentSlidingLevel])
                {
                    accelerateTimer = 0;
                    currentSlidingLevel += 1;
                }
            }
            else if (slidingInput == 0)
            {
                pSM.slidingInput = -1 * pSM.adjustedSlideDirection;
                currentSlidingLevel = 1;
            }
            else if (slidingInput == +1)
            {
                if (currentSlidingLevel == 0)
                {
                    pSM.slidingInput = -1 * pSM.adjustedSlideDirection;
                    currentSlidingLevel = 1;
                }
                else if (currentSlidingLevel == 1)
                {
                    currentSlidingLevel = 0;
                    pSM.slidingInput = 0;
                }
                else
                {
                    currentSlidingLevel -= 1;
                }
            }
        }
        else if (direction == "right" && !holdingChangeDirection && !holdingLeftSlideButton)
        {
            if (slidingInput == -1)
            {
                if (currentSlidingLevel == 0)
                {
                    pSM.slidingInput = 1 * pSM.adjustedSlideDirection;
                    currentSlidingLevel = 1;
                }
                else if (currentSlidingLevel == 1)
                {
                    currentSlidingLevel = 0;
                    pSM.slidingInput = 0;
                }
                else
                {
                    currentSlidingLevel -= 1;
                }
            }
            else if (pSM.slidingInput == 0)
            {
                pSM.slidingInput = 1 * pSM.adjustedSlideDirection;
                currentSlidingLevel = 1;
            }
            else if (slidingInput == 1 && canAccalerate)
            {
                if (stats.speedLevels[stats.speedLevels.Count - 1] != stats.speedLevels[currentSlidingLevel])
                {
                    accelerateTimer = 0;
                    currentSlidingLevel += 1;
                }
            }
        }
        currentSlidingLevelSpeed = stats.speedLevels[currentSlidingLevel];
        canAccalerate = false;
        holdingChangeDirection = false;
        donethisCallbackAlready = true;
    }

    void SwitchSlidingDirectionWithCameraRotation()
    {
        var camDirection = ExtensionMethods.AngleDirection(pathDirection, Camera.main.transform.forward, Vector3.up);

        if (camDirection == -1)
        {
            pSM.adjustedSlideDirection = 1;
        }
        if (camDirection == 1)
        {
            pSM.adjustedSlideDirection = -1;
        }
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
            // dismountTimer += Time.fixedDeltaTime;
            // if (dismountTimer >= stats.ladderDismountTimer)
            // {
            //     dismountTimer = 0;
            //     controller.transform.forward = -pathCreator.path.GetNormalAtDistance(currentDistance);
            //     PlayerStateMachine.OnLadderBottom();
            // }
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

    public PlayerSliding(PlayerMovementStateMachine playerStateMachine)
        : base(playerStateMachine)
    {

    }
}
