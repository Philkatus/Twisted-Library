using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine :MonoBehaviour
{
    protected PlayerState State;

    public void SetState(PlayerState state) 
    {
        State = state;
        StartCoroutine(State.initialize());
    }



}
