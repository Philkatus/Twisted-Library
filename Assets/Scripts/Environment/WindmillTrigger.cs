using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindmillTrigger : MonoBehaviour
{
    [SerializeField] GameObject[] fountains;
    bool toDestroy = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            toDestroy = true;
            ObjectManager.instance.pSM.effects.MoveWind(this.gameObject);
            foreach (GameObject fountain in fountains)
            {
                ObjectManager.instance.pSM.effects.StartFountain(fountain);
            }
        }
        if (toDestroy)
            DestroyMe();
    }
    void DestroyMe()
    {
        Destroy(this);
    }
}
