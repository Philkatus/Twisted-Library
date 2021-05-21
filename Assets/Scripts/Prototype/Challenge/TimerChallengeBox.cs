using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerChallengeBox : MonoBehaviour
{

    public bool isMyChallengeActive;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && isMyChallengeActive)
        {
            this.GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
