using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCogwheel : MonoBehaviour
{
    ChallengeComponent challengeComponent;
    float tWheelAcceleration = 1;
    float currentRotationDirection;
    float turnOnTimer;
    bool turnOn;
    bool changeDirection;
    bool stopWheel;
    bool doOncePerAttempt;
    bool rotateWheel;
    bool doOnce;

    [SerializeField] List<Transform> wheels = new List<Transform>();
    [SerializeField] int CogSoundIndex;

    void Start()
    {
        challengeComponent = GetComponent<ChallengeComponent>();
        challengeComponent.onResetChallenge += new ChallengeComponent.EventHandler(SetStopWheelTrue);
        challengeComponent.type = "cogwheel";
    }

    void Update()
    {
        var challenge = challengeComponent.challenge;
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
                currentRotationDirection = 0;
                tWheelAcceleration = 0;
            }
        }
        if (turnOn)
        {
            turnOnTimer += Time.deltaTime;
            ObjectManager.instance.uILogic.UpdateComponentVisual(challengeComponent.linkedUI, challengeComponent.type, turnOnTimer, challenge.timeToCompleteComponents, true);
            if (turnOnTimer >= 1)
            {
                challengeComponent.Completed = true;
                turnOnTimer = 0;
                turnOn = false;
            }
        }
        if (!turnOn && !stopWheel && !changeDirection && rotateWheel)
        {
            float timeSinceCompletion = Time.time - challenge.componentCompletionTime;
            tWheelAcceleration = ExtensionMethods.Remap(timeSinceCompletion, 0, challenge.timeToCompleteComponents, 1, 0);
            Mathf.Clamp(tWheelAcceleration, 0f, 1f);
            float angleDelta = Mathf.Lerp(0, 1f, tWheelAcceleration) * currentRotationDirection;
            foreach (Transform wheel in wheels)
            {
                wheel.transform.Rotate(0, angleDelta, 0, Space.Self);
            }
        }
        if (challenge.ChallengeCompleted && !doOnce)
        {
            doOnce = true;
            StartCoroutine(RotateWheelNoEnd());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player" && !challengeComponent.challenge.ChallengeCompleted)
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
                        ObjectManager.instance.uILogic.OnChallengeStartedComponent(component.linkedUI, component.type);
                    }
                    AudioManager.Instance.PlayRandom("CogChallenge",transform.position);

                    AudioManager.Instance.CogSound(CogSoundIndex, transform.position);
                }
                if (!doOncePerAttempt)
                {
                    doOncePerAttempt = true;
                    turnOn = true;
                }
                if (currentRotationDirection == 0)
                {
                    tWheelAcceleration = 0;
                    StartCoroutine(RotateWheel());
                    currentRotationDirection = slidingInput;
                }
                else if (currentRotationDirection != slidingInput)
                {
                    StopAllCoroutines();
                    currentRotationDirection = slidingInput;
                    tWheelAcceleration = 1;
                    changeDirection = true;
                }
                else if (rotateWheel)
                {
                    challengeComponent.Completed = true;
                }
            }
        }
    }

    IEnumerator RotateWheel()
    {
        WaitForEndOfFrame delay = new WaitForEndOfFrame();
        while (tWheelAcceleration < 1)
        {
            tWheelAcceleration += Time.deltaTime / 1.3f;
            float angleDelta = Mathf.Lerp(0, 1f, tWheelAcceleration) * currentRotationDirection;
            foreach (Transform wheel in wheels)
            {
                wheel.transform.Rotate(0, angleDelta, 0, Space.Self);
            }
            if (tWheelAcceleration >= 1)
            {
                rotateWheel = true;
                yield return null;
            }
            yield return delay;
        }
    }

    IEnumerator RotateWheelNoEnd()
    {
        WaitForEndOfFrame delay = new WaitForEndOfFrame();
        while (doOnce)
        {
            foreach (Transform wheel in wheels)
            {
                wheel.transform.Rotate(0, 1, 0, Space.Self);
            }
            yield return delay;
        }
    }

    void StopWheel()
    {
        tWheelAcceleration -= Time.deltaTime / 0.7f;
        float angleDelta = Mathf.Lerp(0, 1f, tWheelAcceleration) * currentRotationDirection;
        foreach (Transform wheel in wheels)
        {
            wheel.transform.Rotate(0, angleDelta, 0, Space.Self);
        }
    }

    void SetStopWheelTrue()
    {
        stopWheel = true;
        rotateWheel = false;
        doOncePerAttempt = false;
    }
}
