using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RailTrigger : MonoBehaviour
{
    public UnityEvent onTriggerSwing;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<PlayerMovementStateMachine>().playerState==PlayerMovementStateMachine.PlayerState.swinging) 
        {
            onTriggerSwing.Invoke();
        }
    }
}
