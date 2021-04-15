using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LadderState 
{
    LadderStateMachine LadderStateMachine;
    public LadderState(LadderStateMachine ladderStateMachine)
    {
       LadderStateMachine = ladderStateMachine;
    }


    public virtual void Movement() 
    {

    }




}
