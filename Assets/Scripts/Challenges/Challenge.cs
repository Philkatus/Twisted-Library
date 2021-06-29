using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Challenge : MonoBehaviour
{
    public enum LinkedLandmark
    {
        Volcano, WindChimes
    }

    public LinkedLandmark linkedLandmark;
    public List<ChallengeComponent> components = new List<ChallengeComponent>();
    public float componentCompletionTime;
    public float timeToCompleteComponents;

    Landmark landmark;
    bool challengeCompleted;
    public bool ChallengeCompleted
    {
        get
        {
            return challengeCompleted;
        }
        set
        {
            if (value == true)
            {
                Debug.Log("Challenge complete!");
                gameObject.SendMessage("OnAllComponentsCompleted");
                landmark.CheckIfAllChallengesComplete();
            }
            challengeCompleted = value;
        }
    }

    void Start()
    {
        if (linkedLandmark == LinkedLandmark.Volcano)
        {
            landmark = ChallengeManager.instance.volcano;
            landmark.challenges.Add(this);
        }
        else if (linkedLandmark == LinkedLandmark.WindChimes)
        {
            landmark = ChallengeManager.instance.windChimes;
            landmark.challenges.Add(this);
        }
        foreach (ChallengeComponent component in components)
        {
            component.challenge = this;
        }
    }

    void Update()
    {
        if (componentCompletionTime != 0)
        {
            if (componentCompletionTime + timeToCompleteComponents <= Time.time)
            {
                foreach (ChallengeComponent component in components)
                {
                    component.Completed = false;
                }
                componentCompletionTime = 0;
                Debug.Log("Challenge failed!");
            }
        }
    }
}
