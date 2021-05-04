using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ValuesScriptableObject", order = 1)]
public class ValuesScriptableObject : ScriptableObject
{
    #region movement and walking
    [Header("Movement/Walking")]
    [Tooltip("How fast the player accelerates to maximum speed while walking.")]
    public float movementAcceleration;

    [Tooltip("How fast the player can move while walking and in the air at max.")]
    public float maximumMovementSpeed;

    [Tooltip("How much drag is applied when there is no input while walking.")]
    public float movementDrag;

    [Tooltip("The factor to convert Velocity into Movementspeed")]
    public float movementVelocityFactor = 1;

    #endregion
    [Space]
    #region jumping and air movement
    [Header("Jumping/Air Movement")]
    [Tooltip("How high the player jumps.")]
    public float jumpHeight;
    [Tooltip("Direction of the jump when facing the wall.")]
    public Vector3 wallJump;

    [Tooltip("Limits the movement speed for the air movement.")]
    [Range(.1f, 1)] public float airMovementFactor;
    public float jumpingDrag;

    [Tooltip("...is working against me.")]
    public float gravity;

    [Tooltip("even gravity has its limits.")]
    public float maxFallingSpeed = 40;

    [Tooltip("The factor to convert Velocity into air-Movementspeed")]
    public float jumpVelocityFactor = 1;
    #endregion
    [Space]
    #region Sliding
    [Header("Sliding")]
    [Tooltip("The maximum distance between ladder and rail to snap to the rails.")]
    public float snappingDistance;

    [Tooltip("How fast the player climbs the ladder up and down while sliding.")]
    public float climbingSpeedOnLadder;

    [Tooltip("How fast the player accelerates to maximum sliding speed.")]
    public float slidingAcceleration;
    public float maxSlidingSpeed, slidingSpeedSizeFactor;

    [Tooltip("How fast the player comes to a halt while sliding. 50% is almost instantly.")]
    [Range(0, 50f)] public float slidingDragPercentage;

    [Space]
    [Tooltip("The (animation) speed for the player coming off the ladder on the top while sliding.")]
    public float ladderDismountSpeed;

    [Tooltip("How long the player needs to hold the key to dismount the ladder on top or bottom while sliding.")]
    public float ladderDismountTimer;

    [Tooltip("The factor to convert Velocity into sliding speed.")]
    public float slidingVelocityFactor = 1;

    [Tooltip("If true the player preserves their velocity on snap.")]
    public bool preservesVelocityOnSnap = false;
    #endregion
    [Space]
    #region swinging
    [Header("Swinging")]
    [Tooltip("If true the charackter uses swinging instead of sliding.")]
    public bool useSwinging;

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

    [Tooltip("The factor to convert velocity into swinging speed.")]
    public float swingingVelocityFactor = 1;
    #endregion
}
