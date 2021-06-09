using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeManager : MonoBehaviour
{
    
    public static ChallengeManager instance;

    //public List<Challenge> challenges;
    /*public struct Challenge
    {
        public List<bool> completedChallengeParts;
        public GameObject centralObject;
        public bool challengeCompleted;;
    }*/

    [HideInInspector] public List<bool> completedChallengeParts;
    bool challengeCompleted;
    [SerializeField] GameObject centralObject;
    [SerializeField] GameObject centralObject;
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!challengeCompleted)
        {
            bool allTrue = true;
            foreach (bool b in completedChallengeParts)
            {
                if (!b)
                    allTrue = false;
            }
            if (allTrue)
            {
                challengeCompleted = true;
                centralObject.GetComponent<Animator>().SetTrigger("Start");
                Debug.Log("true;");
            }
        }

    }
}

