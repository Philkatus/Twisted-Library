using System.Collections;
using System.Collections.Generic;
using UnityEngine.VFX;
using UnityEngine;

public class StartWaterfall : MonoBehaviour
{
    [SerializeField] VisualEffect waterFall;
    [SerializeField] GameObject water;

    void Start()
    {
        water.SetActive(false);
    }

    public void StartTheWaterfall()
    {
        // StartVFX
        //waterFall.SendEvent("_Start");
        water.SetActive(true);
        Debug.Log("start waterfall");
    }
}
