using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    protected State State;

    public void SetState(State state)
    {
        if (State != null)
        {
            StartCoroutine(State.Finish());
        }

        if(State == state)
        {
            State.ReInitialize();
        }
        else
        {
            State = state;
            State.Initialize();
        }
        
    }
}
