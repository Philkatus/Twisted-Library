using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landmark : MonoBehaviour
{
    public List<Challenge> challenges = new List<Challenge>();
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
            landmarkComplete = true;
        }
    }
}
