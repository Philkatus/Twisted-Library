using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
    }
    [SerializeField] Camera mainCam;
    [SerializeField] CinemachineFreeLook cam01, cam02;
    [SerializeField] CinemachineBrain brain;
    [SerializeField] Transform target;
    [SerializeField] PlayerMovementStateMachine player;

    CinemachineFreeLook current;
    bool justSwitched = false;
    float delta = 0.005f;
    
    // Start is called before the first frame update
    void Start()
    {
        current = cam01;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SwitchCams();
    }

    private void SwitchCams()
    {
        if(current == cam01)
        {
            if(current.m_YAxis.Value <= delta)
            {
                if (!justSwitched)
                {
                    GoDown();
                }
            }
            else
            {
                justSwitched = false;
            }
        }
        if(current == cam02)
        {
            if (current.m_YAxis.Value >= 1 - delta)
            {
                if (!justSwitched)
                {
                    GoUp();
                }
            }
            else
            {
                justSwitched = false;
            }
        }
    }

    private void GoDown()
    {
        justSwitched = true;

        current.m_YAxis.Value = 0;
        cam01.Priority -= 5;
        cam02.Priority += 5;
        current = cam02;
        
    }

    private void GoUp()
    {
        justSwitched = true;

        current.m_YAxis.Value = 1;
        cam01.Priority += 5;
        cam02.Priority -= 5;
        current = cam01;
    }

    private void AdjustHeightOnDoubleJump()
    {
        float ghostPositionY = target.transform.position.y;
        Vector3 characterViewPos = mainCam.WorldToViewportPoint(target.position + (player.playerVelocity * Time.deltaTime));

        if(characterViewPos.y > 0f)
        {
            ghostPositionY = target.transform.position.y;
        }
        transform.position = new Vector3(transform.position.x, ghostPositionY, transform.position.z);
    }
}