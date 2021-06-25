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


    Vector3 m_CurrentVelocity;
    Vector3 m_DampedPos;

    Vector3 pos;
    RaycastHit hit;

    // public void AssignAllVars(){
    //     Camera = Camera.main;
    //     PlayerTarget = GameObject.Find("POSITION_FollowTarget").transform;
    //     LadderTarget = GameObject.Find("POSITION_FollowTarget_Swinging").transform;
    //     Damping = 0.1f;
    //     PlayerSM = GameObject.FindObjectOfType<PlayerMovementStateMachine>();
    //     EnvironmentLayer = LayerMask.GetMask("Environment");
    // }

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
        Debug.Log("PLAYERTARGET: " + PlayerTarget.position.y);
        Debug.Log("this: " + transform.position.y);
        offsetToPlayer = transform.position.y - PlayerTarget.position.y;
        Debug.Log("OFFSET: " + offsetToPlayer);
        dampingStandard = Damping;
        AdjustCameraY();
    }

    void FixedUpdate()
    {
        if (currentTarget != null)
        {
            pos = currentTarget.position;
            if(currentTarget == LadderTarget){
                pos += (-LadderTarget.forward * LadderTargetOffset.x) + (-LadderTarget.up * LadderTargetOffset.y);
            }
            m_DampedPos = Damping < 0.01f
                ? pos : Vector3.SmoothDamp(m_DampedPos, pos, ref m_CurrentVelocity, Damping);
            pos = m_DampedPos;
            if (Camera != null)
            {
                pos = Camera.transform.worldToLocalMatrix * pos;
                pos += ScreenSpaceOffset;
                pos = Camera.transform.localToWorldMatrix * pos;
            }
            CheckIfFalling();
            MoveCameraY();
        }
    }

    private void MoveCameraY()
    {
        if (inVerticalAdjustMode || currentTarget == LadderTarget)
        {
            transform.position = pos;
            if (Mathf.Approximately(transform.position.y, currentTarget.position.y + ScreenSpaceOffset.y))
            {
                inVerticalAdjustMode = false;
                Damping = dampingStandard;
            }
        }
        else
        {
            transform.position = new Vector3(pos.x, transform.position.y, pos.z);
        }
    }

    private void CheckIfFalling()
    {
        if (!Physics.Raycast(PlayerTarget.position, transform.TransformDirection(Vector3.down), out hit, PlayerSM.stats.jumpHeight + 0.1f, EnvironmentLayer))
        {
            inVerticalAdjustMode = true;
        }
    }

    public void CheckForRail()
    {
        if (PlayerSM.playerState == PlayerMovementStateMachine.PlayerState.swinging)
        {
            currentTarget = LadderTarget;
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

    void OnDrawGizmosSelected()
    {

#if UNITY_EDITOR
        Gizmos.color = Color.red;

        //Draw the suspension
        Gizmos.DrawSphere(
            transform.position,
            0.1f
        );

#endif
    }


}
