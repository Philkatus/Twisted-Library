using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderStateMachine : StateMachine
{
    #region public
    public float length;
    [HideInInspector] public float LadderVelocity;
    public Vector3 direction 
    {
        get 
        {
            return ladderMesh.right;
        }
    }
    #endregion

    #region private
    [SerializeField] Transform ladderMesh;
    #endregion

}
