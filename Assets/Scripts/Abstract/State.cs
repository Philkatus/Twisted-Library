using System.Collections;

public abstract class State
{
    protected PlayerMovementStateMachine PlayerStateMachine;
    protected LadderSizeStateMachine LadderSizeStateMachine;

    public State(PlayerMovementStateMachine playerStateMachine)
    {
        PlayerStateMachine = playerStateMachine;
    }

    public State(LadderSizeStateMachine ladderSizeStateMachine)
    {
        LadderSizeStateMachine = ladderSizeStateMachine;
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

    public virtual void RocketJump()
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


    public virtual IEnumerator ChangeLadderSize()
    {
        yield return null;
    }

    public virtual IEnumerator Finish()
    {
        yield return null;
    }
}
