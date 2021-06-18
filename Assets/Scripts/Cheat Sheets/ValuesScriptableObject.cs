using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ValuesScriptableObject", order = 1)]
public class ValuesScriptableObject : ScriptableObject
{
    #region general
    [Header("Enable Features")]
    [Space]
    [Tooltip("Use the button used for jumping instead of the fold button for the ladder shoot.")]
    public bool useJumpForLadderShoot;

    [Tooltip("Use the trigger buttons to snap and slide in a specific direction.")]
    public bool useTriggerToSlideWithMomentum;

    [Space]
    [Header("General")]
    [Tooltip("How fast the bonusvelocity is lost again")]
    public float bonusVelocityDrag;

    [Space]
    [Header("Input Coyote Timer")]
    [Tooltip("Determines how long the jump input gets saved.")]
    public float jumpInputTimer;

    [Tooltip("Determines how long the snap input gets saved.")]
    public float snapInputTimer;

    [Tooltip("Determines how long the fold input gets saved.")]
    public float foldInputTimer;

    [Tooltip("Determines how long the swing input gets saved.")]
    public float swingInputTimer;

    [Tooltip("How long is the time after falling of a ladder where you can still jump.")]
    public float slidingCoyoteTime = 0.2f;
    #endregion
    [Space]
    #region Ability bools
    [Header("Ability bools")]
    [Tooltip("Is the player able to perform a ladderPush")]
    public bool canLadderPush = true;
    [Tooltip("Is the player able to perform a CatapultJump")]
    public bool canLadderFold = true;
    [Tooltip("Is the player able to Slide")]
    public bool canSlide = true;
    #endregion
    [Space]
    #region movement and walking
    [Header("Movement/Walking")]
    [Tooltip("How fast the player accelerates to maximum speed while walking.")]
    public float movementAcceleration;
    public float MovementAcceleration
    {
        get
        {
            return movementAcceleration * movementVelocityFactor;
        }
    }

    [Tooltip("How fast the player can move while walking and in the air at max.")]
    public float maximumMovementSpeed;

    public float MaximumMovementSpeed
    {
        get
        {
            return maximumMovementSpeed * movementVelocityFactor;
        }
    }

    [Tooltip("How much drag is applied when there is no input while walking.")]
    public float movementDrag;
    public float MovementDrag
    {
        get
        {
            return movementDrag * movementVelocityFactor;
        }
    }

    [Tooltip("How much percentag drag to the bonusvelocity is applied while walking.")]
    [Range(0, 100)]
    public float walkingBonusVelocityDrag = 50;
    public float WalkingBonusVelocityDrag
    {
        get
        {
            return walkingBonusVelocityDrag * movementVelocityFactor;
        }
    }

    [Tooltip("The factor to convert Velocity into Movementspeed")]
    public float movementVelocityFactor = 1;
    #endregion
    [Space]
    #region jumping and air movement
    [Header("Jumping/Air Movement")]
    [Tooltip("How high the player jumps.")]
    public float jumpHeight;
    public float JumpHeight
    {
        get
        {
            return jumpHeight * AirVelocityFactor;
        }
    }

    [Tooltip("To put a clamp on the upwards velocity")]
    public float maxJumpingSpeed;
    public float MaxJumpingSpeedUp
    {
        get
        {
            return maxJumpingSpeed * AirVelocityFactor;
        }
    }

    [Tooltip("To put a clamp on the fowards and sidewards velocity")]
    public float maxJumpingSpeedForward;
    public float MaxJumpingSpeedForward
    {
        get
        {
            return maxJumpingSpeedForward * AirVelocityFactor;
        }
    }

    [Tooltip("How fast the player accelerates with the rocketJump.")]
    public float ladderPushAcceleration;
    public float LadderPushAcceleration
    {
        get
        {
            return ladderPushAcceleration * AirVelocityFactor;
        }
    }
    [Tooltip("factor to controll how much the current bonus Velocity factors into the end reVelocity" +
        "higher values mean the curretn velocity doesn't get changed much")]
    [Range(1,10)] public float ladderPushCurrentVelocityFactor;

    [Tooltip("Direction of the jump when facing the wall.")]
    public Vector3 jumpFromLadderDirection;

    [Tooltip("Limits the movement speed for the air movement.")]
    public float airMovementAcceleration;
    public float AirMovementAcceleration
    {
        get
        {
            return airMovementAcceleration * AirVelocityFactor;
        }
    }

    [Tooltip("How much drag in the air is applied while no input is given")]
    public float jumpingDrag;
    public float JumpingDrag
    {
        get
        {
            return jumpingDrag * AirVelocityFactor;
        }
    }
    [Tooltip("...is working against me.")]
    public float gravity;
    public float Gravity
    {
        get
        {
            return gravity * AirVelocityFactor;
        }
    }
    [Tooltip("how long do you float at the top before fallign down agai")]
    public float floatTime =1;

    [Tooltip("even gravity has its limits.")]
    public float maxFallingSpeed;
    public float MaxFallingSpeed
    {
        get
        {
            return maxFallingSpeed * AirVelocityFactor;
        }
    }

    [Tooltip("The factor to convert Velocity into air-Movementspeed")]
    public float AirVelocityFactor = 1;

    #endregion
    [Space]
    #region Sliding
    [Header("Sliding")]
    [Tooltip("The maximum distance between ladder and rail to snap to the rails.")]
    public float snappingDistance = 10;

    [Tooltip("The maximum distance between ladder and rail to snap to the next rail while sliding.")]
    public float resnappingDistance = .5f;

    [Tooltip("The angle from pathDorection to camera.forward which enables a special case where the button used to snap is saved and used to slide forward.")]
    public float specialCaseAngleForSlidingInput = 20f;

    [Tooltip("When this angle between pathDorection and camera.forward is exceeded, the buttons determining the sliding direction depend on the camera again.")]
    public float angleToLeaveSpecialCaseSlindingInput = 45f;

    [Tooltip("When this angle between pathDorection and camera.forward is exceeded, the sliding direction will be reversed to match the perspective.")]
    public float fromAngleForAdjustedSlidingDirection = 45f;

    [Tooltip("When this angle between pathDorection and camera.forward is exceeded, the sliding direction will be normal again.")]
    public float toAngleForAdjustedSlidingDirection = 135f;

    [Tooltip("The minumum player velocity needed to influence the snap direction.")]
    public float minVelocityToChangeSnapDirection = 1;

    [Tooltip("How fast the player climbs the ladder up and down while sliding.")]
    public float climbingSpeedOnLadder;

    [Tooltip("The factor that determines how much the height on the ladder changes the velocity of the rail catapult jump.")]
    [Range(0f, 1)] public float heightOnLadderKatapulFactor = .2f;

    [Space]
    [Tooltip("The (animation) speed for the player coming off the ladder on the top while sliding.")]
    public float ladderDismountSpeed;

    [Tooltip("How long the player needs to hold the key to dismount the ladder on top or bottom while sliding.")]
    public float ladderDismountTimer;

    [Tooltip("If true the player preserves their velocity on snap.")]
    public bool preservesVelocityOnSnap = false;

    [Tooltip("The percentage of velocity the player has when falling at the end of a rail that gets added as bonus velocity. 1 = 100%")]
    public float fallingMomentumPercentage;

    [Tooltip("The offset between the ladder and the player, so that the position on the ladder is right.")]
    public float playerOffsetFromLadder;

    [Tooltip("The sliding speed range is from zero to maxSlidingSpeed.")]
    public float maxSlidingSpeed;

    [Tooltip("How fast the player accelerates to maxSlidingSpeed while pressing the button completely.")]
    public float slidingTimeToAccecelerate = 0.7f;

    [Tooltip("How fast the player decelarates to a halt while sliding and pressing the button completely.")]
    public float slidingTimeToDecelerate = 0.2f;

    [Tooltip("The time needed for the player to slow down and start sliding in the opposite direction.")]
    public float timeToSwitchDirection;

    [Tooltip("The time players need to wait before accelerating again.")]
    public float accelerationCooldown;

    #endregion
    [Space]
    #region swinging
    [Header("Swinging")]

    [Tooltip("How fast the player accelerates while swinging.")]
    public float swingingAcceleration;

    [Tooltip("How fast the player decelerates while swinging with reverse input. Minimum deceleration.")]
    public float minSwingingDeceleration;

    [Tooltip("How fast the player decelerates while swinging with reverse input. Maximum deceleration.")]
    public float maxSwingingDeceleration;

    [Tooltip("How fast the player decelerates while hanging from the rail. Minimum deceleration.")]
    public float minHangingDeceleration;

    [Tooltip("How fast the player decelerates while hanging from the rail. Maximum deceleration.")]
    public float maxHangingDeceleration;

    [Tooltip("A factor that restricts the acceleration while hanging from the lowest rail.")]
    [Range(0, 1)]
    public float hangingAccelerationFactor;

    [Tooltip("Maximum angle before snap from center while pushing form the rail for swinging.")]
    public float maxPushAngle;

    [Tooltip("The maximum speed the player can reach while swinging.")]
    public float maxSwingSpeed;

    [Tooltip("How fast the player decelerates while swinging without giving an input.")]
    public float swingingGravity;

    #endregion

    #region ladder
    [Space]
    [Header("Ladder")]
    [Tooltip("The length of the ladder while extended.")]
    public float ladderLengthBig = 4;

    [Tooltip("The length of the ladder while retracted.")]
    public float ladderLengthSmall = 1.25f;

    [Tooltip("How long it takes for the ladder to fold.")]
    public float foldingTime = 0.2f;

    [Tooltip("Additional time after foldingTime until folding ends.")]
    public float extraFoldingTime = 0.5f;

    [Tooltip("How long it takes for the ladder to Unfold on snap.")]
    public float onSnapUnFoldingTime = 0.5f;

    [Tooltip("The factor by which the jumping speed is mutliplied when a rail catapult jump is performed.")]
    public float railCatapultJumpMultiplier = 17;
    public float RailCatapultJumpMultiplier
    {
        get
        {
            return railCatapultJumpMultiplier * AirVelocityFactor;
        }

    }

    [Tooltip("The factor by which the jumping speed is mutliplied when a reversed rail catapult jump is performed.")]
    public float reversedRailCatapultJumpMultiplier;
    public float ReversedRailCatapultJumpMultiplier
    {
        get
        {
            return reversedRailCatapultJumpMultiplier * AirVelocityFactor;
        }

    }
    #endregion
}
