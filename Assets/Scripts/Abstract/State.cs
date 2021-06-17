using System.Collections;
using System;
using UnityEngine.InputSystem;
using UnityEngine;

public abstract class State
{
    protected PlayerMovementStateMachine PSM;
    protected LadderSizeStateMachine LadderSizeStateMachine;
    protected Vector3 clamp;


    public State(PlayerMovementStateMachine playerStateMachine)
    {
        PSM = playerStateMachine;
        LadderSizeStateMachine = playerStateMachine.ladderSizeStateMachine;
    }

    public State(LadderSizeStateMachine ladderSizeStateMachine)
    {
        LadderSizeStateMachine = ladderSizeStateMachine;
        PSM = ladderSizeStateMachine.playerStateMachine;
    }

    public virtual void Initialize()
    {

    }

    public virtual void ReInitialize()
    {

    }

    public virtual void Movement()
    {

    }

    public virtual void Jump()
    {

    }
    public virtual void Fold()
    {

    }

    public virtual void Snap()
    {

    }

    public virtual void Swing()
    {

    }

    public virtual void LadderPush()
    {

    }

    public virtual void FallFromLadder()
    {

    }

    public virtual IEnumerator ChangeLadderSize()
    {
        yield return null;
    }

    public virtual IEnumerator Finish()
    {
        yield return null;
    }
}
