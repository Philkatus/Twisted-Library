using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchOnAfterSnap : MonoBehaviour
{
    public bool switchOn;
    public bool switchOff;
    public bool isSwitchedOn;
    public Quaternion snapRotation;
    public Transform pivot;

    [SerializeField] Transform pivotEnd;
    Quaternion onRotation;
    Quaternion offRotation;
    float tSwitchOn;
    float tSwitchOff;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rail>().isASwitch = true;
        offRotation = pivot.rotation;
        onRotation = pivotEnd.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (switchOn)
        {
            Debug.Log("tOn:" + tSwitchOn);
            tSwitchOn += Time.deltaTime;
            pivot.transform.rotation = Quaternion.Lerp(snapRotation, onRotation, tSwitchOn);
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
            Debug.Log("tOff:" + tSwitchOff);
            switchOn = false;
            tSwitchOff += Time.deltaTime;
            pivot.transform.rotation = Quaternion.Lerp(onRotation, offRotation, tSwitchOff);
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
