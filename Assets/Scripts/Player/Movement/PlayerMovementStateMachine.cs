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
    public bool isWallJumping;
    public bool animationControllerisFoldingJumped;

    public List<Shelf> possibleShelves;
    public Shelf closestShelf;
    public Transform ladder;
    public Transform ladderMesh;
    public LadderSizeStateMachine ladderSizeStateMachine;
    public CharacterController controller;
    [HideInInspector] public InputAction slideAction;
    [HideInInspector] public InputAction swingAction;
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
    InputActionMap playerControlsMap;
    InputAction jumpAction;
    InputAction moveAction;
    InputAction snapAction;
    InputAction foldAction;
    #endregion

    private void Start()
    {
        myParent = transform.parent;

        ladderWalkingPosition = ladder.localPosition;
        ladderWalkingRotation = ladder.localRotation;

        SetState(new PlayerWalking(this));
        possibleShelves = new List<Shelf>();

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
        #endregion
    }

    private void Update()
    {
        GetInput();
        State.Movement();
    }

    public void TryToSnapToShelf()
    {
        if (CheckForShelf())
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
        isPerformedFold = foldAction.triggered;
    }

    ///<summary>
    /// A function to determine the closest shelf to the player. Returns false if none are in range.
    ///</summary>
    public bool CheckForShelf()
    {
        if (possibleShelves.Count == 0)
        {
            return false;
        }
        else
        {
            float closestDistance = Mathf.Infinity;
            for (int i = 0; i < possibleShelves.Count; i++)
            {
                float distance = Vector3.Distance(possibleShelves[i].transform.position, transform.position);
                if (distance < closestDistance)
                {
                    closestShelf = possibleShelves[i];
                    closestDistance = distance;
                }
            }
            return true;
        }
    }

    ///<summary>
    /// A Function to determin the closest Shelf to the player that ignores the currentShelf. Return false if none are in range.
    ///</summary>
    public bool CheckForNextClosestShelf(Shelf currentClosestShelf)
    {
        if (possibleShelves.Count == 1)
        {
            return false;
        }
        else
        {
            //�finding of the direction  of the current rail
            VertexPath currentClosestPath = currentClosestShelf.pathCreator.path;
            Vector3 currentDirection = currentClosestPath.GetDirectionAtDistance(currentDistance, EndOfPathInstruction.Stop);


            float closestDistance = Mathf.Infinity;
            Shelf nextClosestShelf = null;

            for (int i = 0; i < possibleShelves.Count; i++)
            {
                float distance = Vector3.Distance(possibleShelves[i].transform.position, transform.position);
                VertexPath possiblePath = possibleShelves[i].pathCreator.path;
                Vector3 possiblePathDirection = possiblePath.GetDirectionAtDistance(
                possiblePath.GetClosestDistanceAlongPath(currentClosestPath.GetPointAtDistance(currentDistance, EndOfPathInstruction.Stop)), EndOfPathInstruction.Stop);


                if (distance < closestDistance
                    && possibleShelves[i] != currentClosestShelf
                    && possibleShelves[i].transform.position.y == currentClosestShelf.transform.position.y)
                {
                    if (Mathf.Abs(Vector3.Dot(currentDirection, possiblePathDirection)) >= .99f)
                    {
                        closestDistance = distance;
                        nextClosestShelf = possibleShelves[i];
                    }
                }
            }


            if (nextClosestShelf != null)
            {
                closestShelf = nextClosestShelf;
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
    #endregion

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
        currentVelocity -= this.resultingVelocity(currentVelocity,targetDirection);
        currentVelocity += clampedVelocity;
        

        return currentVelocity;
    }

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
    /// Gets called when the player snaps his ladder to a shelf.
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
}
