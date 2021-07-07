using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleVolcanoUI : MonoBehaviour
{
    bool volcanoUIShown = true;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            if (!volcanoUIShown)
            {
                ChallengeManager.instance.volcano.lerpScaleToBig = true;
                ChallengeManager.instance.volcano.lerpScaleToSmall = false;
                volcanoUIShown = true;
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Player")
        {
            if (volcanoUIShown)
            {
                ChallengeManager.instance.volcano.lerpScaleToBig = false;
                ChallengeManager.instance.volcano.lerpScaleToSmall = true;
                volcanoUIShown = false;
            }
        }
    }
}
