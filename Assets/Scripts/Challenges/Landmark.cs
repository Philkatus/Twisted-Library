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
    public bool landmarkComplete;
    [SerializeField] bool isWindChimes;
    [SerializeField] int landmarkNo;
    PlayerMovementStateMachine psm;

    // Start is called before the first frame update
    void Start()
    {
        psm = ObjectManager.instance.pSM;
        psm.allLandmarks[landmarkNo] = transform.position;
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
            if (psm.nextLandmarkNo == landmarkNo)
            {
                psm.nextLandmarkNo++;
                psm.activatedLandmarkNos.Add(landmarkNo);
            }

            if (VoiceManager.Instance != null)
            {
                VoiceManager.Instance.TryToAchievementSound();
            }
            if (ChallengeManager.instance.volcano.landmarkComplete && ChallengeManager.instance.windChimes.landmarkComplete)
            {
                ObjectManager.instance.uILogic.StartCoroutine(ObjectManager.instance.uILogic.ShowCredits());
            }
        }
    }

    public void ShowChallengeCompletionInUI(Challenge challenge)
    {
        int index = challenges.IndexOf(challenge);
        switch (index)
        {
            case 0:
                ObjectManager.instance.uILogic.OnChallengeCompleteLandmark(firstLinkedUI, 0);
                break;
            case 1:
                ObjectManager.instance.uILogic.OnChallengeCompleteLandmark(secondLinkedUI, 1);
                break;
            case 2:
                ObjectManager.instance.uILogic.OnChallengeCompleteLandmark(thirdLinkedUI, 2);
                break;
        }
    }

    public void ShowLandmarkUI()
    {
        ObjectManager.instance.uILogic.ShowLandmarkUI(firstLinkedUI, secondLinkedUI, thirdLinkedUI, groundUI);
    }
}
