using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landmark : MonoBehaviour
{
    public List<Challenge> challenges = new List<Challenge>();
    public GameObject firstLinkedUI;
    public GameObject secondLinkedUI;
    public GameObject thirdLinkedUI;
    public GameObject groundUI;
    [HideInInspector] public bool lerpScaleToBig;
    [HideInInspector] public bool lerpScaleToSmall;
    float timer;
    bool landmarkComplete;
    [SerializeField] bool isWindChimes;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (lerpScaleToSmall)
        {
            timer += Time.deltaTime;
            ObjectManager.instance.uILogic.SetLandmarkScaleToSmall(firstLinkedUI, secondLinkedUI, thirdLinkedUI, groundUI, timer, isWindChimes);
            if (timer >= 1)
            {
                lerpScaleToSmall = false;
                timer = 0;
            }
        }
        else if (lerpScaleToBig)
        {
            timer += Time.deltaTime;
            ObjectManager.instance.uILogic.SetLandmarkScaleToBig(firstLinkedUI, secondLinkedUI, thirdLinkedUI, groundUI, timer, isWindChimes);
            if (timer >= 1)
            {
                lerpScaleToBig = false;
                timer = 0;
            }
        }
    }

    public void CheckIfAllChallengesComplete()
    {
        bool allChallengesComplete = true;
        foreach (Challenge challenge in challenges)
        {
            if (!challenge.ChallengeCompleted)
            {
                allChallengesComplete = false;
                break;
            }
        }
        if (allChallengesComplete)
        {
            ObjectManager.instance.uILogic.OnLandmarkComplete(groundUI);
            landmarkComplete = true;
            if (VoiceManager.Instance != null) 
            {
                VoiceManager.Instance.TryToAchievementSound();
            }
        }
    }

    public void ShowChallengeCompletionInUI(Challenge challenge)
    {
        int index = challenges.IndexOf(challenge);
        switch (index)
        {
            case 0:
                ObjectManager.instance.uILogic.OnChallengeCompleteLandmark(firstLinkedUI);
                break;
            case 1:
                ObjectManager.instance.uILogic.OnChallengeCompleteLandmark(secondLinkedUI);
                break;
            case 2:
                ObjectManager.instance.uILogic.OnChallengeCompleteLandmark(thirdLinkedUI);
                break;
        }
    }

    public void ShowLandmarkUI()
    {
        ObjectManager.instance.uILogic.ShowLandmarkUI(firstLinkedUI, secondLinkedUI, thirdLinkedUI, groundUI);
    }
}
