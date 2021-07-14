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
    public Transform followTarget;
    public Animator anim;
    public Transform LadderVisuals;

    public MeshRenderer[] upGrades = new MeshRenderer[3];
    #endregion

    private void Start()
    {
        SetState(new LadderFold(this));
    }

    private void Update()
    {
        State.Fold();
        State.FollowLadderTarget();
    }

    #region Functions to change the State
    ///<summary>
    /// Is called when the ladder expands.
    ///</summary>
    public void OnGrow()
    {
        if (!(State is LadderUnfold))
        {
            SetState(new LadderUnfold(this));
            playerStateMachine.ladderState = PlayerMovementStateMachine.LadderState.LadderUnfold;
        }
    }

    ///<summary>
    /// Is called when the ladder shortens.
    ///</summary>
    public void OnShrink()
    {
        if (!(State is LadderFollow))
        {
            SetState(new LadderFollow(this));
            playerStateMachine.ladderState = PlayerMovementStateMachine.LadderState.LadderFollow;
            playerStateMachine.snappingStep = PlayerMovementStateMachine.SnappingStep.Finished;
        }
    }

    ///<summary>
    /// Is called when the ladder folds in both directions.
    ///</summary>
    public void OnFold()
    {
        if (playerStateMachine.playerState == PlayerMovementStateMachine.PlayerState.swinging && !playerStateMachine.dismounting)
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
        if (!playerStateMachine.stats.useNewSnapping)
        {
            SetState(new LadderSnap(this));
            playerStateMachine.ladderState = PlayerMovementStateMachine.LadderState.LadderUnfold;
        }
        else 
        {
            SetState(new LadderSnap(this));
            playerStateMachine.ladderState = PlayerMovementStateMachine.LadderState.LadderSnap;
        }
    }
    public void OnLadderPush()
    {
        SetState(new LadderPush(this));
        playerStateMachine.ladderState = PlayerMovementStateMachine.LadderState.LadderPush;
    }
    public void OnLadderPushEnd()
    {
        SetState(new LadderFold(this));
        playerStateMachine.ladderState = PlayerMovementStateMachine.LadderState.LadderFold;
    }

    #endregion
}
