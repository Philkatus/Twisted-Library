using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementStateMachine : StateMachine
{
    #region public
   

    [Header("changeable")]
    public float movementSpeed;
    public float speedOnLadder;
    public float jumpheight;
    
    [Header("for Reference")]
    public float HeightOnLadder;
    public float LadderVelocity;

    public Shelf closestShelf;

    public LadderStateMachine ladderScript;

    public CharacterController controller;

    [HideInInspector] public List<Shelf> possibleShelfs;
    [HideInInspector] public bool OnGround;
    [HideInInspector] public float SideWaysInput;
    [HideInInspector] public float ForwardInput;

    #endregion

    #region Private

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

    public void OnLand()
    {
        SetState(new PlayerWalking(this));
    }
    public void OnLadderTop()
    {
        SetState(new PlayerWalking(this));

    }
    public void OnLadderBottom()
    {
        SetState(new PlayerWalking(this));

    }
    public void OnSnap()
    {
        SetState(new PlayerSliding(this));

    }
    public void OnFall()
    {
        SetState(new PlayerInTheAir(this));

    }

}
