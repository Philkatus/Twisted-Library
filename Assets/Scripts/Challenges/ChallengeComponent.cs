using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeComponent : MonoBehaviour
{
    public delegate void EventHandler();
    public event EventHandler onResetChallenge;
    public Challenge challenge;
    public GameObject linkedUI;
    [HideInInspector] public string type;

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
            Debug.LogError("This scene needs a Challenge Manager");
        else
            challengeManager = ChallengeManager.instance;
        // if (!challenge.components.Contains(this))
        //     challenge.components.Add(this);
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
