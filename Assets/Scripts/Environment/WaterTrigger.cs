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
        if (collider.tag == "Player")
        {
            if (!psm.isOnWater)
                ObjectManager.instance.pSM.effects.SetActiveShadow(false);
            psm.isOnWater = true;
            psm.stillOnWater = true;

        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Player")
        {
            StartCoroutine(WaterExitDelay());
        }
    }
    IEnumerator WaterExitDelay()
    {
        psm.stillOnWater = false;
        yield return new WaitForSeconds(0.5f);
        if (!psm.stillOnWater)
        {
            psm.isOnWater = false;
            ObjectManager.instance.pSM.effects.SetActiveShadow(true);
        }
       
    }
}
