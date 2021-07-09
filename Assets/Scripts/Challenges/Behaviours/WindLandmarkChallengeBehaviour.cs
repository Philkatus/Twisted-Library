using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindLandmarkChallengeBehaviour : MonoBehaviour
{

    [SerializeField] RotateStuffEaselyDude rotateScript;
    [SerializeField] ScaleStuffEasilyDude scaleScript;


    public void OnAllComponentsCompleted()
    {
        rotateScript.isPlaying = true;
        scaleScript.isPlaying = true;
    }
}
