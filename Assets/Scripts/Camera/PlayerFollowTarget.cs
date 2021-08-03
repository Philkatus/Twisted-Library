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

    [SerializeField] LayerMask EnvironmentLayer;

    float dampingStandard;
    float offsetToPlayer;

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
        offsetToPlayer = transform.position.y - PlayerTarget.position.y;
        dampingStandard = Damping;
    }

    void FixedUpdate()
    {
        if (currentTarget != null)
        {
            pos = currentTarget.position;
            if (currentTarget == LadderTarget)
            {
                pos += (-(currentTarget.forward * LadderTargetOffset.x)) + (-(currentTarget.up * LadderTargetOffset.y));
            }

            m_DampedPos = Damping < 0.01f ? pos : Vector3.Lerp(m_DampedPos, pos, Damping);
            // Vector3.SmoothDamp(m_DampedPos, pos, ref m_CurrentVelocity, Damping);
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
        if (ObjectManager.instance.pSM.playerState != PlayerMovementStateMachine.PlayerState.swinging)
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
        else
        {
            transform.position = currentTarget.position;
        }
            
        
    }

    private bool CheckIfFalling()
    {
        {
            if (!Physics.Raycast(PlayerTarget.position, transform.TransformDirection(Vector3.down), out hit, 6, EnvironmentLayer))
            {
                doNotAdjust = false;
                return true;
            }
        }
        return false;
    }

    bool doNotAdjust = false;
    public void OnSimpleJump()
    {
        if (!CheckIfFalling())
        {
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
    [SerializeField] AnimationCurve lerpCurve;
    [SerializeField] Transform tempTarget;

    public void FollowPlayer()
    {
        if (switchCoroutine != null)
        {
            StopCoroutine(switchCoroutine);
        }
        switchCoroutine = StartCoroutine(MoveTowards(PlayerTarget));
    }

    public void FollowLadder()
    {
        Debug.Log("FOllow Ladder");
        if (switchCoroutine != null)
        {
            StopCoroutine(switchCoroutine);
        }
        LadderTarget.GetComponent<FollowTarget>().SetPosition();
        switchCoroutine = StartCoroutine(MoveTowards(LadderTarget));
    }

    Coroutine switchCoroutine;
    IEnumerator MoveTowards(Transform endTarget)
    {
        Debug.Log("SWITCH TO " + endTarget.name);
        m_DampedPos = transform.position;
        m_CurrentVelocity = Vector3.zero;
        var timer = 0f;
        var startPos = transform.position;
        var maxDuration = .4f;
        tempTarget.position = startPos;
        tempTarget.position = currentTarget.position;
        currentTarget = tempTarget;
        
        while (timer < maxDuration)
        {
            var endPos = endTarget.position;
            timer += Time.deltaTime;
            currentTarget.transform.position = Vector3.Lerp(startPos, endPos, lerpCurve.Evaluate(timer / maxDuration));
            yield return null;
        }
        currentTarget = endTarget;
        switchCoroutine = null;
    }
}
