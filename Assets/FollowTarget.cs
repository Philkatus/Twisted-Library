using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset;

    void FixedUpdate()
    {
        if (target != null && ObjectManager.instance.pSM.closestRail != null)
        {
            transform.position = target.position + (-(transform.forward * offset.x)) + (-(transform.up * offset.y) + -(transform.right * offset.z));
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, target.eulerAngles.y, transform.eulerAngles.z);
            Debug.Log("Y: " + target.eulerAngles.y);
            //Debug.Log("EULER: " + target.eulerAngles.y);
            if (ObjectManager.instance.pSM.closestRail.railType == Rail.RailType.OnWall)
            {
                Debug.Log("ADJUST");
                //transform.position = target.position + (-(transform.forward * offset.x)) + (-(transform.up * offset.y) + -(transform.right * offset.z));
            }
            else
            {
                transform.position = target.position;
            }
        }
    }

    public void SetRotation()
    {
        if (target != null)
        {
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, target.eulerAngles.y, transform.eulerAngles.z);
            //transform.position = target.position + (-(transform.forward * offset.x)) + (-(transform.up * offset.y) + -(transform.right * offset.z));
        }
    }
}
