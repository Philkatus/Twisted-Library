using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PlayerSliding : State
{
    #region INHERITED
    float currentDistance;
    float speed;
    float ladderLength;
    Shelf closestShelf;
    CharacterController controller;
    PathCreator pathCreator;
    PlayerMovementStateMachine pSM;
    LadderStateMachine ladder;
    LadderSizeStateMachine ladderSizeState;
    
    #endregion
    #region PRIVATE
  
    #endregion


    //Wie macht man parent Swap (Braucht die Leiter und die Player dings)

    public override IEnumerator Initialize()
    {
        // Zuweisungen
        pSM = PlayerStateMachine;
        ladderSizeState = pSM.ladderSizeStateMachine;
        ladderLength = ladderSizeState.ladderLengthBig;
        speed = pSM.speedOnLadder;
        closestShelf = pSM.closestShelf;
        controller = pSM.controller;
        ladder = pSM.ladderStateMachine;
        pathCreator = closestShelf.pathCreator;
        pSM.HeightOnLadder = -1;


        //Leiter auf den path setzen

        Vector3 startingPoint = pathCreator.path.GetClosestPointOnPath(pSM.transform.position);
        currentDistance = pathCreator.path.GetClosestDistanceAlongPath(startingPoint);
        ladder.transform.position = startingPoint;
        ladder.transform.forward = -pathCreator.path.GetNormalAtDistance(currentDistance);
        Debug.Log(ladder.transform.position);

        // PC auf Leiter setzen = > WIE KOMM ICH AN DEN CHARACTER RAN?
        ladder.transform.parent = null;
        Vector3 targetPosition = startingPoint + ladder.direction * ladderLength;
        targetPosition.y = Mathf.Clamp(targetPosition.y, ladder.direction.y*ladderLength, controller.transform.position.y);
        controller.transform.position = targetPosition;
        pSM.HeightOnLadder = -(startingPoint - targetPosition).magnitude/ladderLength;

        controller.transform.forward = -pathCreator.path.GetNormalAtDistance(currentDistance);
        controller.transform.parent = ladder.transform;
        pSM.ladderSizeStateMachine.OnGrow();


        // Parent Swap () => Leiter ist Parent

        yield return null;
    }

    public override IEnumerator Finish()
    {
        // Parent Swap () => Player ist Parent
        controller.transform.parent = null;
        ladder.transform.localPosition = new Vector3(4, 0, 0);
        ladder.transform.parent = controller.transform;



        pSM.ladderSizeStateMachine.OnShrink();
        
        yield break;
    }

    public override void Jump()
    {
        //Ein Sprung 
        //eine speed mitgeben????
        PlayerStateMachine.playerVelocity.y = PlayerStateMachine.jumpheight;
        PlayerStateMachine.OnFall();
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

        pSM.transform.position = ladder.transform.position + ladder.direction * ladderLength * pSM.HeightOnLadder;
        
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
