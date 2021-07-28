using System.Collections;
using System.Collections.Generic;
using UnityEngine.VFX;
using UnityEngine;

public class StartWaterfall : MonoBehaviour
{
    [SerializeField] GameObject waterFall;
    [SerializeField] GameObject waterbase;
    [SerializeField] GameObject water;

    void Start()
    {
        waterbase.SetActive(false);
    }

    public void StartTheWaterfall()
    {
        StartCoroutine(ObjectManager.instance.pSM.effects.TriggerStartWaterfall(27.48489f * 3, waterFall, waterbase));
    }
}
