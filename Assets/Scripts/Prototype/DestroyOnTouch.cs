using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnTouch : MonoBehaviour
{
    public GameObject particle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Instantiate(particle, this.transform.position, Quaternion.identity);
            Destroy(this.transform.parent.gameObject);
        }
    }

}
