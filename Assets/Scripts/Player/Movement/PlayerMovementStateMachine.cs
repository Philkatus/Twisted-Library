using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementStateMachine : StateMachine
{
    #region public


    [Header("changeable")]
    public float movementAcceleration;
    public float maximumSpeed;
    public float movementDrag;

    [Space]
    public float OnLadderAcceleration;
    public float maximumSpeedOnLadder;

    [Space]
    public float slidingAcceleration;
    public float maxSlidingSpeed;
    public float ladderDrag;
    public float ladderDismountSpeed;

    [Space]
    public float jumpheight;
    public float gravity;
    public Vector3 playerVelocity;

    [Header("for Reference")]
    public float HeightOnLadder;
    public float LadderVelocity;
    public float currentDistance;
    public Quaternion ladderWalkingRotation;
    public Vector3 ladderWalkingPosition;
    public Vector3 ladderDirection
    {
        get
        {

            return ladderMesh.right;
        }
    }

    public List<Shelf> possibleShelfs;
    public Shelf closestShelf;

    public Transform ladder;
    public LadderSizeStateMachine ladderSizeStateMachine;

    public CharacterController controller;


    [HideInInspector] public bool OnGround;
    [HideInInspector] public float sideWaysInput;
    [HideInInspector] public float ForwardInput;

    #endregion

    #region Private
    [SerializeField] Transform ladderMesh;
    #endregion

    private void Start()
    {
        ladderWalkingPosition = ladder.localPosition;
        ladderWalkingRotation = ladder.localRotation;
        SetState(new PlayerWalking(this));
        possibleShelfs = new List<Shelf>();
    }

    private void FixedUpdate()
    {

    }

    private void Update()
    {
        GetInput();
        State.Movement();

        if (Input.GetButtonDown("Jump"))
        {
            State.Jump();
            Debug.Log(State.ToString() + ".jump");
        }

        if (Input.GetButtonDown("Interact") && CheckForShelf())
        {
            StartCoroutine(State.Snap());
            Debug.Log(State.ToString() + ".Snap");
        }
    }

    #region utillity
    public void GetInput()
    {
        ForwardInput = Input.GetAxis("Vertical");
        sideWaysInput = Input.GetAxis("Horizontal");
    }

    ///<summary>
    /// A Function to determin the closest Shelf to the player. Return false if none are in range.
    ///</summary>
    public bool CheckForShelf()
    {
        if (possibleShelfs.Count == 0)
        {
            return false;
        }
        else
        {
            float closestDistance = Mathf.Infinity;
            for (int i = 0; i < possibleShelfs.Count; i++)
            {
                float distance = Vector3.Distance(possibleShelfs[i].transform.position, transform.position);
                if (distance < closestDistance)
                {
                    closestShelf = possibleShelfs[i];
                    closestDistance = distance;
                }

            }



            return true;
        }
    }

    /// <summary>
    /// calculates the resulting velocity through a change in direction
    /// </summary>
    /// <param name="currentVelocity"> </param>
    /// <param name="targetDirection"> the normalized direction you want to change to</param>
    /// <returns></returns>
    public Vector3 resultingVelocity(Vector3 currentVelocity, Vector3 targetDirection)
    {
        float resultingSpeed = currentVelocity.x * targetDirection.x + currentVelocity.y * targetDirection.y + currentVelocity.z * targetDirection.z;

        return targetDirection * resultingSpeed;
    }
    #endregion
    #region functions to change State
    ///<summary>
    /// gets called when the player lands on the floor
    ///</summary>
    public void OnLand()
    {
        SetState(new PlayerWalking(this));
    }
    ///<summary>
    /// gets called when the player leaves the ladder on the top
    ///</summary>
    public void OnLadderTop()
    {
        SetState(new PlayerWalking(this));

    }
    ///<summary>
    /// gets called when the player leave the ladder on the bottom
    ///</summary>
    public void OnLadderBottom()
    {
        SetState(new PlayerWalking(this));

    }
    ///<summary>
    /// gets called when the player snappes his ladder to a shelf
    ///</summary>
    public void OnSnap()
    {
        SetState(new PlayerSliding(this));
        //ladder.SetState(new LadderSliding(ladder));
        ladderSizeStateMachine.SetState(new LadderBig(ladderSizeStateMachine));
    }
    ///<summary>
    /// gets called when the player changes to in the air
    ///</summary>
    public void OnFall()
    {
        SetState(new PlayerInTheAir(this));
        //ladder.SetState(new LadderWalking(ladder));
        ladderSizeStateMachine.SetState(new LadderSmall(ladderSizeStateMachine));
    }
    #endregion
}
