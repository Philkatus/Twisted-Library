using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementStateMachine : StateMachine
{
    #region public


    [Header("changeable")]
    public float movementSpeed;
    public float speedOnLadder;
    public float slidingSpeed;
    public float maximumSpeedOnLadder;
    public float ladderDrag;
    public float jumpheight;
    public float gravity;
    public Vector3 playerVelocity;

    [Header("for Reference")]
    public float HeightOnLadder;
    public float LadderVelocity;
    public float momentum;
    public float currentDistance;
    public Vector3 moveDirection;
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
    [HideInInspector] public float SideWaysInput;
    [HideInInspector] public float ForwardInput;

    #endregion

    #region Private
    [SerializeField] Transform ladderMesh;
    #endregion

    private void Start()
    {
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

    public void GetInput()
    {
        ForwardInput = Input.GetAxis("Vertical");
        SideWaysInput = Input.GetAxis("Horizontal");
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
