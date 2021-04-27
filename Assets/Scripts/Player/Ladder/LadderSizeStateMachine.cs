using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderSizeStateMachine : StateMachine
{
    #region public
    [Header("Changeable")]
    public float ladderLengthBig;
    public float ladderLengthSmall;
    public float foldSpeed;
    public float foldJumpMultiplier;
    public float extraFoldJumpTimer;

    [Header("For reference")]
    public float ladderLength;
    public bool isFoldingUp;
    public Vector3 startFoldingUpPos;
    public PlayerMovementStateMachine playerStateMachine;
    public Transform ladderParent;

    #endregion

    #region private

    #endregion

    private void Start()
    {
        SetState(new LadderSmall(this));
    }

    private void Update()
    {
        State.Fold();
    }

    #region Functions to change the State
    ///<summary>
    /// Is called when the ladder expands.
    ///</summary>
    public void OnGrow()
    {
        if (State.GetType() != new LadderBig(this).GetType())
        {
            SetState(new LadderBig(this));

        }
    }

    ///<summary>
    /// Is called when the ladder shortens.
    ///</summary>
    public void OnShrink()
    {
        if (State.GetType() != new LadderSmall(this).GetType())
        {
            SetState(new LadderSmall(this));
        }
    }


    ///<summary>
    /// Is called when the ladder folds in both directions.
    ///</summary>
    public void OnFold()
    {
        if (State.GetType() != new LadderFold(this).GetType())
        {
            SetState(new LadderFold(this));
        }
        else if (State.GetType() != new LadderUnfold(this).GetType())
        {
            SetState(new LadderUnfold(this));
        }
    }

 
    #endregion
}
