using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchOnAfterSnap : MonoBehaviour
{
    public bool switchOn;
    public bool switchOff;
    public Quaternion snapRotation;
    public Quaternion railSnapRotation;
    public Transform pivot;
    public Transform railParent;

    [SerializeField] Transform pivotEnd;
    [SerializeField] Transform railParentEnd;
    [SerializeField] ChallengeComponent challengeComponent;
    Quaternion onRotation;
    Quaternion offRotation;
    Quaternion railOnRotation;
    Quaternion railOffRotation;
    float tSwitchOn;
    float tSwitchOff;
    bool doOncePerAttempt;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rail>().isASwitch = true;
        offRotation = pivot.rotation;
        onRotation = pivotEnd.rotation;
        railOnRotation = railParentEnd.rotation;
        railOffRotation = railParent.rotation;
        challengeComponent.onResetChallenge += new ChallengeComponent.EventHandler(SwitchOff);
        challengeComponent.type = "switch";
    }

    // Update is called once per frame
    void Update()
    {
        if (!challengeComponent.challenge.ChallengeCompleted)
        {
            if (switchOn)
            {
                if (!challengeComponent.challenge.challengeStarted && !doOncePerAttempt)
                {
                    foreach (ChallengeComponent component in challengeComponent.challenge.components)
                    {
                        ObjectManager.instance.uILogic.OnChallengeStartedComponent(component.linkedUI, challengeComponent.type);
                    }
                    doOncePerAttempt = true;
                }
                tSwitchOn += Time.deltaTime * 2;
                pivot.transform.rotation = Quaternion.Lerp(snapRotation, onRotation, tSwitchOn);
                railParent.transform.rotation = Quaternion.Lerp(railSnapRotation, railOnRotation, tSwitchOn);
                ObjectManager.instance.uILogic.UpdateComponentVisual(challengeComponent.linkedUI, challengeComponent.type, tSwitchOn, challengeComponent.challenge.timeToCompleteComponents, true);
                if (pivot.transform.rotation == onRotation)
                {
                    switchOn = false;
                    tSwitchOn = 0;
                    tSwitchOff = 0;
                    challengeComponent.Completed = true;
                }
            }
            if (switchOff)
            {
                switchOn = false;
                tSwitchOff += Time.deltaTime;
                pivot.transform.rotation = Quaternion.Lerp(onRotation, offRotation, tSwitchOff);
                railParent.transform.rotation = Quaternion.Lerp(railOnRotation, railOffRotation, tSwitchOff);
                if (pivot.transform.rotation == offRotation)
                {
                    switchOff = false;
                    tSwitchOff = 0;
                    tSwitchOn = 0;
                }
            }
        }
    }

    void SwitchOff()
    {
        switchOff = true;
        doOncePerAttempt = false;
    }
}
