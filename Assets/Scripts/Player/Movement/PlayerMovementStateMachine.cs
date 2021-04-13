using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementStateMachine : StateMachine
{
    #region public
    public CharacterController controller;
    public float speed;
    public float jumpheight;
    public float FowardInput;
    public float SideWardsInput;
    public float closestShelf;
    public float possibleShelfs;

    [HideInInspector] public bool OnGround;
    #endregion

    #region Private

    #endregion

    private void Start()
    {
        SetState(new PlayerWalking(this));
    }

    private void Update()
    {
        if (Input.GetButtonDown("jump")) 
        {
            StartCoroutine(State.Jump());
        }

        if (Input.GetButtonDown("interact")) 
        {
            StartCoroutine(State.Snap());
        }
        
    }

    /*
    public bool CheckForShelf() 
    {

    }
    */

}
