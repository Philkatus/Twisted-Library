using UnityEngine;

public class ChangeRailVisualization : MonoBehaviour
{
    public GameObject g;
    public Material material;
    public Material changeToThisMaterial;
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
            g.GetComponent<MeshRenderer>().material = changeToThisMaterial;
            //material.SetColor("_EmissiveColor", Color.white * 1050);
            DynamicGI.UpdateEnvironment();
        }
        else
        {
            g.GetComponent<MeshRenderer>().material = material;
            //material.SetColor("_EmissiveColor", Color.black);
        }
    }
}
