using UnityEngine;

public class ChangeRailVisualization : MonoBehaviour
{
    public GameObject g;
    public Material material;
    RailSearchManager railAllocator;
    Rail thisRail;

    void Start()
    {
        g = this.gameObject;
        material = g.GetComponent<MeshRenderer>().material;
        material.EnableKeyword("_EMISSION");
        railAllocator = RailSearchManager.instance;
        thisRail = transform.parent.GetComponent<Rail>();
    }

    void Update()
    {
        if (railAllocator.currentRailVisual == thisRail)
        {
            material.SetColor("_EmissiveColor", Color.white * 1050);
            DynamicGI.UpdateEnvironment();
        }
        else
        {
            material.SetColor("_EmissiveColor", Color.black);
        }
    }
}
