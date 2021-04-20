using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementStateMachine : StateMachine
{
    #region public
    [Header("Changeable")]
    public float movementAcceleration;
    public float maximumSpeed;
    public float movementDrag;

    [Space]
    public float OnLadderAcceleration;
    public float maximumSpeedOnLadder;

    [Space]
    public float slidingAcceleration;
    public float maxSlidingSpeed;
    [Range(0,50f)] public float slidingDragPercentage;

    public float ladderDismountSpeed;

    [Space]
    public float jumpheight;
    [Range(.1f, 1)] public float jumpMovementFactor;
    public float gravity;

    [Header("For reference")]
    public float HeightOnLadder;
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

    [HideInInspector] public float sideWaysInput;
    [HideInInspector] public float forwardInput;
    #endregion

    #region Private
    [SerializeField] Transform ladderMesh;
    #endregion

    private void Start()
    {
        ladderWalkingPosition = ladder.localPosition;
        ladderWalkingRotation = ladder.localRotation;
        SetState(new PlayerWalking(this));
        possibleShelves = new List<Shelf>();
    }

    private void Update()
    {
        GetInput();
        State.Movement();

        if (Input.GetButtonDown("Jump"))
        {
            State.Jump();
        }

        if (Input.GetButtonDown("Interact") && CheckForShelf())
        {
            StartCoroutine(State.Snap());
        }
    }

    #region utility
    public void GetInput()
    {
        forwardInput = Input.GetAxis("Vertical");
        sideWaysInput = Input.GetAxis("Horizontal");
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
    }
    ///<summary>
    /// Gets called when the player leaves the ladder on the top.
    ///</summary>
    public void OnLadderTop()
    {
        SetState(new PlayerWalking(this));
    }
    ///<summary>
    /// Gets called when the player leaves the ladder on the bottom.
    ///</summary>
    public void OnLadderBottom()
    {
        SetState(new PlayerInTheAir(this));
    }
    ///<summary>
    /// Gets called when the player snaps his ladder to a shelf.
    ///</summary>
    public void OnSnap()
    {
        SetState(new PlayerSliding(this));
        ladderSizeStateMachine.SetState(new LadderBig(ladderSizeStateMachine));
    }
    ///<summary>
    /// Gets called when the player changes to in the air.
    ///</summary>
    public void OnFall()
    {
        SetState(new PlayerInTheAir(this));
        ladderSizeStateMachine.SetState(new LadderSmall(ladderSizeStateMachine));
    }
    #endregion
}
