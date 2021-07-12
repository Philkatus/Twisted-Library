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
    [HideInInspector] public float componentTimer;
    [HideInInspector] public bool challengeStarted;
    [HideInInspector] public bool waitToHideComponentsUI;
    public float timeToCompleteComponents;

    Landmark landmark;
    Landmark otherLandmark;
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
                gameObject.SendMessage("OnAllComponentsCompleted");
                landmark.CheckIfAllChallengesComplete();
                landmark.ShowChallengeCompletionInUI(this);
                componentTimer = 0;
                if (VoiceManager.Instance != null)
                {
                    VoiceManager.Instance.TryToSmallAchievementSound();
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
            otherLandmark = ChallengeManager.instance.windChimes;
            landmark = ChallengeManager.instance.volcano;
        }
        else if (linkedLandmark == LinkedLandmark.WindChimes)
        {
            otherLandmark = ChallengeManager.instance.volcano;
            landmark = ChallengeManager.instance.windChimes;
        }
    }

    void Update()
    {
        if (componentCompletionTime != 0 && !challengeCompleted)
        {
            float timeSinceCompletion = Time.time - componentCompletionTime;

            if (timeSinceCompletion >= timeToCompleteComponents)
            {
                foreach (ChallengeComponent component in components)
                {
                    component.Completed = false;
                    ObjectManager.instance.uILogic.OnChallengeFailed(component.linkedUI, component.type);
                }
                componentCompletionTime = 0;
                challengeStarted = false;
                waitToHideComponentsUI = true;
            }
            foreach (ChallengeComponent component in components)
            {
                if (component.Completed)
                {
                    ObjectManager.instance.uILogic.UpdateComponentVisual(component.linkedUI, component.type, timeSinceCompletion, timeToCompleteComponents, false);
                }
            }
        }

        if (componentCompletionTime != 0)
        {
            componentTimer = 0;
            waitToHideComponentsUI = false;
        }

        if (waitToHideComponentsUI)
        {
            componentTimer += Time.deltaTime;
            if (componentTimer >= 4)
            {
                foreach (ChallengeComponent component in components)
                {
                    ObjectManager.instance.uILogic.OnHideChallengeComponent(component.linkedUI, component.type);
                }
                waitToHideComponentsUI = false;
                componentTimer = 0;
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
            componentCompletionTime = 0;
            foreach (ChallengeComponent component in components)
            {
                ObjectManager.instance.uILogic.OnChallengeComplete(component.linkedUI, component.type);
                ObjectManager.instance.uILogic.OnHideChallengeComponent(component.linkedUI, component.type);
            }
        }
    }
}
