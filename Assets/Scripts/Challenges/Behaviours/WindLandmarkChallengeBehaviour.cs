using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindLandmarkChallengeBehaviour : MonoBehaviour
{

    [SerializeField] RotateStuffEaselyDude rotateScript;


    public void OnAllComponentsCompleted()
    {
        rotateScript.isPlaying = true;
    }
}
