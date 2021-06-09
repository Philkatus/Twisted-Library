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

    void Start()
    {
        if (ChallengeManager.instance == null)
            Debug.Log("This scene needs a Challenge Manager");
        else
            relatedChallenge = ChallengeManager.instance;
        anim = GetComponent<Animator>();
        //relatedChallenge.challenges[challengeNumber].completedChallengeParts[componentNumber] = false;
        relatedChallenge.completedChallengeParts[componentNumber] = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            relatedChallenge.completedChallengeParts[componentNumber] = true;
            anim.SetTrigger("Start");
        }
    }
}
