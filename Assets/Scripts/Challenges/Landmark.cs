using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landmark : MonoBehaviour
{
    public List<Challenge> challenges = new List<Challenge>();
    public GameObject firstLinkedUI;
    public GameObject secondLinkedUI;
    public GameObject thirdLinkedUI;
    bool landmarkComplete;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
            ObjectManager.instance.uILogic.OnLandmarkComplete();
            landmarkComplete = true;
        }
    }

    public void ShowChallengeCompletionInUI(Challenge challenge)
    {
        int index = challenges.IndexOf(challenge);
        switch (index)
        {
            case 0:
                ObjectManager.instance.uILogic.OnChallengeComplete(firstLinkedUI);
                break;
            case 1:
                ObjectManager.instance.uILogic.OnChallengeComplete(secondLinkedUI);
                break;
            case 2:
                ObjectManager.instance.uILogic.OnChallengeComplete(thirdLinkedUI);
                break;
        }
    }
}
