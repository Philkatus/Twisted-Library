using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset;

    void FixedUpdate()
    {
        if (target != null)
        {
            if (ObjectManager.instance.pSM.closestRail.railType == Rail.RailType.OnWall)
            {
                transform.position = target.position + (-(transform.forward * offset.x)) + (-(transform.up * offset.y) + -(transform.right * offset.z));
            }
            else
            {
                transform.position = target.position;
            }
        }
    }

    public void SetRotation()
    {
        if(target != null)
        {
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, target.eulerAngles.y, transform.eulerAngles.z);
        }
    }
}
