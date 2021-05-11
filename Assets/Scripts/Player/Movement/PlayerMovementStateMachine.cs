using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PathCreation;

public class PlayerMovementStateMachine : StateMachine
{
    #region public

    [Header("Changeable")]
    [Tooltip("Change to use different variable value sets. Found in Assets-> Scripts-> Cheat Sheets.")]
    public ValuesScriptableObject valuesAsset;
    public InputActionAsset actionAsset;
    [Space]
    [Header("For reference")]
    public PlayerState playerState;
    public LadderState ladderState;
    [Space]
    public float swingingPosition;
    public float HeightOnLadder = -1;
    public float currentDistance;
    public float sideWaysInput;
    public float forwardInput;
    public float swingingInput;
    public float slidingInput;
    public bool isPerformedFold;
    public bool dismounting;

    public Vector3 playerVelocity;
    public Vector3 railCheckLadderPosition;

    public bool isWallJumping;
    public bool animationControllerisFoldingJumped;

    public Rail closestRail;
    public Transform ladder;
    public Transform ladderMesh;
    public LadderSizeStateMachine ladderSizeStateMachine;
    public CharacterController controller;
    [HideInInspector] public InputAction slideAction;
    [HideInInspector] public InputAction swingAction;
    [HideInInspector] public InputAction snapAction;
    [HideInInspector] public InputAction stopSlidingAction;
    [HideInInspector] public Quaternion ladderWalkingRotation;
    [HideInInspector] public Vector3 ladderWalkingPosition;
    [HideInInspector]
    public Vector3 ladderDirection
    {
        get
        {
            return ladderMesh.right;
        }
    }
    [HideInInspector] public Transform myParent;
    #endregion

    #region Private
    float railCheckTimer;
    RailSearchManager railAllocator;
    InputActionMap playerControlsMap;
    InputAction jumpAction;
    InputAction moveAction;

    InputAction foldAction;
    #endregion

    private void Start()
    {
        myParent = transform.parent;
        railAllocator = RailSearchManager.instance;
        ladderWalkingPosition = ladder.localPosition;
        ladderWalkingRotation = ladder.localRotation;

        SetState(new PlayerWalking(this));
        #region controls
        playerControlsMap = actionAsset.FindActionMap("PlayerControls");
        playerControlsMap.Enable();
        jumpAction = playerControlsMap.FindAction("Jump");
        moveAction = playerControlsMap.FindAction("Movement");
        snapAction = playerControlsMap.FindAction("Snap");
        slideAction = playerControlsMap.FindAction("Slide");
        swingAction = playerControlsMap.FindAction("Swing");
        foldAction = playerControlsMap.FindAction("Fold");
        stopSlidingAction = playerControlsMap.FindAction("StopSliding");

        jumpAction.performed += context => State.Jump();
        snapAction.performed += context => TryToSnapToShelf();
        foldAction.performed += context => ladderSizeStateMachine.OnFold();
        #endregion
    }

    private void Update()
    {
        railCheckTimer += Time.deltaTime;
        if (railCheckTimer >= 0.6f)
        {
            CheckForRail();
            railCheckTimer = 0;
        }
    }

    private void FixedUpdate()
    {
        GetInput();
        State.Movement();
        Debug.DrawRay(transform.position, playerVelocity, Color.magenta);
    }

    public void TryToSnapToShelf()
    {

        if (CheckForRail())
        {
            State.Snap();
        }

    }

    #region utility
    public void GetInput()
    {
        forwardInput = moveAction.ReadValue<Vector2>().y;
        sideWaysInput = moveAction.ReadValue<Vector2>().x;
        slidingInput = slideAction.ReadValue<float>();
        swingingInput = swingAction.ReadValue<float>();

    }

    ///<summary>
    /// A function to determine the closest rail to the player. Returns false if none are in range.
    ///</summary>
    public bool CheckForRail()
    {
        if (playerState == PlayerState.walking)
        {
            railCheckLadderPosition = controller.transform.position;
        }
        else if (playerState == PlayerState.sliding)
        {
            railCheckLadderPosition = ladder.transform.position;
        }
        else if (playerState == PlayerState.inTheAir)
        {
            railCheckLadderPosition = controller.transform.position;
        }

        railAllocator.CheackForRailsInRange(controller.transform);
        var possibleRails = railAllocator.railsInRange;

        if (possibleRails.Count == 0)
        {
            return false;
        }
        else
        {
            float closestDistance = valuesAsset.snappingDistance;
            for (int i = 0; i < possibleRails.Count; i++)
            {

                float distance = Vector3.Distance(possibleRails[i].pathCreator.path.GetClosestPointOnPath(railCheckLadderPosition), railCheckLadderPosition);

                if (distance < closestDistance)
                {
                    closestRail = possibleRails[i];
                    closestDistance = distance;
                }
            }
            if (closestRail != null)
            {
                var visualsRail = closestRail.transform.GetChild(0).gameObject.GetComponent<ChangeRailVisualization>();
                if (visualsRail)
                {
                    visualsRail.isClosestRail = true;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    ///<summary>
    /// A function to determine the closest rail to the player that ignores the current rail. Return false if none are in range.
    ///</summary>
    public bool CheckForNextClosestRail(Rail currentClosestRail)
    {
        railCheckLadderPosition = ladder.transform.position;
        railAllocator.CheackForRailsInRange(controller.transform);
        var possibleRails = railAllocator.railsInRange;

        if (possibleRails.Count == 1)
        {
            return false;
        }
        else
        {
            //finding of the direction  of the current rail
            VertexPath currentClosestPath = currentClosestRail.pathCreator.path;
            Vector3 currentDirection = currentClosestPath.GetDirectionAtDistance(currentDistance, EndOfPathInstruction.Stop);

            float closestDistance = valuesAsset.slidingSnappingDistance;
            Rail nextClosestShelf = null;

            for (int i = 0; i < possibleRails.Count; i++)
            {
                float distance = Vector3.Distance(possibleRails[i].pathCreator.path.GetClosestPointOnPath(railCheckLadderPosition), railCheckLadderPosition);
                VertexPath possiblePath = possibleRails[i].pathCreator.path;
                Vector3 possiblePathDirection = possiblePath.GetDirectionAtDistance(
                possiblePath.GetClosestDistanceAlongPath(currentClosestPath.GetPointAtDistance(currentDistance, EndOfPathInstruction.Stop)), EndOfPathInstruction.Stop);

                if (distance < closestDistance
                    && possibleRails[i] != currentClosestRail
                    && possibleRails[i].transform.position.y == currentClosestRail.transform.position.y)
                {
                    if (Mathf.Abs(Vector3.Dot(currentDirection, possiblePathDirection)) >= .99f)
                    {
                        closestDistance = distance;
                        nextClosestShelf = possibleRails[i];
                    }
                }
            }

            if (nextClosestShelf != null)
            {
                closestRail = nextClosestShelf;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Calculates the resulting signed magnitude alongside the targetdirection after a change of direction.
    /// </summary>
    /// <param name="currentVelocity">the Velocity to change </param>
    /// <param name="targetDirection">The normalized direction you want to change to</param>
    /// <returns></returns>
    public float resultingSpeed(Vector3 currentVelocity, Vector3 targetDirection)
    {
        float resultingSpeed = currentVelocity.x * targetDirection.x + currentVelocity.y * targetDirection.y + currentVelocity.z * targetDirection.z;

        return resultingSpeed;
    }

    /// <summary>
    /// calculates the resulting velocity through a change in direction
    /// </summary>
    /// <param name="currentVelocity"> the Velocity to change </param>
    /// <param name="targetDirection"> the normalized direction you want to change to</param>
    /// <returns></returns>
    public Vector3 resultingVelocity(Vector3 currentVelocity, Vector3 targetDirection)
    {
        float resultingSpeed = this.resultingSpeed(currentVelocity, targetDirection);

        return targetDirection * resultingSpeed;
    }
    /// <summary>
    /// calculates the resulting clamped velocity through a change in direction
    /// </summary>
    /// <param name="currentVelocity"> the Velocity to change </param>
    /// <param name="targetDirection"> the normalized direction you want to change to</param>
    /// <param name="maximumSpeed"> the maximum speed the return value gets clamped to</param>
    /// <returns></returns>
    public Vector3 resultingClampedVelocity(Vector3 currentVelocity, Vector3 targetDirection, float maximumSpeed)
    {
        float resultingSpeed = this.resultingSpeed(currentVelocity, targetDirection);
        resultingSpeed = Mathf.Clamp(resultingSpeed, -maximumSpeed, maximumSpeed);

        return targetDirection * resultingSpeed;
    }

    /// <summary>
    /// takes the Player Velocity and puts a clamp on one direction of it
    /// </summary>
    /// <param name="currentVelocity"> the Velocity to change </param>
    /// <param name="targetDirection"> The direction to clamp </param>
    /// <param name="maximumSpeed"> the maximumspeed that the return Vector should have in the target direction </param>
    /// <returns></returns>
    public Vector3 ClampPlayerVelocity(Vector3 currentVelocity, Vector3 targetDirection, float maximumSpeed)
    {
        float resultingSpeed = this.resultingSpeed(currentVelocity, targetDirection);
        Vector3 clampedVelocity = targetDirection * Mathf.Clamp(resultingSpeed, -maximumSpeed, maximumSpeed);
        currentVelocity -= this.resultingVelocity(currentVelocity, targetDirection);
        currentVelocity += clampedVelocity;


        return currentVelocity;
    }
    #endregion




    #region functions to change states
    ///<summary>
    /// Gets called when the player lands on the floor.
    ///</summary>
    public void OnLand()
    {
        SetState(new PlayerWalking(this));
        playerState = PlayerState.walking;
        HeightOnLadder = -1;
    }
    ///<summary>
    /// Gets called when the player leaves the ladder on the top.
    ///</summary>
    public void OnLadderTop()
    {
        SetState(new PlayerWalking(this));
        playerState = PlayerState.walking;
        ladderSizeStateMachine.OnShrink();

    }
    ///<summary>
    /// Gets called when the player leaves the ladder on the bottom.
    ///</summary>
    public void OnLadderBottom()
    {
        SetState(new PlayerInTheAir(this));
        playerState = PlayerState.inTheAir;
        ladderSizeStateMachine.OnShrink();

    }
    ///<summary>
    /// Gets called when the player snaps his ladder to a rail.
    ///</summary>
    public void OnSnap()
    {
        ladderSizeStateMachine.OnGrow();


        if (valuesAsset.useSwinging)
        {
            SetState(new PlayerSwinging(this));
            playerState = PlayerState.swinging;
        }
        else
        {
            SetState(new PlayerSliding(this));
            playerState = PlayerState.sliding;
        }

    }

    ///<summary>
    /// Gets called when the player snaps to the next path.
    ///</summary>
    public void OnResnap()
    {
        if (valuesAsset.useSwinging)
        {
            SetState(this.State);
            playerState = PlayerState.swinging;
        }
        else
        {
            SetState(this.State);
            playerState = PlayerState.sliding;
        }

    }

    ///<summary>
    /// Gets called when the player changes to in the air.
    ///</summary>
    public void OnFall()
    {
        SetState(new PlayerInTheAir(this));
        playerState = PlayerState.inTheAir;
        ladderSizeStateMachine.OnShrink();
        HeightOnLadder = -1;
    }
    #endregion

    public enum PlayerState
    {
        walking,
        inTheAir,
        sliding,
        swinging

    };

    public enum LadderState
    {
        LadderBig,
        LadderSmall,
        LadderFold,
        LadderUnfold

    };
}
