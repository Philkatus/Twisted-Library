using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSliding : PlayerState
{
    #region INHERITED
    float speed;
    float FowardInput;
    float SideWardsInput;
    Shelf closestShelf;
    CharacterController controller;
    #endregion
    #region PRIVATE
    float climbingSpeed;
    #endregion


    //Wie macht man parent Swap (Braucht die Leiter und die Player dings)

    public override IEnumerator initialize()
    {
        // Zuweisungen
        speed = PlayerStateMachine.speed;
        FowardInput = PlayerStateMachine.FowardInput;
        SideWardsInput = PlayerStateMachine.SideWardsInput;
        closestShelf = PlayerStateMachine.closestShelf;
        controller = PlayerStateMachine.controller;

        climbingSpeed = 1.3f;
        // PC auf Leiter setzen

        // Parent Swap => Leiter ist Parent

        yield break;
    }

    public override IEnumerator Finish()
    {
        // Parent Swap => Player ist Parent
        yield break;
    }

    public override IEnumerator GroundDetection()
    {
        yield break;
    }

    public override IEnumerator Jump()
    {
        //Ein Sprung
        //speed = 1.3
        //OnFall.trigger
        //OnLadderShrink.trigger

        yield break;
    }

    public override IEnumerator Movement()
    {
        // WS - Leiter hoch/runter
        // speed = 1
        // Man erreicht das Ende der Leiter: kleiner Timer

        // Man kommt oben an: OnLadderTop.trigger
        //OnLadderShrink.trigger

        //Man kommt unten an: OnLadderBottom.trigger
        //OnLadderShrink.trigger

        //(low prio) Kopf in die Input Richtung und dann Bewegungs Richtung zeigen
        yield break;
    }

    public PlayerSliding(PlayerMovementStateMachine playerStateMachine)
        : base(playerStateMachine) 
    {

    }
}
