using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTrigger : MonoBehaviour
{
    PlayerMovementStateMachine psm;

    void Start()
    {
        psm = ObjectManager.instance.pSM;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player" && !psm.isOnWater)
        {
            psm.isOnWater = true;
            ObjectManager.instance.pSM.effects.SetActiveShadow(false);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Player" && psm.isOnWater)
        {
            psm.isOnWater = false;
            ObjectManager.instance.pSM.effects.SetActiveShadow(true);
        }
    }
}
