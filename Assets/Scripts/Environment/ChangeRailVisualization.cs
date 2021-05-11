using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeRailVisualization : MonoBehaviour
{
    public GameObject g;
    public Material material;
    public bool closestRail = false;

    void Start()
    {
        g = this.gameObject;
        material = g.GetComponent<MeshRenderer>().material;
        material.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        if (closestRail)
        {
            material.SetColor("_EmissionColor", Color.white * 3);
            DynamicGI.UpdateEnvironment();
        }
        else
        {
            material.SetColor("_EmissionColor", Color.black);
        }
    }
}
