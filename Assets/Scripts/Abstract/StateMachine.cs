using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    protected State State;

    public void SetState(State state)
    {
        if (State == state)
        {
            State.ReInitialize();
        }
        else
        {
            if (State != null)
            {
                StartCoroutine(State.Finish());
            }
            State = state;
            State.Initialize();
        }
    }
}
