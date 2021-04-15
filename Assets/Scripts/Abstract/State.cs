using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    protected PlayerMovementStateMachine PlayerStateMachine;
    protected LadderStateMachine LadderStateMachine;
    protected LadderSizeStateMachine LadderSizeStateMachine;

    public State(PlayerMovementStateMachine playerStateMachine)
    {
        PlayerStateMachine = playerStateMachine;
    }

    public State(LadderStateMachine ladderStateMachine)
    {
        LadderStateMachine = ladderStateMachine;
    }

    public State(LadderSizeStateMachine ladderSizeStateMachine)
    {
        LadderSizeStateMachine = ladderSizeStateMachine;
    }

    public virtual IEnumerator Initialize()
    {
        yield return null;
    }

    public virtual void Movement()
    {

    }

    public virtual void Jump()
    {

    }

    public virtual IEnumerator Snap()
    {
        yield return null;
    }

    public virtual IEnumerator GroundDetection()
    {
        yield return null;
    }

    public virtual IEnumerator ChangeLadderSize()
    {
        yield return null;
    }

    public virtual IEnumerator Finish()
    {
        yield return null;
    }

    public virtual void Gravitation()
    {

    }
}
