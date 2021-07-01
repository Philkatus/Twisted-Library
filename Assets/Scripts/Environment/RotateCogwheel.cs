using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCogwheel : MonoBehaviour
{
    Animator anim;
    ChallengeComponent challengeComponent;
    float tWheelAcceleration = 1;
    float currentRotationDirection;
    bool changeDirection;
    bool stopWheel;
    bool doOncePerAttempt;

    void Start()
    {
        anim = GetComponent<Animator>();
        challengeComponent = GetComponent<ChallengeComponent>();
        challengeComponent.onResetChallenge += new ChallengeComponent.EventHandler(SetStopWheelTrue);
        challengeComponent.type = "cogwheel";
    }

    void Update()
    {
        if (changeDirection)
        {
            if (tWheelAcceleration > 0)
            {
                StopWheel();
            }
            else if (tWheelAcceleration <= 0)
            {
                changeDirection = false;
                tWheelAcceleration = 0;
                StartCoroutine(RotateWheel());
            }
        }
        if (stopWheel)
        {
            if (tWheelAcceleration > 0)
            {
                StopWheel();
            }
            else
            {
                stopWheel = false;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            var psm = other.gameObject.GetComponent<PlayerMovementStateMachine>();
            bool isSliding = psm.playerState == PlayerMovementStateMachine.PlayerState.swinging;
            var slidingInput = psm.slidingInput;
            if (slidingInput != 0 && isSliding)
            {
                if (!challengeComponent.challenge.challengeStarted && !doOncePerAttempt)
                {
                    foreach (ChallengeComponent component in challengeComponent.challenge.components)
                    {
                        ObjectManager.instance.uILogic.OnChallengeStartedComponent(component.linkedUI, challengeComponent.type);
                    }
                    challengeComponent.challenge.ShowCurrentLandmark();
                    doOncePerAttempt = true;
                }
                challengeComponent.Completed = true;
                if (currentRotationDirection == 0)
                {
                    tWheelAcceleration = 0;
                    StartCoroutine(RotateWheel());
                    currentRotationDirection = slidingInput;
                }
                else if (currentRotationDirection != slidingInput)
                {
                    currentRotationDirection = slidingInput;
                    tWheelAcceleration = 1;
                    changeDirection = true;
                    StopAllCoroutines();
                }
            }
        }
    }

    IEnumerator RotateWheel()
    {
        while (true)
        {
            tWheelAcceleration += Time.deltaTime / 1.3f;
            Mathf.Clamp(tWheelAcceleration, 0f, 1f);
            float angleDelta = Mathf.Lerp(0, 1f, tWheelAcceleration) * currentRotationDirection;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y - angleDelta, transform.eulerAngles.z);
            yield return new WaitForEndOfFrame();
        }
    }

    void StopWheel()
    {
        tWheelAcceleration -= Time.deltaTime / 0.7f;
        float angleDelta = Mathf.Lerp(0, 1f, tWheelAcceleration) * currentRotationDirection;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + angleDelta, transform.eulerAngles.z);
    }

    void SetStopWheelTrue()
    {
        stopWheel = true;
        doOncePerAttempt = false;
    }
}
