using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementStateMachine : StateMachine
{
    #region public
    public CharacterController controller;

    public float speed;
    public float jumpheight;
    public float FowardInput;
    public float SideWardsInput;
    public Shelf closestShelf;

    [HideInInspector] public List<Shelf> possibleShelfs;

    [HideInInspector] public bool OnGround;
    #endregion

    #region Private

    #endregion

    private void Start()
    {
        SetState(new PlayerWalking(this));
        possibleShelfs = new List<Shelf>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("jump")) 
        {
            StartCoroutine(State.Jump());
        }

        if (Input.GetButtonDown("interact")&&CheckForShelf()) 
        {
            StartCoroutine(State.Snap());
        }

    }

    ///<summary>
    /// A Function to determin the closest Shelf to the player. Return false if none are in range.
    ///</summary>
    public bool CheckForShelf() 
    {
        if (possibleShelfs.Count == 0)
        {
            return false;
        }
        else 
        {
            float closestDistance = Mathf.Infinity;
            for (int i = 0; i < possibleShelfs.Count; i++) 
            {
                float distance = Vector3.Distance(possibleShelfs[i].transform.position, transform.position);
                if ( distance < closestDistance) 
                {
                    closestShelf = possibleShelfs[i];
                    closestDistance = distance;
                }
                
            }



            return true;
        }
    }
    

}
