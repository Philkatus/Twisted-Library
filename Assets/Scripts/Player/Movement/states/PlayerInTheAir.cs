using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInTheAir : PlayerState 
{
    public PlayerInTheAir(PlayerMovementStateMachine playerStateMachine)
       : base(playerStateMachine)
    {

    }

    public override void Jump()
    {
        Debug.Log("jump");
    }
}
