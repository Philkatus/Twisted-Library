using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Shelf : MonoBehaviour
{
    #region Public
    public PathCreator pathCreator;
    [Tooltip("can swing to both sides or just to one")]
    public bool twoSided;
    #endregion

    #region Private
    PlayerMovementStateMachine playerStateMachine;
    #endregion


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerStateMachine == null)
            {
                playerStateMachine = other.GetComponent<PlayerMovementStateMachine>();
            }
            playerStateMachine.possibleShelves.Add(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            if (playerStateMachine == null)
            {
                playerStateMachine = other.GetComponent<PlayerMovementStateMachine>();
            }
            playerStateMachine.possibleShelves.Remove(this);
        }
    }
}
