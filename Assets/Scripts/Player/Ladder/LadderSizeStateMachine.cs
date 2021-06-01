using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderSizeStateMachine : StateMachine
{
    #region public
    [Header("For reference")]
    public float ladderLength;
    public bool isFoldingUp;
    public bool isUnFolding = false;
    public PlayerMovementStateMachine playerStateMachine;
    public Transform ladderParent;
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
        if (!(State is LadderBig))
        {
            SetState(new LadderBig(this));
            playerStateMachine.ladderState = PlayerMovementStateMachine.LadderState.LadderBig;
        }
    }

    ///<summary>
    /// Is called when the ladder shortens.
    ///</summary>
    public void OnShrink()
    {
        if (!(State is LadderSmall))
        {
            SetState(new LadderSmall(this));
            playerStateMachine.ladderState = PlayerMovementStateMachine.LadderState.LadderSmall;
        }
    }


    ///<summary>
    /// Is called when the ladder folds in both directions.
    ///</summary>
    public void OnFold()
    {
        if (playerStateMachine.playerState == PlayerMovementStateMachine.PlayerState.swinging)
        {
            playerStateMachine.foldInputBool = false;
            if (!(State is LadderFold))
            {
                SetState(new LadderFold(this));
                playerStateMachine.ladderState = PlayerMovementStateMachine.LadderState.LadderFold;
            }
            else if (!(State is LadderUnfold))
            {
                SetState(new LadderUnfold(this));
                playerStateMachine.ladderState = PlayerMovementStateMachine.LadderState.LadderUnfold;
            }
        }
    }

    public void OnSnap()
    {
        SetState(new LadderUnfold(this));
        playerStateMachine.ladderState = PlayerMovementStateMachine.LadderState.LadderUnfold;
    }

    public void OnLadderPush()
    {
        SetState(new LadderRocketJump(this));
        playerStateMachine.ladderState = PlayerMovementStateMachine.LadderState.LadderRocketJump;
    }
    public void OnRocketJumpEnd()
    {
        SetState(new LadderFold(this));
        playerStateMachine.ladderState = PlayerMovementStateMachine.LadderState.LadderFold;
    }

    #endregion
}
