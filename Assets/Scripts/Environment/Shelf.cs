using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Shelf : MonoBehaviour
{
    #region Public
    public PathCreator pathCreator;
    #endregion
    #region Private
    PlayerMovementStateMachine playerStateMachine;

    #endregion
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            Debug.Log("you entered my triggerzone");
            if (playerStateMachine == null)
            {
                playerStateMachine = other.GetComponent<PlayerMovementStateMachine>();
            }
            playerStateMachine.possibleShelfs.Add(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("you left my triggerzone");
            if (playerStateMachine == null)
            {
                playerStateMachine = other.GetComponent<PlayerMovementStateMachine>();
            }
            playerStateMachine.possibleShelfs.Remove(this);
        }
    }
}
