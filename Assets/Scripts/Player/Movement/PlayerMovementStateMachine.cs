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

    [Header("For reference")]
    public float swingingPosition;
    public float HeightOnLadder=-1;
    public float currentDistance;
    public Quaternion ladderWalkingRotation;
    public Vector3 ladderWalkingPosition;
    public Vector3 playerVelocity;
    public Vector3 ladderDirection
    {
        get
        {
            return ladderMesh.right;
        }
    }

    public List<Shelf> possibleShelves;
    public Shelf closestShelf;
    public Transform ladder;
    public LadderSizeStateMachine ladderSizeStateMachine;
    public CharacterController controller;
    [HideInInspector] public InputAction slideAction;
    [HideInInspector] public InputAction slideLeftAction;
    [HideInInspector] public InputAction swingAction;

    public float sideWaysInput;
    public float forwardInput;
    
    public float swingingInput;
    public float slidingInput;

    [HideInInspector]public Transform myParent;
    #endregion

    #region Private
    [SerializeField] Transform ladderMesh;
    InputActionMap playerControlsMap;
    InputAction jumpAction;
    InputAction moveAction;
    InputAction snapAction;
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

        jumpAction.performed += context => State.Jump();
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
                Vector3 possiblePathDirection = possiblePath.GetDirectionAtDistance(possiblePath.GetClosestDistanceAlongPath(transform.position), EndOfPathInstruction.Stop);

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
    /// <param name="currentVelocity">The velocity you start with before the change </param>
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
    /// <param name="currentVelocity"> </param>
    /// <param name="targetDirection"> the normalized direction you want to change to</param>
    /// <returns></returns>
    public Vector3 resultingVelocity(Vector3 currentVelocity, Vector3 targetDirection)
    {
        float resultingSpeed = this.resultingSpeed(currentVelocity, targetDirection);

        return targetDirection * resultingSpeed;
    }
    #endregion

    #region functions to change states
    ///<summary>
    /// Gets called when the player lands on the floor.
    ///</summary>
    public void OnLand()
    {
        SetState(new PlayerWalking(this));
        HeightOnLadder = -1;
    }
    ///<summary>
    /// Gets called when the player leaves the ladder on the top.
    ///</summary>
    public void OnLadderTop()
    {
        SetState(new PlayerWalking(this));
        ladderSizeStateMachine.OnShrink();
    }
    ///<summary>
    /// Gets called when the player leaves the ladder on the bottom.
    ///</summary>
    public void OnLadderBottom()
    {
        SetState(new PlayerInTheAir(this));
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
        }
        else 
        {
            SetState(new PlayerSliding(this));
        }
      
    }
    ///<summary>
    /// Gets called when the player changes to in the air.
    ///</summary>
    public void OnFall()
    {
        SetState(new PlayerInTheAir(this));
        ladderSizeStateMachine.OnShrink();
        HeightOnLadder = -1;
    }
    #endregion
}
