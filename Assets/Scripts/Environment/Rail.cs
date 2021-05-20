using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Rail : MonoBehaviour
{
    #region Public
    public PathCreator pathCreator;
    public enum RailType
    {
        TwoSided, OnWall, FreeHanging
    }
    [Tooltip("the type of shelf (for swinging): " +
        " Two Sided - Can Swing in loop with no limitations; " +
        " OnWall - has a Wall to push off from; " +
        " FreeHanging - Can Swing back and forth, but not necessarily up ")]
    public RailType railType;
    public bool stopSlidingAtTheEnd;
    #endregion

    #region Private
    PlayerMovementStateMachine playerStateMachine;
    #endregion

}
