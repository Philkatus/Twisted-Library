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
    public float movementVelocityFactor;

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

    [Tooltip("The factor to convert Velocity into air-Movementspeed")]
    public float jumpVelocityFactor;
    #endregion
    [Space]
    #region Sliding
    [Header("Sliding")]
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

    [Tooltip("The factor to convert Velocity into Sliding Speed")]
    public float slidingVelocityFactor;

    [Tooltip("If true the player preserves their velocity on Snap")]
    public bool preservesVelocityOnSnap;
    #endregion
    [Space]
    #region swinging
    [Header("swinging")]
    [Tooltip("if true the charackter uses swinging instead of sliding.")]
    public bool useSwinging;

    [Tooltip("How fast the player accelrates while swinging.")]
    public float swingingAcceleration;

    [Tooltip("How fast the player decelerate while swinging with reverse input")]
    public float swingingDeceleration;

    [Tooltip("The maximum Speed the Player can reach while swinging.")]
    public float maxSwingSpeed;

    [Tooltip("How fast the player decelerate while swinging without giving an input")]
    public float swingingGravity;

    [Tooltip("The factor to convert Velocity into Swinging Speed")]
    public float swingingVelocityFactor;
    #endregion
}
