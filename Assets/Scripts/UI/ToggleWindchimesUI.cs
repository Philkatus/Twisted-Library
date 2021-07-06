using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleWindchimesUI : MonoBehaviour
{
    bool winchimeUIShown;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            if (winchimeUIShown)
            {
                ChallengeManager.instance.windChimes.lerpScaleToBig = false;
                ChallengeManager.instance.windChimes.lerpScaleToSmall = true;
            }
            else
            {
                ChallengeManager.instance.windChimes.lerpScaleToBig = true;
                ChallengeManager.instance.windChimes.lerpScaleToSmall = false;
            }
        }
    }
}
