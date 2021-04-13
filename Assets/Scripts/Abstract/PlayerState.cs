using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState
{
    protected Player_Movement_StateMachine PlayerStateMachine;

    public PlayerState(Player_Movement_StateMachine playerStateMachine) 
    {
        playerStateMachine = PlayerStateMachine;
    }

    public virtual IEnumerator initialize()
    {
        yield return null;
    }

    public virtual IEnumerator Movement()
    {
        yield return null;
    }

    public virtual IEnumerator Jump()
    {
        yield return null;
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
}
