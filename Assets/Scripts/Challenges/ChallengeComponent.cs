using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeComponent : MonoBehaviour
{
    ChallengeManager relatedChallenge;
    //[SerializeField] int challengeNumber;
    [SerializeField] int componentNumber;
    Animator anim;
    float timeLeft = 0;
    float tWheelAcceleration = 1;
    float currentRotationDirection;
    bool changeDirection;

    void Start()
    {
        if (ChallengeManager.instance == null)
            Debug.Log("This scene needs a Challenge Manager");
        else
            relatedChallenge = ChallengeManager.instance;
        anim = GetComponent<Animator>();
        //relatedChallenge.challenges[challengeNumber].completedChallengeParts[componentNumber] = false;
        relatedChallenge.completedChallengeParts.Add(false);
    }

    void Update()
    {
        if (changeDirection)
        {
            if (tWheelAcceleration > 0)
            {
                tWheelAcceleration -= Time.deltaTime / 0.7f;
                float angleDelta = Mathf.Lerp(0, 1f, tWheelAcceleration) * currentRotationDirection;
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + angleDelta, transform.eulerAngles.z);
            }
            else if (tWheelAcceleration <= 0)
            {
                changeDirection = false;
                tWheelAcceleration = 0;
                StartCoroutine(RotateWheel());
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
                if (currentRotationDirection == 0)
                {
                    tWheelAcceleration = 0;
                    relatedChallenge.completedChallengeParts[componentNumber] = true;
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
}
