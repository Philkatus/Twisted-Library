using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeManager : MonoBehaviour
{
    public static ChallengeManager instance;
    public Landmark windChimes;
    public Landmark volcano;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

interface ICentralObject
{
    void OnAllComponentsCompleted();
}
