using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSliding : PlayerState
{
    float speed;
    float FowardInput;
    float SideWardsInput;
    float closestShelf;
    float possibleShelfs;
    CharacterController controller; 
    //Wie macht man parent Swap (Braucht die Leiter und die Player dings)
    // 

    public override IEnumerator initialize()
    {
        // Zuweisungen
        speed = PlayerStateMachine.speed;
        FowardInput = PlayerStateMachine.FowardInput;
        SideWardsInput = PlayerStateMachine.SideWardsInput;
        closestShelf = PlayerStateMachine.closestShelf;
        possibleShelfs = PlayerStateMachine.possibleShelfs;
        controller = PlayerStateMachine.controller;

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
        yield break;
    }

    public override IEnumerator Movement()
    {
        //
        // WS - Leiter hoch/runter
        // speed = 1
        // Man erreicht das Ende der Leiter: kleiner Timer
        // Man kommt oben an: OnLadderTop.trigger
        //OnLadderShrink.trigger
        //Man kommt unten an: OnLadderBottom.trigger
        //OnLadderShrink.trigger
        //AD - Momentum aufbauen
        //speed = 1.5(hat eine max.speed)
        //Behält die Geschwindigkeit, keine friction
        //(low prio) Kopf in die Input Richtung und dann Bewegungs Richtung zeigen

        yield break;
    }

    public override IEnumerator Snap()
    {
        yield break;
    }

    public PlayerSliding(PlayerMovementStateMachine playerStateMachine)
        : base(playerStateMachine) 
    {

    }
}
