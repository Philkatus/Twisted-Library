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
        State = state;
        StartCoroutine(State.Initialize());
    }
}
