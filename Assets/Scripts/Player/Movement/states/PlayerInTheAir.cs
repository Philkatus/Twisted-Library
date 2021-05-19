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
        controller.transform.SetParent(PlayerStateMachine.myParent);
        PlayerStateMachine.ladder.transform.localScale = new Vector3(1, 1, 1);
        controller.transform.localScale = new Vector3(1, 1, 1);
        PlayerStateMachine.ladder.transform.SetParent(PlayerStateMachine.animController.spine);
        PlayerStateMachine.ladder.localPosition = PlayerStateMachine.ladderWalkingPosition;
        PlayerStateMachine.ladder.localRotation = PlayerStateMachine.ladderWalkingRotation;



        values = PlayerStateMachine.valuesAsset;
        controller = PlayerStateMachine.controller;

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

        /* Philips snapping
        if (pSM.slidingInput != 0 || pSM.swingingInput != 0)
        {
            pSM.TryToSnapToShelf();
        }
        */

        if (direction != Vector3.zero)
        {
            controller.transform.forward = direction;
        }
        pSM.baseVelocity += direction * Time.fixedDeltaTime * values.movementAcceleration * values.airMovementFactor;

        //when wall jump occured, set the isWallJumping to false after 1 sec
        wallJumpingTime += Time.fixedDeltaTime;
        if (wallJumpingTime >= 1)
        {
            pSM.isWallJumping = false;
        }
        /*
        if (pSM.forwardInput <= 0.1f && pSM.forwardInput >= -.1f && !pSM.isWallJumping)
        {
            Vector3 currentDragForward = values.jumpingDrag * pSM.resultingVelocity(pSM.playerVelocity, directionForward) / values.airMovementFactor;
            pSM.baseVelocity -= currentDragForward * Time.fixedDeltaTime;
        }
        if (pSM.sideWaysInput <= 0.1f && pSM.sideWaysInput >= -.1f && !pSM.isWallJumping)
        {
            Vector3 currentDragSideways = values.jumpingDrag * pSM.resultingVelocity(pSM.playerVelocity, directionRight) / values.airMovementFactor;
            pSM.baseVelocity -= currentDragSideways * Time.fixedDeltaTime;
        }
        */
        pSM.baseVelocity.y -= values.gravity * Time.fixedDeltaTime;
        float ClampedVelocityY = Mathf.Clamp(pSM.baseVelocity.y, -values.maxFallingSpeed, values.maxJumpingSpeed);
        pSM.baseVelocity = pSM.baseVelocity.normalized * Mathf.Clamp(pSM.baseVelocity.magnitude, 0, values.maximumMovementSpeed);
        pSM.baseVelocity.y = ClampedVelocityY;


        controller.Move(pSM.playerVelocity * Time.fixedDeltaTime * values.jumpVelocityFactor);
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
            PlayerStateMachine.isRocketJumping = false;
            pSM.OnLand();
        }
    }

    public override void Snap()
    {
        PlayerStateMachine.isRocketJumping = false;
        PlayerStateMachine.OnSnap();

    }

    public override void RocketJump()
    {
        if (!PlayerStateMachine.isRocketJumping)
        {
            Debug.Log("Rocket");
            float sphereRadius = .2f;
            float MaxHeight = PlayerStateMachine.ladderSizeStateMachine.ladderLengthBig - sphereRadius;
            float acceleration = values.rocketJumpAcceleration;
            Vector3 origin = PlayerStateMachine.transform.position;



            LayerMask mask = LayerMask.GetMask("Environment");

            List<RaycastHit> hits = new List<RaycastHit>();
            Ray ray = new Ray(origin, Vector3.down);
            //hits.AddRange( Physics.SphereCastAll(ray, MaxHeight, 1, mask));
            hits.AddRange(Physics.SphereCastAll(origin, sphereRadius, Vector3.down, MaxHeight, mask, QueryTriggerInteraction.Ignore));
            hits.AddRange(Physics.SphereCastAll(origin, sphereRadius, Vector3.down + Vector3.forward, MaxHeight, mask, QueryTriggerInteraction.Ignore));
            hits.AddRange(Physics.SphereCastAll(origin, sphereRadius, Vector3.down + Vector3.back, MaxHeight, mask, QueryTriggerInteraction.Ignore));
            hits.AddRange(Physics.SphereCastAll(origin, sphereRadius, Vector3.down + Vector3.right, MaxHeight, mask, QueryTriggerInteraction.Ignore));
            hits.AddRange(Physics.SphereCastAll(origin, sphereRadius, Vector3.down + Vector3.left, MaxHeight, mask, QueryTriggerInteraction.Ignore));


            float closestDistance = Mathf.Infinity;
            RaycastHit closestHit;
            Vector3 target = Vector3.zero;

            for (int i = 0; i < hits.Count; i++)
            {

                float distance = hits[i].distance;
                if (distance < closestDistance)
                {

                    closestHit = hits[i];
                    closestDistance = distance;
                    target = closestHit.point;

                }

            }

            if (target != Vector3.zero)
            {
                PlayerMovementStateMachine pSM = PlayerStateMachine;
                pSM.ladderJumpTarget = target;
                pSM.baseVelocity.y = 0;
                //pSM.baseVelocity = pSM.resultingVelocity(pSM.playerVelocity, (pSM.transform.position - target).normalized);
                pSM.bonusVelocity = (pSM.transform.position - target).normalized * acceleration;
                Debug.DrawLine(PlayerStateMachine.transform.position, target, Color.white, 5);
                PlayerStateMachine.isRocketJumping = true;
                pSM.ladderSizeStateMachine.OnRocketJump();
            }


        }


    }
}
