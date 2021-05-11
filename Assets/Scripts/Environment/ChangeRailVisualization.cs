using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeRailVisualization : MonoBehaviour
{
    public GameObject g;
    public Material material;
    public bool isClosestRail = false;

    void Start()
    {
        g = this.gameObject;
        material = g.GetComponent<MeshRenderer>().material;
        material.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        if (isClosestRail)
        {
            material.SetColor("_EmissionColor", Color.white * 3);
            material.SetColor("_Color", Color.black);
            Debug.Log("dfhjfkjdgjhfd");
            DynamicGI.UpdateEnvironment();
        }
        else
        {
            material.SetColor("_EmissionColor", Color.black);
        }
    }
}
