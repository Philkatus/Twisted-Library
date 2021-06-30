using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerFollowTarget : MonoBehaviour
{
    #region Singleton
    public static PlayerFollowTarget instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
    }
    #endregion

    [Tooltip("Insert Main Camera.")]

    [SerializeField] Camera Camera;

    [Tooltip("Insert the player target, the camera should follow.")]
    [SerializeField] Transform PlayerTarget;
    [SerializeField] Transform LadderTarget;
    [SerializeField] Vector3 LadderTargetOffset;
    private Transform currentTarget;
    [SerializeField] float Damping;
    [SerializeField] Vector3 ScreenSpaceOffset;

    [SerializeField] PlayerMovementStateMachine PlayerSM;
    [SerializeField] LayerMask EnvironmentLayer;

    float dampingStandard;
    float offsetToPlayer;
    bool inVerticalAdjustMode = false;
    bool isFalling = false;

    Coroutine readjustCameraCo;

    Vector3 m_CurrentVelocity;
    Vector3 m_DampedPos;

    Vector3 pos;
    RaycastHit hit;


    void OnEnable()
    {
        currentTarget = PlayerTarget;

        if (Camera == null)
            Camera = Camera.main;
        if (currentTarget != null)
            m_DampedPos = currentTarget.position;

        var pos = PlayerTarget.position;
        pos = Camera.transform.worldToLocalMatrix * pos;
        pos += ScreenSpaceOffset;
        pos = Camera.transform.localToWorldMatrix * pos;
        // Debug.Log("PLAYERTARGET: " + PlayerTarget.position.y);
        Debug.Log("this: " + transform.position.y);
        offsetToPlayer = transform.position.y - PlayerTarget.position.y;
        // Debug.Log("OFFSET: " + offsetToPlayer);
        dampingStandard = Damping;
        AdjustCameraY();
    }

    void FixedUpdate()
    {
        if (currentTarget != null)
        {
            pos = currentTarget.position;
            if (currentTarget == LadderTarget)
            {
                Debug.DrawRay(currentTarget.position, -currentTarget.forward, Color.blue, Mathf.Infinity);
                pos += (-(currentTarget.forward * LadderTargetOffset.x)) + (-(LadderTarget.right * LadderTargetOffset.y));
            }
            m_DampedPos = Damping < 0.01f ? pos : Vector3.SmoothDamp(m_DampedPos, pos, ref m_CurrentVelocity, Damping);
            pos = m_DampedPos;
            if (Camera != null)
            {
                pos = Camera.transform.worldToLocalMatrix * pos;
                pos += ScreenSpaceOffset;
                pos = Camera.transform.localToWorldMatrix * pos;
            }
            // Debug.Log("Target: " + currentTarget.position.y);
            CheckIfFalling();
            MoveCameraY();
        }
    }

    private void MoveCameraY()
    {
        if (doNotAdjust)
        {
            transform.position = new Vector3(pos.x, transform.position.y, pos.z);
        }
        else
        {
            transform.position = pos;
        }
    }

    private bool CheckIfFalling()
    {
        if (!Physics.Raycast(PlayerTarget.position, transform.TransformDirection(Vector3.down), out hit, 5, EnvironmentLayer))
        {
            doNotAdjust = false;
            return true;
        }
        return false;
    }

    bool doNotAdjust = false;
    public void OnSimpleJump()
    {

        if (CheckIfFalling())
        {
            Debug.Log("falling");
        }
        else
        {
            Debug.Log("Not Falling");
            doNotAdjust = true;
        }

    }
    public void DoAdjustY(bool onLadderPush)
    {
        if (doNotAdjust == true || onLadderPush)
        {
            m_DampedPos = transform.position;
            m_CurrentVelocity = new Vector3(m_CurrentVelocity.x, 0, m_CurrentVelocity.z);
            doNotAdjust = false;
        }
        else
        {
            transform.position = new Vector3(transform.position.x, currentTarget.position.y, transform.position.z);
        }
    }

    public void FollowPlayer()
    {
        currentTarget = PlayerTarget;
    }

    public void FollowLadder()
    {
        currentTarget = LadderTarget;
    }

    public void AdjustCameraY()
    {
        inVerticalAdjustMode = true;
    }
}
