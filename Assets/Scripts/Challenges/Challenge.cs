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
    [HideInInspector] public bool challengeStarted;
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
                landmark.ShowChallengeCompletionInUI(this);
                foreach (ChallengeComponent component in components)
                {
                    ObjectManager.instance.uILogic.OnChallengeCompleteComponent(component.linkedUI);
                }
            }
            challengeCompleted = value;
        }
    }

    void Start()
    {
        foreach (ChallengeComponent component in components)
        {
            component.challenge = this;
        }
        if (linkedLandmark == LinkedLandmark.Volcano)
        {
            landmark = ChallengeManager.instance.volcano;
        }
        else if (linkedLandmark == LinkedLandmark.WindChimes)
        {
            landmark = ChallengeManager.instance.windChimes;
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
                    ObjectManager.instance.uILogic.UpdateComponentVisual(component.linkedUI, "switch", timeSinceCompletion, timeToCompleteComponents, false);
                }
            }

            if (timeSinceCompletion >= timeToCompleteComponents)
            {
                foreach (ChallengeComponent component in components)
                {
                    component.Completed = false;
                    ObjectManager.instance.uILogic.OnChallengeFailed(component.linkedUI);
                }
                componentCompletionTime = 0;
                challengeStarted = false;
                Debug.Log("Challenge failed!");
            }
        }
    }

    public void CheckIfChallengeCompleted()
    {
        bool allComponentsComplete = true;
        foreach (ChallengeComponent component in components)
        {
            if (!component.Completed)
            {
                allComponentsComplete = false;
                break;
            }
        }
        if (allComponentsComplete)
        {
            ChallengeCompleted = true;
            foreach (ChallengeComponent component in components)
            {
                ObjectManager.instance.uILogic.OnChallengeCompleteComponent(component.linkedUI);
            }
        }
    }

    public void ShowCurrentLandmark()
    {
        ObjectManager.instance.uILogic.OnChallengeStartedLandmark(landmark.firstLinkedUI);
        ObjectManager.instance.uILogic.OnChallengeStartedLandmark(landmark.secondLinkedUI);
        ObjectManager.instance.uILogic.OnChallengeStartedLandmark(landmark.thirdLinkedUI);
    }
}
