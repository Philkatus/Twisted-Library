using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableUITrigger : MonoBehaviour
{
    bool uiShown;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player" && !uiShown)
        {
            ChallengeManager.instance.volcano.ShowLandmarkUI();
            //ChallengeManager.instance.windChimes.ShowLandmarkUI();
            uiShown = true;
        }
    }
}
