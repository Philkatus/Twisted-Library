using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientSoundsManager : MonoBehaviour
{
    Transform[] oceanSoundPositions;
    private void Awake()
    {
        foreach(Transform trans in oceanSoundPositions)
        {
            AudioManager.Instance.PlayRandom("Ocean", trans.position);
        }
    }
}
