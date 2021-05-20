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


}
