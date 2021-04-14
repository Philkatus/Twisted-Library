using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState
{
    protected PlayerMovementStateMachine PlayerStateMachine;

    public PlayerState(PlayerMovementStateMachine playerStateMachine)
    {
        PlayerStateMachine = playerStateMachine;
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

    public virtual IEnumerator Finish()
    {
        yield return null;
    }

    public virtual void Gravitation()
    {

    }
}
