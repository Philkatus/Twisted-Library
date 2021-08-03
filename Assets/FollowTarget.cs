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
        var velocity = Vector3.zero;
        //transform.position = Vector3.SmoothDamp(target.position, finalPosition, ref velocity, 0.9f);
        transform.position = finalPosition;
    }
    void FixedUpdate()
    {
        if (pSM.playerState == PlayerMovementStateMachine.PlayerState.swinging)
        {
            SetPosition();

            //if(timer < maxDuration)
            //{
            //    transform.position = Vector3.Lerp(transform.position, finalPosition, timer/maxDuration);
            //    timer += Time.deltaTime;
            //}
            //else
            //{
            //    timer = 0;
            //}
            //if (pSM.closestRail.railType == Rail.RailType.OnWall)
            //{
            //    //Debug.Log("ADJUST");
            //    //transform.position = target.position + (-(transform.forward * offset.x)) + (-(transform.up * offset.y) + -(transform.right * offset.z));
            //}
            //else
            //{
            //    transform.position = target.position;
            //}
        }
    }
}
