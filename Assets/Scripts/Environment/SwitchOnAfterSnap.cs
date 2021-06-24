using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchOnAfterSnap : MonoBehaviour
{
    public bool switchOn;
    public bool switchOff;
    public bool isSwitchedOn;
    public Quaternion snapRotation;
    public Quaternion railSnapRotation;
    public Transform pivot;
    public Transform railParent;

    [SerializeField] Transform pivotEnd;
    [SerializeField] Transform railParentEnd;
    Quaternion onRotation;
    Quaternion offRotation;
    Quaternion railOnRotation;
    Quaternion railOffRotation;
    float tSwitchOn;
    float tSwitchOff;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rail>().isASwitch = true;
        offRotation = pivot.rotation;
        onRotation = pivotEnd.rotation;
        railOnRotation = railParentEnd.rotation;
        railOffRotation = railParent.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (switchOn)
        {
            tSwitchOn += Time.deltaTime;
            pivot.transform.rotation = Quaternion.Lerp(snapRotation, onRotation, tSwitchOn);
            railParent.transform.rotation = Quaternion.Lerp(railSnapRotation, railOnRotation, tSwitchOn);
            if (pivot.transform.rotation == onRotation)
            {
                switchOn = false;
                tSwitchOn = 0;
                tSwitchOff = 0;
                isSwitchedOn = true;
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
                isSwitchedOn = false;
            }
        }
    }
}
