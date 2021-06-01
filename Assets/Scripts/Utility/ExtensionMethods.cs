using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static float Remap(this float value, float inputFrom, float inputTo, float outputFrom, float outputTo)
    {
        return (value - inputFrom) / (inputTo - inputFrom) * (outputTo - outputFrom) + outputFrom;
    }

    /// <summary>
    /// Checks if a vector points to the right or left of another Vector.
    /// Returns -1 when to the left, 1 to the right, and 0 for forward/backward.
    /// </summary>
    /// <param name="refDirection"> The referenced direction </param>
    /// <param name="targetDirection"> The direction to check if its left or right of refDirection </param>
    /// <param name="up"> The up direction to both other directions </param>
    /// <returns></returns>
    public static float AngleDirection(Vector3 refDirection, Vector3 targetDirection, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(refDirection, targetDirection);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0.0f)
        {
            return 1.0f;
        }
        else if (dir < 0.0f)
        {
            return -1.0f;
        }
        else
        {
            return 0.0f;
        }
    }


    /// <summary>
    /// Calculates the resulting signed magnitude alongside the targetdirection after a change of direction.
    /// </summary>
    /// <param name="currentVelocity">the Velocity to change </param>
    /// <param name="targetDirection">The normalized direction you want to change to</param>
    /// <returns></returns>
    public static float resultingSpeed(Vector3 currentVelocity, Vector3 targetDirection)
    {
        float resultingSpeed = currentVelocity.x * targetDirection.x + currentVelocity.y * targetDirection.y + currentVelocity.z * targetDirection.z;

        return resultingSpeed;
    }

    /// <summary>
    /// calculates the resulting velocity through a change in direction
    /// </summary>
    /// <param name="currentVelocity"> the Velocity to change </param>
    /// <param name="targetDirection"> the normalized direction you want to change to</param>
    /// <returns></returns>
    public static Vector3 resultingVelocity(Vector3 currentVelocity, Vector3 targetDirection)
    {
        float resultingSpeed = ExtensionMethods.resultingSpeed(currentVelocity, targetDirection);

        return targetDirection * resultingSpeed;
    }

    /// <summary>
    /// calculates the resulting clamped velocity through a change in direction
    /// </summary>
    /// <param name="currentVelocity"> the Velocity to change </param>
    /// <param name="targetDirection"> the normalized direction you want to change to</param>
    /// <param name="maximumSpeed"> the maximum speed the return value gets clamped to</param>
    /// <returns></returns>
    public static Vector3 resultingClampedVelocity(Vector3 currentVelocity, Vector3 targetDirection, float maximumSpeed)
    {
        float resultingSpeed = ExtensionMethods.resultingSpeed(currentVelocity, targetDirection);
        resultingSpeed = Mathf.Clamp(resultingSpeed, -maximumSpeed, maximumSpeed);

        return targetDirection * resultingSpeed;
    }

    /// <summary>
    /// takes the Player Velocity and puts a clamp on one direction of it
    /// </summary>
    /// <param name="currentVelocity"> the Velocity to change </param>
    /// <param name="targetDirection"> The direction to clamp </param>
    /// <param name="maximumSpeed"> the maximumspeed that the return Vector should have in the target direction </param>
    /// <returns></returns>
    public static Vector3 ClampPlayerVelocity(Vector3 currentVelocity, Vector3 targetDirection, float maximumSpeed)
    {
        float resultingSpeed = ExtensionMethods.resultingSpeed(currentVelocity, targetDirection);
        Vector3 clampedVelocity = targetDirection * Mathf.Clamp(resultingSpeed, -maximumSpeed, maximumSpeed);
        currentVelocity -= ExtensionMethods.resultingVelocity(currentVelocity, targetDirection);
        currentVelocity += clampedVelocity;
        return currentVelocity;
    }





}
