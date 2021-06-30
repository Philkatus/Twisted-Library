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
    [HideInInspector] public float componentCompletionTime;
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
            if (value == true && !challengeCompleted)
            {
                Debug.Log("Challenge complete!");
                gameObject.SendMessage("OnAllComponentsCompleted");
                landmark.CheckIfAllChallengesComplete();
                ObjectManager.instance.uILogic.OnChallengeComplete();
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
        if (componentCompletionTime != 0 && !challengeCompleted)
        {
            float timeSinceCompletion = Time.time - componentCompletionTime;
            foreach (ChallengeComponent component in components)
            {
                if (component.Completed)
                {
                    ObjectManager.instance.uILogic.UpdateComponentVisual(component.linkedUI, component.type, timeSinceCompletion, timeToCompleteComponents, false);
                }
            }

            if (timeSinceCompletion >= timeToCompleteComponents)
            {
                foreach (ChallengeComponent component in components)
                {
                    component.Completed = false;
                }
                ObjectManager.instance.uILogic.OnChallengeFailed();
                componentCompletionTime = 0;
                Debug.Log("Challenge failed!");
            }
        }
    }
}
