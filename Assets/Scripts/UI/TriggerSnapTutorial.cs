using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSnapTutorial : MonoBehaviour
{
    bool doOnce;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player" && !doOnce)
        {
            StartCoroutine(ObjectManager.instance.uILogic.ShowTutorialExplanation("snap"));
            doOnce = true;
        }
    }
}
