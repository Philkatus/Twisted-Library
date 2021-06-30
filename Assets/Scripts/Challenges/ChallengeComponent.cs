using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeComponent : MonoBehaviour
{
    public delegate void EventHandler();
    public event EventHandler onResetChallenge;
    public Challenge challenge;
    public GameObject linkedUI;

    bool completed;
    public bool Completed
    {
        get
        {
            return completed;
        }
        set
        {
            if (value == false)
            {
                onResetChallenge();
            }
            else
            {
                challenge.componentCompletionTime = Time.time;
                ObjectManager.instance.uILogic.OnChallengeComponentComplete(linkedUI);
                CheckIfChallengeCompleted();
            }
            completed = value;
        }
    }

    ChallengeManager challengeManager;

    void Start()
    {
        if (ChallengeManager.instance == null)
            Debug.Log("This scene needs a Challenge Manager");
        else
            challengeManager = ChallengeManager.instance;
    }

    void CheckIfChallengeCompleted()
    {
        bool allComponentsComplete = true;
        foreach (ChallengeComponent component in challenge.components)
        {
            if (!component.completed)
            {
                allComponentsComplete = false;
                break;
            }
        }
        if (allComponentsComplete)
        {
            challenge.ChallengeCompleted = true;
        }
    }
}
