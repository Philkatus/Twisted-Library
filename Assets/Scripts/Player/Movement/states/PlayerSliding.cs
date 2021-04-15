using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PlayerSliding : PlayerState
{
    #region INHERITED
    float currentDistance;
    float speed;
    float FowardInput;
    float SideWardsInput;
    Shelf closestShelf;
    CharacterController controller;
    PathCreator pathCreator;
    PlayerMovementStateMachine pSM;
    LadderStateMachine ladder;
    #endregion
    #region PRIVATE
  
    #endregion


    //Wie macht man parent Swap (Braucht die Leiter und die Player dings)

    public override IEnumerator Initialize()
    {
        // Zuweisungen
        pSM = PlayerStateMachine;
        speed = pSM.speedOnLadder;
        closestShelf = pSM.closestShelf;
        controller = pSM.controller;
        ladder = pSM.ladderScript;
        pathCreator = closestShelf.pathCreator;
        pSM.HeightOnLadder = -1;


        //Leiter auf den path setzen

        Vector3 startingPoint = pathCreator.path.GetClosestPointOnPath(pSM.transform.position);
        currentDistance = pathCreator.path.GetClosestDistanceAlongPath(startingPoint);
        ladder.transform.position = startingPoint;
        ladder.transform.forward = -pathCreator.path.GetNormalAtDistance(currentDistance);

        // PC auf Leiter setzen = > WIE KOMM ICH AN DEN CHARACTER RAN?
        ladder.transform.parent = null;
        controller.transform.position = startingPoint+ladder.direction * ladder.length;

        controller.transform.forward = -pathCreator.path.GetNormalAtDistance(currentDistance);
        controller.transform.parent = ladder.transform;



        // Parent Swap () => Leiter ist Parent

        yield return null;
    }

    public override IEnumerator Finish()
    {
        // Parent Swap () => Player ist Parent
        yield break;
    }

    public override void Jump()
    {
        //Ein Sprung 
        //eine speed mitgeben????
        //OnFall.trigger
        //OnLadderShrink.trigger

        //yield break;
    }

    public override void Movement()
    {

        //An der Leiter Hoch und runter bewegen
        //controller.Move(new Vector3(0, speed * FowardInput, 0)); // muss 0 durch was anderes ersetzt werden??
        pSM.HeightOnLadder += pSM.ForwardInput * speed*Time.deltaTime;
        pSM.HeightOnLadder = Mathf.Clamp(pSM.HeightOnLadder, -1, 0);

        pSM.transform.position = ladder.transform.position + ladder.direction * ladder.length * pSM.HeightOnLadder;
            //(low prio) Kopf in die Input Richtung und dann Bewegungs Richtung zeigen
            //KopfReference

            // if( Man erreicht das Ende der Leiter )
            // kleiner Timer
                //timer abgelaufen)
                //weg nach oben)
                
                        //OnLadderTop.trigger
                        //OnLadderShrink.trigger
                    
                    //weg nach unten)
                
                    //OnLadderBottom.trigger
                    //OnLadderShrink.trigger
                
            
            // else ()
            // { timer = 0; }*/
            // yield return new WaitForEndOfFrame();
        

        //yield break;
    }

    public PlayerSliding(PlayerMovementStateMachine playerStateMachine)
        : base(playerStateMachine)
    {

    }
}
