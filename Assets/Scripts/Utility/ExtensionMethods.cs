using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{

    public static float Remap(this float value, float inputFrom, float inputTo, float outputFrom, float outputTo)
    {

        return (value - inputFrom) / (inputTo - inputFrom) * (outputTo - outputFrom) + outputFrom;

    }


}
