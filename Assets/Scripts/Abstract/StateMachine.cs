using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    protected State State;

    public void SetState(State state)
    {
        StartCoroutine(State.Finish());
        State = state;
        StartCoroutine(State.Initialize());
    }



}
