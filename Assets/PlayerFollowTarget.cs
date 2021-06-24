using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollowTarget : MonoBehaviour
{
    public static PlayerFollowTarget instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
    }

    public bool isLadderPush = false;
    public Camera Camera;
    public Transform Target;
    public float Damping;
    private float dampingStandard;
    public Vector3 ScreenSpaceOffset;

    public float standardFollowHeight;

    [SerializeField] PlayerMovementStateMachine player;

    Vector3 m_CurrentVelocity;
    Vector3 m_DampedPos;

    void OnEnable()
    {
        if (Camera == null)
            Camera = Camera.main;
        if (Target != null)
            m_DampedPos = Target.position;

        
        var pos = Target.position;
        pos = Camera.transform.worldToLocalMatrix * pos;
        pos += ScreenSpaceOffset;
        pos = Camera.transform.localToWorldMatrix * pos;
        standardFollowHeight = pos.y;
        dampingStandard = Damping;
    }

    void FixedUpdate()
    {
        if (Target != null)
        {
            var pos = Target.position;
            m_DampedPos = Damping < 0.01f
                ? pos : Vector3.SmoothDamp(m_DampedPos, pos, ref m_CurrentVelocity, Damping);
            pos = m_DampedPos;
            if (Camera != null)
            {
                pos = Camera.transform.worldToLocalMatrix * pos;
                pos += ScreenSpaceOffset;
                pos = Camera.transform.localToWorldMatrix * pos;
            }

            if(isLadderPush){
                transform.position = pos;
                if(pos.y <= standardFollowHeight + 0.01f){
                    isLadderPush = false;
                }
            }else{
                transform.position = new Vector3(pos.x, transform.position.y, pos.z);
            }
        }
    }

    public void OnLadderPush(){
        isLadderPush = true;
    }
}
