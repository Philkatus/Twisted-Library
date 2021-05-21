using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerChallengeStarter : MonoBehaviour
{
    public TimerChallenge timerChallenge;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            timerChallenge.challengePanel.SetActive(true);
            timerChallenge.prefabText.text = "Go!";
            timerChallenge.isChallengeActive = true;
        }
    }
}
