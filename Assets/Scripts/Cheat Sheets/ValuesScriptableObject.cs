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
    #endregion
    [Space]
    #region Sliding
    [Header("Sliding")]
    [Tooltip("How fast the player climbs the ladder up and down while sliding.")]
    public float climbingSpeedOnLadder;

    [Tooltip("How fast the player accelerates to maximum sliding speed.")]
    public float slidingAcceleration;
    public float maxSlidingSpeed;

    [Tooltip("How fast the player comes to a halt while sliding. 50% is almost instantly.")]
    [Range(0, 50f)] public float slidingDragPercentage;

    [Space]
    [Tooltip("The (animation) speed for the player coming off the ladder on the top while sliding.")]
    public float ladderDismountSpeed;

    [Tooltip("How long the player needs to hold the key to dismount the ladder on top or bottom while sliding.")]
    public float ladderDismountTimer;
    #endregion
    [Space]
    #region swinging
    [Header("swinging")]
    [Tooltip("if true the charackter uses swinging instead of sliding.")]
    public bool useSwinging;

    [Tooltip("How fast the player accelrates while swinging.")]
    public float SwingingAcceleration;

    [Tooltip("How fast the player decelerate while swinging with reverse input")]
    public float SwingingDeceleration;

    [Tooltip("The maximum Speed the Player can reach while swinging.")]
    public float maxSwingSpeed;

    [Tooltip("The factor that is use to convert swinging velocity to movement conversion and back.")]
    public float VelocityToSwingingConversion;

    [Tooltip("How fast the player decelerate while swinging without giving an input")]
    public float SwingingDrag;
    #endregion
}
