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
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, target.eulerAngles.y, transform.eulerAngles.z);
            transform.position = target.position + (-(transform.forward * offset.x)) + (-(transform.up * offset.y) + -(transform.right * offset.z));
        }
    }
}
