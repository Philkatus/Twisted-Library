using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeCompletionManager : MonoBehaviour
{
    public List<GameObject> challenges = new List<GameObject>();
    public Text UIChallengeInfo;
    public int nbrOfCompletedChallenges;


    private void Start()
    {
        if(challenges.Count == 0)
        {
            this.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        nbrOfCompletedChallenges = 0;
        foreach(GameObject challenge in challenges)
        {
            if(challenge.GetComponentInChildren<TimerChallenge>().isCompleted)
            {
                nbrOfCompletedChallenges++;
            }
        }

        UIChallengeInfo.text = nbrOfCompletedChallenges.ToString() + "/" + challenges.Count.ToString() + " Challenges Complete";
    }
}
