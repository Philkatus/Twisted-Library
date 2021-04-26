using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ValuesScriptableObject", order = 1)]
public class ValuesScriptableObject : ScriptableObject
{
    [Header("Movement/Walking")]
    [Tooltip("How fast the player accelerates to maximum speed while walking.")]
    public float movementAcceleration;

    [Tooltip("How fast the player can move while walking and in the air at max.")]
    public float maximumMovementSpeed;

    [Tooltip("How much drag is applied when there is no input while walking.")]
    public float movementDrag;

    [Space]
    [Header("Jumping/Air Movement")]
    [Tooltip("How high the player jumps.")]
    public float jumpHeight;

    [Tooltip("Limits the movement speed for the air movement.")]
    [Range(.1f, 1)] public float airMovementFactor;
    public float jumpingDrag;

    [Tooltip("...is working against me.")]
    public float gravity;

    [Space]
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
}