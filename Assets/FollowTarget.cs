using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset;
    [SerializeField] Vector3 swingingOffset;

    PlayerMovementStateMachine pSM;
    private void Start()
    {
        pSM = ObjectManager.instance.pSM;
    }

    public void SetPosition()
    {
        Vector3 railOrientation = pSM.closestRail.pathCreator.path.GetNormalAtDistance(pSM.currentDistance);
        int ladderOrientation = pSM.snapdirection;

        Vector3 cameraOrientation = railOrientation * ladderOrientation;
        transform.rotation = Quaternion.LookRotation(cameraOrientation, Vector3.up);

        Vector3 finalPosition = target.position + (offset.x * transform.forward + offset.y * transform.up + offset.z * transform.right);
        transform.position = finalPosition;
    }
    void FixedUpdate()
    {
        if (pSM.playerState == PlayerMovementStateMachine.PlayerState.swinging)
        {
            SetPosition();
        }
    }
}
