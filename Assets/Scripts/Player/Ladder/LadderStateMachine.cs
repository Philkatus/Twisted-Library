using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderStateMachine : StateMachine
{
    #region public
    public float length;
    public PlayerMovementStateMachine playerStateMachine;
    [HideInInspector] public float LadderVelocity;
    public Vector3 direction
    {
        get
        {
            
            return ladderMesh.right;
        }
    }
    #endregion

    #region private
    [SerializeField] Transform ladderMesh;
    #endregion
    private void Start()
    {
        SetState(new LadderWalking(this));
    }

    private void Update()
    {
        playerStateMachine.GetInput();
        State.Movement();
    }
}
