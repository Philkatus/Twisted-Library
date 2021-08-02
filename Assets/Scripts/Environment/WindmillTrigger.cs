using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindmillTrigger : MonoBehaviour
{
    bool toDestroy = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            toDestroy = true;
            ObjectManager.instance.pSM.effects.MoveWind(this.gameObject);
        }
        if (toDestroy)
            DestroyMe();
    }
    void DestroyMe()
    {
        Destroy(this);
    }
}
