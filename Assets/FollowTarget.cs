using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] Transform target;

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position;
            Debug.Log("Rotation: " + target.rotation.y);
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, target.eulerAngles.y, transform.eulerAngles.z);
        }
    }
}
