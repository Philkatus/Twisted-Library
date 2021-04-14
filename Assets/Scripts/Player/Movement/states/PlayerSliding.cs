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
        /*speed = PlayerStateMachine.speed;
        FowardInput = PlayerStateMachine.FowardInput;
        SideWardsInput = PlayerStateMachine.SideWardsInput;
        closestShelf = PlayerStateMachine.closestShelf;
        controller = PlayerStateMachine.controller;

        climbingSpeed = 1.3f;*/

        // PC auf Leiter setzen = > WIE KOMM ICH AN DEN CHARACTER RAN?

        // Parent Swap () => Leiter ist Parent

        yield break;
    }

    public override IEnumerator Finish()
    {
        // Parent Swap () => Player ist Parent
        yield break;
    }

    public override IEnumerator Jump()
    {
        //Ein Sprung 
        //eine speed mitgeben????
        //OnFall.trigger
        //OnLadderShrink.trigger

        yield break;
    }

    public override IEnumerator Movement()
    {
        while (true)
        {
            //An der Leiter Hoch und runter bewegen
            /*controller.Move(new Vector3(0, speed * FowardInput, 0)); // muss 0 durch was anderes ersetzt werden??

            //(low prio) Kopf in die Input Richtung und dann Bewegungs Richtung zeigen
            //KopfReference

            // if( Man erreicht das Ende der Leiter )
            {// kleiner Timer
                if (//timer abgelaufen)
                if (//weg nach oben)
                {
                        //OnLadderTop.trigger
                        //OnLadderShrink.trigger
                    }
                    else (//weg nach unten)
                {
                    //OnLadderBottom.trigger
                    //OnLadderShrink.trigger
                }
            }
            // else ()
            // { timer = 0; }*/
            yield return new WaitForEndOfFrame();
        }

        yield break;
    }

    public PlayerSliding(PlayerMovementStateMachine playerStateMachine)
        : base(playerStateMachine)
    {

    }
}
