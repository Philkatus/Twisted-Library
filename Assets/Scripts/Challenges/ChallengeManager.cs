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
    [SerializeField] GameObject wallObject;
    [SerializeField] GameObject secondWall;
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
                StartCoroutine(RotateWheel(centralObject));
                StartCoroutine(MoveWall(wallObject, wallObject.transform.right, 10f));
                StartCoroutine(MoveWall(secondWall, -secondWall.transform.right, 5f));
            }
        }

    }
    IEnumerator RotateWheel(GameObject wheel)
    {
        while (true)
        {
            float angle = 10 * Time.deltaTime;
            wheel.transform.eulerAngles = new Vector3(wheel.transform.eulerAngles.x, wheel.transform.eulerAngles.y + angle, wheel.transform.eulerAngles.z);
            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator MoveWall (GameObject wall, Vector3 direction, float time)
    {
        float timer = 0;
        while (timer < time)
        {
            timer += Time.deltaTime;
            wall.transform.position += direction.normalized;
            yield return new WaitForEndOfFrame();
        }
    }
}

