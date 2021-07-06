using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleWindchimesUI : MonoBehaviour
{
    bool windchimeUIShown;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            if (!windchimeUIShown)
            {
                ChallengeManager.instance.windChimes.lerpScaleToBig = true;
                ChallengeManager.instance.windChimes.lerpScaleToSmall = false;
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Player")
        {
            if (windchimeUIShown)
            {
                ChallengeManager.instance.windChimes.lerpScaleToBig = false;
                ChallengeManager.instance.windChimes.lerpScaleToSmall = true;
            }
        }
    }
}
