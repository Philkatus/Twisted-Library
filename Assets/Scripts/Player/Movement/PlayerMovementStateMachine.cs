using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementStateMachine : StateMachine
{
    #region public
    public float speed;
    public float jumpheight;
    public float FowardInput;
    public float SideWardsInput;

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
    }



}
