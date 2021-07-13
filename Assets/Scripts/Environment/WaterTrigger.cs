using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTrigger : MonoBehaviour
{
    bool walkingInWater;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player" && !walkingInWater)
        {
            walkingInWater = true;
            Debug.Log("went into water");
            ObjectManager.instance.pSM.effects.SetActiveShadow(false);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Player" && walkingInWater)
        {
            Debug.Log("left water");
            walkingInWater = false;
            ObjectManager.instance.pSM.effects.SetActiveShadow(true);
        }
    }
}
