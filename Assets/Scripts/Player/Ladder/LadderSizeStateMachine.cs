using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderSizeStateMachine : StateMachine
{
    #region public
    public float ladderLengthBig;
    public float ladderLengthSmall;
    public Transform ladderParent;

    #endregion

    #region private

    #endregion

    private void Start()
    {
        SetState(new LadderBig(this));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartCoroutine(State.ChangeLadderSize());
        }
    }

    #region Functions to change the State
    ///<summary>
    /// Is called when the ladder expands.
    ///</summary>
    public void OnGrow()
    {
        SetState(new LadderBig(this));
    }

    ///<summary>
    /// Is called when the ladder shortens.
    ///</summary>
    public void OnShrink()
    {
        SetState(new LadderSmall(this));
    }
    #endregion
}
