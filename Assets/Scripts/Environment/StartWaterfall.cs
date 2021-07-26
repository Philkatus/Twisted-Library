using System.Collections;
using System.Collections.Generic;
using UnityEngine.VFX;
using UnityEngine;

public class StartWaterfall : MonoBehaviour
{
    [SerializeField] VisualEffect waterFall;

    public void StartTheWaterfall()
    {
        // StartVFX
        waterFall.SendEvent("_Start");
    }
}
