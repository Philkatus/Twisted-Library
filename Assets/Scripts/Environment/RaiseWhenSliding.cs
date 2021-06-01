using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiseWhenSliding : MonoBehaviour
{
    PlayerMovementStateMachine pSM;
    [SerializeField] float maxSpeed;
    float currentSpeed;
    bool isInRange;

    // Start is called before the first frame update
    void Start()
    {
        pSM = ObjectManager.instance.pSM;
    }

    // Update is called once per frame
    void Update()
    {
        if (isInRange)
        {
            if (pSM.slidingInput != 0 && pSM.playerState == PlayerMovementStateMachine.PlayerState.swinging)
            {
                currentSpeed += Mathf.Lerp(0.00f, 1.00f, 0.1f) * Time.deltaTime;
                float speed = currentSpeed >= maxSpeed ? maxSpeed : currentSpeed;
                this.transform.position += Vector3.up * speed * Time.deltaTime;
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            isInRange = true;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            currentSpeed = 0;
            isInRange = false;
        }
    }
}
