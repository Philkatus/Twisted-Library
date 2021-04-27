using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInTheAir : State
{
    CharacterController controller;
    ValuesScriptableObject values;

    float wallJumpingTime;

    public PlayerInTheAir(PlayerMovementStateMachine playerStateMachine) : base(playerStateMachine)
    {

    }

    public override void Initialize()
    {
        controller = PlayerStateMachine.controller;
        controller.transform.parent = PlayerStateMachine.myParent;
        PlayerStateMachine.ladder.transform.parent = controller.transform;
        PlayerStateMachine.ladder.localPosition = PlayerStateMachine.ladderWalkingPosition;
        PlayerStateMachine.ladder.localRotation = PlayerStateMachine.ladderWalkingRotation;
        values = PlayerStateMachine.valuesAsset;
        controller = PlayerStateMachine.controller;
        PlayerStateMachine.playerVelocity.y = Mathf.Clamp(PlayerStateMachine.playerVelocity.y, 0, Mathf.Infinity);

        wallJumpingTime = 0;
    }

    public override void Movement()
    {
        // Air Movement
        Transform cam = Camera.main.transform;
        PlayerMovementStateMachine pSM = PlayerStateMachine;
        Vector3 directionForward = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        Vector3 directionRight = new Vector3(cam.right.x, 0, cam.right.z).normalized;
        Vector3 direction = directionForward * pSM.forwardInput + directionRight * pSM.sideWaysInput; 

        if (pSM.slidingInput != 0 || pSM.swingingInput != 0)
        {
            pSM.TryToSnapToShelf();
        }

        if (direction != Vector3.zero)
        {
            controller.transform.forward = direction;
        }
        pSM.playerVelocity += direction * Time.deltaTime * values.movementAcceleration * values.airMovementFactor;

        //when wall jump occured, set the isWallJumping to false after 1 sec
        wallJumpingTime += Time.deltaTime;
        if(wallJumpingTime >= 1)
        {
            pSM.isWallJumping = false;
        }

        if (pSM.forwardInput <= 0.3f && pSM.forwardInput >= -.3f && !pSM.isWallJumping)
        {
            Vector3 currentDragForward = values.jumpingDrag * pSM.resultingVelocity(pSM.playerVelocity, directionForward) / values.airMovementFactor;
            pSM.playerVelocity -= currentDragForward * Time.deltaTime;
        }
        if (pSM.sideWaysInput <= 0.3f && pSM.sideWaysInput >= -.3f && !pSM.isWallJumping)
        {
            Vector3 currentDragSideways = values.jumpingDrag * pSM.resultingVelocity(pSM.playerVelocity, directionRight) / values.airMovementFactor;
            pSM.playerVelocity -= currentDragSideways * Time.deltaTime;
        }
        pSM.playerVelocity.y -= values.gravity * Time.deltaTime;
        /*
        float currentDrag = pSM.movementDrag + pSM.playerVelocity.magnitude * .999f;
        pSM.playerVelocity.x = pSM.playerVelocity.normalized.x * Mathf.Clamp(pSM.playerVelocity.magnitude - currentDrag * Time.deltaTime, 0, pSM.maximumSpeed);
        pSM.playerVelocity.z = pSM.playerVelocity.normalized.z * Mathf.Clamp(pSM.playerVelocity.magnitude - currentDrag * Time.deltaTime, 0, pSM.maximumSpeed);
        */
        float unClampedVelocityY = pSM.playerVelocity.y;
        pSM.playerVelocity = pSM.playerVelocity.normalized * Mathf.Clamp(pSM.playerVelocity.magnitude, 0, values.maximumMovementSpeed);
        pSM.playerVelocity.y = unClampedVelocityY;

        controller.Move(pSM.playerVelocity * Time.deltaTime);
        // Gravity and falling

        //pSM.playerVelocity += direction * Time.deltaTime * pSM.movementAcceleration * pSM.jumpMovementFactor;
        /*
        float currentDrag = pSM.movementDrag + pSM.playerVelocity.magnitude * .999f;
        pSM.playerVelocity.x = pSM.playerVelocity.normalized.x * Mathf.Clamp(pSM.playerVelocity.magnitude - currentDrag * Time.deltaTime, 0, pSM.maximumSpeed);
        pSM.playerVelocity.z = pSM.playerVelocity.normalized.z * Mathf.Clamp(pSM.playerVelocity.magnitude - currentDrag * Time.deltaTime, 0, pSM.maximumSpeed);
        */
        //controller.Move(pSM.playerVelocity * Time.deltaTime);

        if (controller.isGrounded)
        {
            pSM.OnLand();
        }
    }

    public override void Snap()
    {
        PlayerStateMachine.OnSnap();
        
    }
}
