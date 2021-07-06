using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeComponent : MonoBehaviour
{
    public delegate void EventHandler();
    public event EventHandler onResetChallenge;
    [HideInInspector] public Challenge challenge;
    public GameObject linkedUI;
    [HideInInspector] public string type;

    [SerializeField] bool completed;
    public bool Completed
    {
        get
        {
            return completed;
        }
        set
        {
            completed = value;
            if (value == false)
            {
                challenge.componentTimer = 0;
                onResetChallenge();
            }
            else
            {
                challenge.componentTimer = 0;
                challenge.componentCompletionTime = Time.time;
                challenge.challengeStarted = true;
                ObjectManager.instance.uILogic.OnComponentComplete(linkedUI, type);
                challenge.CheckIfChallengeCompleted();
            }
        }
    }

    ChallengeManager challengeManager;

    void Start()
    {
        if (ChallengeManager.instance == null)
            Debug.LogError("This scene needs a Challenge Manager");
        else
            challengeManager = ChallengeManager.instance;
    }
}
